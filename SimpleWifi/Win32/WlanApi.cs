using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Threading;
using System.Text;
using SimpleWifi.Win32.Interop;
using SimpleWifi.Win32.Helpers;

namespace SimpleWifi.Win32
{
	/// <summary>
	/// Represents a client to the Zeroconf (Native Wifi) service.
	/// </summary>
	/// <remarks>
	/// This class is the entrypoint to Native Wifi management. To manage WiFi settings, create an instance
	/// of this class.
	/// </remarks>
	public class WlanClient
	{
		internal IntPtr clientHandle;
		internal uint negotiatedVersion;
		internal WlanInterop.WlanNotificationCallbackDelegate wlanNotificationCallback;

		private Dictionary<Guid,WlanInterface> ifaces = new Dictionary<Guid,WlanInterface>();

		/// <summary>
		/// Creates a new instance of a Native Wifi service client.
		/// Throws Win32 errors: ERROR_INVALID_PARAMETER, ERROR_NOT_ENOUGH_MEMORY, RPC_STATUS, ERROR_REMOTE_SESSION_LIMIT_EXCEEDED.
		/// </summary>
		public WlanClient()
		{
			// OK!
			WlanInterop.ThrowIfError(WlanInterop.WlanOpenHandle(WlanInterop.WLAN_CLIENT_VERSION_XP_SP2, IntPtr.Zero, out negotiatedVersion, out clientHandle));

			try
			{
				// Interop callback
				wlanNotificationCallback = new WlanInterop.WlanNotificationCallbackDelegate(OnWlanNotification);

				WlanNotificationSource prevSrc;
				WlanInterop.ThrowIfError(WlanInterop.WlanRegisterNotification(clientHandle, WlanNotificationSource.All, false, wlanNotificationCallback, IntPtr.Zero, IntPtr.Zero, out prevSrc));
			}
			catch
			{
				WlanInterop.WlanCloseHandle(clientHandle, IntPtr.Zero);
				throw;
			}
		}
		
		~WlanClient()
		{
			// Free the handle when deconstructing the client.
			WlanInterop.WlanCloseHandle(clientHandle, IntPtr.Zero);
		}

		// Called from interop
		private void OnWlanNotification(ref WlanNotificationData notifyData, IntPtr context)
		{
			WlanInterface wlanIface = ifaces.ContainsKey(notifyData.interfaceGuid) ? ifaces[notifyData.interfaceGuid] : null;

			switch(notifyData.notificationSource)
			{
				case WlanNotificationSource.ACM:
					switch((WlanNotificationCodeAcm)notifyData.notificationCode)
					{
						case WlanNotificationCodeAcm.ConnectionStart:
						case WlanNotificationCodeAcm.ConnectionComplete:
						case WlanNotificationCodeAcm.ConnectionAttemptFail:
						case WlanNotificationCodeAcm.Disconnecting:
						case WlanNotificationCodeAcm.Disconnected:
							WlanConnectionNotificationData? connNotifyData = WlanHelpers.ParseWlanConnectionNotification(ref notifyData);

							if (connNotifyData.HasValue && wlanIface != null)
								wlanIface.OnWlanConnection(notifyData, connNotifyData.Value);

							break;
						case WlanNotificationCodeAcm.ScanFail:
							int expectedSize = Marshal.SizeOf(typeof (WlanReasonCode));

							if (notifyData.dataSize >= expectedSize)
							{
								WlanReasonCode reasonCode = (WlanReasonCode) Marshal.ReadInt32(notifyData.dataPtr);

								if (wlanIface != null)
									wlanIface.OnWlanReason(notifyData, reasonCode);
							}
							break;
					}
					break;
				case WlanNotificationSource.MSM:
					switch((WlanNotificationCodeMsm)notifyData.notificationCode)
					{
						case WlanNotificationCodeMsm.Associating:
						case WlanNotificationCodeMsm.Associated:
						case WlanNotificationCodeMsm.Authenticating:
						case WlanNotificationCodeMsm.Connected:
						case WlanNotificationCodeMsm.RoamingStart:
						case WlanNotificationCodeMsm.RoamingEnd:
						case WlanNotificationCodeMsm.Disassociating:
						case WlanNotificationCodeMsm.Disconnected:
						case WlanNotificationCodeMsm.PeerJoin:
						case WlanNotificationCodeMsm.PeerLeave:
						case WlanNotificationCodeMsm.AdapterRemoval:
							WlanConnectionNotificationData? connNotifyData = WlanHelpers.ParseWlanConnectionNotification(ref notifyData);
							
							if (connNotifyData.HasValue && wlanIface != null)
									wlanIface.OnWlanConnection(notifyData, connNotifyData.Value);

							break;
					}
					break;
			}
			
			if (wlanIface != null)
				wlanIface.OnWlanNotification(notifyData);
		}

		/// <summary>
		/// Gets the WLAN interfaces.
		/// 
		/// Possible Win32 exceptions:
		/// 
		/// ERROR_INVALID_PARAMETER: A parameter is incorrect. This error is returned if the hClientHandle or ppInterfaceList parameter is NULL. This error is returned if the pReserved is not NULL. This error is also returned if the hClientHandle parameter is not valid.
		/// ERROR_INVALID_HANDLE: The handle hClientHandle was not found in the handle table.
		/// RPC_STATUS: Various error codes.
		/// ERROR_NOT_ENOUGH_MEMORY: Not enough memory is available to process this request and allocate memory for the query results.
		/// </summary>
		/// <value>The WLAN interfaces.</value>
		public WlanInterface[] Interfaces
		{
			get
			{
				IntPtr ifaceList;

				WlanInterop.ThrowIfError(WlanInterop.WlanEnumInterfaces(clientHandle, IntPtr.Zero, out ifaceList)); 

				try
				{
					WlanInterfaceInfoListHeader header = (WlanInterfaceInfoListHeader) Marshal.PtrToStructure(ifaceList, typeof (WlanInterfaceInfoListHeader));
					
					Int64 listIterator = ifaceList.ToInt64() + Marshal.SizeOf(header);
					WlanInterface[] interfaces = new WlanInterface[header.numberOfItems];
					List<Guid> currentIfaceGuids = new List<Guid>();

					for (int i = 0; i < header.numberOfItems; ++i)
					{
						WlanInterfaceInfo info = (WlanInterfaceInfo) Marshal.PtrToStructure(new IntPtr(listIterator), typeof(WlanInterfaceInfo));

						listIterator += Marshal.SizeOf(info);
						currentIfaceGuids.Add(info.interfaceGuid);

						WlanInterface wlanIface;
						if (ifaces.ContainsKey(info.interfaceGuid))
							wlanIface = ifaces[info.interfaceGuid];
						else
							wlanIface = new WlanInterface(this, info);

						interfaces[i] = wlanIface;
						ifaces[info.interfaceGuid] = wlanIface;
					}

					// Remove stale interfaces
					Queue<Guid> deadIfacesGuids = new Queue<Guid>();
					foreach (Guid ifaceGuid in ifaces.Keys)
					{
						if (!currentIfaceGuids.Contains(ifaceGuid))
							deadIfacesGuids.Enqueue(ifaceGuid);
					}

					while(deadIfacesGuids.Count != 0)
					{
						Guid deadIfaceGuid = deadIfacesGuids.Dequeue();
						ifaces.Remove(deadIfaceGuid);						
					}

					return interfaces;
				}
				finally
				{
					WlanInterop.WlanFreeMemory(ifaceList);
				}
			}
		}
	}
}
