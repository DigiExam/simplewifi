using System;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace SimpleWifi.Win32.Interop
{
	// TODO: Separate the functions and the structs/enums. Many of the structs/enums should remain public
	// (since they're reused in the OOP interfaces) -- the rest (including all P/Invoke function mappings)
	// should become internal.

	// All structures which native methods rely on should be kept in the Wlan class.
	// Only change the layout of those structures if it matches the native API documentation.
	// Some structures might have helper properties but adding or changing fields is prohibited.
	// This class is not documented since all the documentation resides in the MSDN. The code
	// documentation only covers details which concern interop users.
	// Some identifier names were modified to correspond to .NET naming conventions
	// but otherwise retain their native meaning.

	/// <summary>
	/// Defines the Native Wifi API through P/Invoke interop.
	/// </summary>
	/// <remarks>
	/// This class is intended for internal use. Use the <see cref="WlanCliient"/> class instead.
	/// </remarks>
	internal static class WlanInterop
	{
		#region P/Invoke API

		public const uint WLAN_CLIENT_VERSION_XP_SP2 = 1;
		public const uint WLAN_CLIENT_VERSION_LONGHORN = 2;

		[DllImport("wlanapi.dll")]
		public static extern int WlanOpenHandle(
			[In] UInt32 clientVersion,
			[In, Out] IntPtr pReserved,
			[Out] out UInt32 negotiatedVersion,
			[Out] out IntPtr clientHandle);

		[DllImport("wlanapi.dll")]
		public static extern int WlanCloseHandle(
			[In] IntPtr clientHandle,
			[In, Out] IntPtr pReserved);

		[DllImport("wlanapi.dll")]
		public static extern int WlanEnumInterfaces(
			[In] IntPtr clientHandle,
			[In, Out] IntPtr pReserved,
			[Out] out IntPtr ppInterfaceList);

		[DllImport("wlanapi.dll")]
		public static extern int WlanQueryInterface(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] WlanIntfOpcode opCode,
			[In, Out] IntPtr pReserved,
			[Out] out int dataSize,
			[Out] out IntPtr ppData,
			[Out] out WlanOpcodeValueType wlanOpcodeValueType);

		[DllImport("wlanapi.dll")]
		public static extern int WlanSetInterface(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] WlanIntfOpcode opCode,
			[In] uint dataSize,
			[In] IntPtr pData,
			[In, Out] IntPtr pReserved);

		/// <param name="pDot11Ssid">Not supported on Windows XP SP2: must be a <c>null</c> reference.</param>
		/// <param name="pIeData">Not supported on Windows XP SP2: must be a <c>null</c> reference.</param>
		[DllImport("wlanapi.dll")]
		public static extern int WlanScan(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] IntPtr pDot11Ssid,
			[In] IntPtr pIeData,
			[In, Out] IntPtr pReserved);

		[DllImport("wlanapi.dll")]
		public static extern int WlanGetAvailableNetworkList(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] WlanGetAvailableNetworkFlags flags,
			[In, Out] IntPtr reservedPtr,
			[Out] out IntPtr availableNetworkListPtr);


		[DllImport("wlanapi.dll")]
		public static extern int WlanSetProfile(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] WlanProfileFlags flags,
			[In, MarshalAs(UnmanagedType.LPWStr)] string profileXml,
			[In, Optional, MarshalAs(UnmanagedType.LPWStr)] string allUserProfileSecurity,
			[In] bool overwrite,
			[In] IntPtr pReserved,
			[Out] out WlanReasonCode reasonCode);

		/// <param name="flags">Not supported on Windows XP SP2: must be a <c>null</c> reference.</param>
		[DllImport("wlanapi.dll")]
		public static extern int WlanGetProfile(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In, MarshalAs(UnmanagedType.LPWStr)] string profileName,
			[In] IntPtr pReserved,
			[Out] out IntPtr profileXml,
			[Out, Optional] out WlanProfileFlags flags,
			[Out, Optional] out WlanAccess grantedAccess);

		[DllImport("wlanapi.dll")]
		public static extern int WlanGetProfileList(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] IntPtr pReserved,
			[Out] out IntPtr profileList
		);

		[DllImport("wlanapi.dll")]
		public static extern void WlanFreeMemory(IntPtr pMemory);

		[DllImport("wlanapi.dll")]
		public static extern int WlanReasonCodeToString(
			[In] WlanReasonCode reasonCode,
			[In] int bufferSize,
			[In, Out] StringBuilder stringBuffer,
			IntPtr pReserved
		);		

		/// <summary>
		/// Defines the callback function which accepts WLAN notifications.
		/// </summary>
		public delegate void WlanNotificationCallbackDelegate(ref WlanNotificationData notificationData, IntPtr context);

		[DllImport("wlanapi.dll")]
		public static extern int WlanRegisterNotification(
			[In] IntPtr clientHandle,
			[In] WlanNotificationSource notifSource,
			[In] bool ignoreDuplicate,
			[In] WlanNotificationCallbackDelegate funcCallback,
			[In] IntPtr callbackContext,
			[In] IntPtr reserved,
			[Out] out WlanNotificationSource prevNotifSource);
		
		[DllImport("wlanapi.dll")]
		public static extern int WlanConnect(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] ref WlanConnectionParameters connectionParameters,
			IntPtr pReserved);

		[DllImport("wlanapi.dll")]
		public static extern int WlanDeleteProfile(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In, MarshalAs(UnmanagedType.LPWStr)] string profileName,
			IntPtr reservedPtr
		);

		[DllImport("wlanapi.dll")]
		public static extern int WlanDisconnect(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			IntPtr pReserved);

		[DllImport("wlanapi.dll")]
		public static extern int WlanGetNetworkBssList(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] IntPtr dot11SsidInt,
			[In] Dot11BssType dot11BssType,
			[In] bool securityEnabled,
			IntPtr reservedPtr,
			[Out] out IntPtr wlanBssList
		);
		
		#endregion

		/// <summary>
		/// Helper method to wrap calls to Native WiFi API methods.
		/// If the method falls, throws an exception containing the error code.
		/// </summary>
		/// <param name="win32ErrorCode">The error code.</param>
		[DebuggerStepThrough]
		internal static void ThrowIfError(int win32ErrorCode)
		{
			if (win32ErrorCode != 0)
				throw new Win32Exception(win32ErrorCode);
		}
	}
}
