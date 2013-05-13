using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleWifi.Win32.Interop
{

	/// <summary>
	/// The header of an array of information about available networks.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct WlanAvailableNetworkListHeader
	{
		/// <summary>
		/// Contains the number of <see cref=""/> items following the header.
		/// </summary>
		public uint numberOfItems;
		/// <summary>
		/// The index of the current item. The index of the first item is 0.
		/// </summary>
		public uint index;
	}

	/// <summary>
	/// Contains information about an available wireless network.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct WlanAvailableNetwork
	{
		/// <summary>
		/// Contains the profile name associated with the network.
		/// If the network doesn't have a profile, this member will be empty.
		/// If multiple profiles are associated with the network, there will be multiple entries with the same SSID in the visible network list. Profile names are case-sensitive.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string profileName;
		/// <summary>
		/// Contains the SSID of the visible wireless network.
		/// </summary>
		public Dot11Ssid dot11Ssid;
		/// <summary>
		/// Specifies whether the network is infrastructure or ad hoc.
		/// </summary>
		public Dot11BssType dot11BssType;
		/// <summary>
		/// Indicates the number of BSSIDs in the network.
		/// </summary>
		public uint numberOfBssids;
		/// <summary>
		/// Indicates whether the network is connectable or not.
		/// </summary>
		public bool networkConnectable;
		/// <summary>
		/// Indicates why a network cannot be connected to. This member is only valid when <see cref="networkConnectable"/> is <c>false</c>.
		/// </summary>
		public WlanReasonCode wlanNotConnectableReason;
		/// <summary>
		/// The number of PHY types supported on available networks.
		/// The maximum value of this field is 8. If more than 8 PHY types are supported, <see cref="morePhyTypes"/> must be set to <c>true</c>.
		/// </summary>
		private uint numberOfPhyTypes;
		/// <summary>
		/// Contains an array of <see cref="Dot11PhyType"/> values that represent the PHY types supported by the available networks.
		/// When <see cref="numberOfPhyTypes"/> is greater than 8, this array contains only the first 8 PHY types.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		private Dot11PhyType[] dot11PhyTypes;
		/// <summary>
		/// Gets the <see cref="Dot11PhyType"/> values that represent the PHY types supported by the available networks.
		/// </summary>
		public Dot11PhyType[] Dot11PhyTypes
		{
			get
			{
				Dot11PhyType[] ret = new Dot11PhyType[numberOfPhyTypes];
				Array.Copy(dot11PhyTypes, ret, numberOfPhyTypes);
				return ret;
			}
		}
		/// <summary>
		/// Specifies if there are more than 8 PHY types supported.
		/// When this member is set to <c>true</c>, an application must call <see cref="WlanClient.WlanInterface.GetNetworkBssList"/> to get the complete list of PHY types.
		/// <see cref="WlanBssEntry.phyId"/> contains the PHY type for an entry.
		/// </summary>
		public bool morePhyTypes;
		/// <summary>
		/// A percentage value that represents the signal quality of the network.
		/// This field contains a value between 0 and 100.
		/// A value of 0 implies an actual RSSI signal strength of -100 dbm.
		/// A value of 100 implies an actual RSSI signal strength of -50 dbm.
		/// You can calculate the RSSI signal strength value for values between 1 and 99 using linear interpolation.
		/// </summary>
		public uint wlanSignalQuality;
		/// <summary>
		/// Indicates whether security is enabled on the network.
		/// </summary>
		public bool securityEnabled;
		/// <summary>
		/// Indicates the default authentication algorithm used to join this network for the first time.
		/// </summary>
		public Dot11AuthAlgorithm dot11DefaultAuthAlgorithm;
		/// <summary>
		/// Indicates the default cipher algorithm to be used when joining this network.
		/// </summary>
		public Dot11CipherAlgorithm dot11DefaultCipherAlgorithm;
		/// <summary>
		/// Contains various flags for the network.
		/// </summary>
		public WlanAvailableNetworkFlags flags;
		/// <summary>
		/// Reserved for future use. Must be set to NULL.
		/// </summary>
		uint reserved;
	}

	/// <summary>
	/// Contains information provided when registering for notifications.
	/// </summary>
	/// <remarks>
	/// Corresponds to the native <c>WLAN_NOTIFICATION_DATA</c> type.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	public struct WlanNotificationData
	{
		/// <summary>
		/// Specifies where the notification comes from.
		/// </summary>
		/// <remarks>
		/// On Windows XP SP2, this field must be set to <see cref="WlanNotificationSource.None"/>, <see cref="WlanNotificationSource.All"/> or <see cref="WlanNotificationSource.ACM"/>.
		/// </remarks>
		public WlanNotificationSource notificationSource;
		/// <summary>
		/// Indicates the type of notification. The value of this field indicates what type of associated data will be present in <see cref="dataPtr"/>.
		/// </summary>
		public int notificationCode;
		/// <summary>
		/// Indicates which interface the notification is for.
		/// </summary>
		public Guid interfaceGuid;
		/// <summary>
		/// Specifies the size of <see cref="dataPtr"/>, in bytes.
		/// </summary>
		public int dataSize;
		/// <summary>
		/// Pointer to additional data needed for the notification, as indicated by <see cref="notificationCode"/>.
		/// </summary>
		public IntPtr dataPtr;

		/// <summary>
		/// Gets the notification code (in the correct enumeration type) according to the notification source.
		/// </summary>
		public object NotificationCode
		{
			get
			{
				if (notificationSource == WlanNotificationSource.MSM)
					return (WlanNotificationCodeMsm)notificationCode;
				else if (notificationSource == WlanNotificationSource.ACM)
					return (WlanNotificationCodeAcm)notificationCode;
				else
					return notificationCode;
			}

		}
	}


	/// <summary>
	/// Specifies the parameters used when using the <see cref="WlanConnect"/> function.
	/// </summary>
	/// <remarks>
	/// Corresponds to the native <c>WLAN_CONNECTION_PARAMETERS</c> type.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	public struct WlanConnectionParameters
	{
		/// <summary>
		/// Specifies the mode of connection.
		/// </summary>
		public WlanConnectionMode wlanConnectionMode;
		/// <summary>
		/// Specifies the profile being used for the connection.
		/// The contents of the field depend on the <see cref="wlanConnectionMode"/>:
		/// <list type="table">
		/// <listheader>
		/// <term>Value of <see cref="wlanConnectionMode"/></term>
		/// <description>Contents of the profile string</description>
		/// </listheader>
		/// <item>
		/// <term><see cref="WlanConnectionMode.Profile"/></term>
		/// <description>The name of the profile used for the connection.</description>
		/// </item>
		/// <item>
		/// <term><see cref="WlanConnectionMode.TemporaryProfile"/></term>
		/// <description>The XML representation of the profile used for the connection.</description>
		/// </item>
		/// <item>
		/// <term><see cref="WlanConnectionMode.DiscoverySecure"/>, <see cref="WlanConnectionMode.DiscoveryUnsecure"/> or <see cref="WlanConnectionMode.Auto"/></term>
		/// <description><c>null</c></description>
		/// </item>
		/// </list>
		/// </summary>
		[MarshalAs(UnmanagedType.LPWStr)]
		public string profile;
		/// <summary>
		/// Pointer to a <see cref="Dot11Ssid"/> structure that specifies the SSID of the network to connect to.
		/// This field is optional. When set to <c>null</c>, all SSIDs in the profile will be tried.
		/// This field must not be <c>null</c> if <see cref="wlanConnectionMode"/> is set to <see cref="WlanConnectionMode.DiscoverySecure"/> or <see cref="WlanConnectionMode.DiscoveryUnsecure"/>.
		/// </summary>
		public IntPtr dot11SsidPtr;
		/// <summary>
		/// Pointer to a <see cref="Dot11BssidList"/> structure that contains the list of basic service set (BSS) identifiers desired for the connection.
		/// </summary>
		/// <remarks>
		/// On Windows XP SP2, must be set to <c>null</c>.
		/// </remarks>
		public IntPtr desiredBssidListPtr;
		/// <summary>
		/// A <see cref="Dot11BssType"/> value that indicates the BSS type of the network. If a profile is provided, this BSS type must be the same as the one in the profile.
		/// </summary>
		public Dot11BssType dot11BssType;
		/// <summary>
		/// Specifies ocnnection parameters.
		/// </summary>
		/// <remarks>
		/// On Windows XP SP2, must be set to 0.
		/// </remarks>
		public WlanConnectionFlags flags;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct WlanBssListHeader
	{
		internal uint totalSize;
		internal uint numberOfItems;
	}
	
	/// <summary>
	/// Contains information about a basic service set (BSS).
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct WlanBssEntry
	{
		/// <summary>
		/// Contains the SSID of the access point (AP) associated with the BSS.
		/// </summary>
		public Dot11Ssid dot11Ssid;
		/// <summary>
		/// The identifier of the PHY on which the AP is operating.
		/// </summary>
		public uint phyId;
		/// <summary>
		/// Contains the BSS identifier.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public byte[] dot11Bssid;
		/// <summary>
		/// Specifies whether the network is infrastructure or ad hoc.
		/// </summary>
		public Dot11BssType dot11BssType;
		public Dot11PhyType dot11BssPhyType;
		/// <summary>
		/// The received signal strength in dBm.
		/// </summary>
		public int rssi;
		/// <summary>
		/// The link quality reported by the driver. Ranges from 0-100.
		/// </summary>
		public uint linkQuality;
		/// <summary>
		/// If 802.11d is not implemented, the network interface card (NIC) must set this field to TRUE. If 802.11d is implemented (but not necessarily enabled), the NIC must set this field to TRUE if the BSS operation complies with the configured regulatory domain.
		/// </summary>
		public bool inRegDomain;
		/// <summary>
		/// Contains the beacon interval value from the beacon packet or probe response.
		/// </summary>
		public ushort beaconPeriod;
		/// <summary>
		/// The timestamp from the beacon packet or probe response.
		/// </summary>
		public ulong timestamp;
		/// <summary>
		/// The host timestamp value when the beacon or probe response is received.
		/// </summary>
		public ulong hostTimestamp;
		/// <summary>
		/// The capability value from the beacon packet or probe response.
		/// </summary>
		public ushort capabilityInformation;
		/// <summary>
		/// The frequency of the center channel, in kHz.
		/// </summary>
		public uint chCenterFrequency;
		/// <summary>
		/// Contains the set of data transfer rates supported by the BSS.
		/// </summary>
		public WlanRateSet wlanRateSet;
		/// <summary>
		/// Offset of the information element (IE) data blob.
		/// </summary>
		public uint ieOffset;
		/// <summary>
		/// Size of the IE data blob, in bytes.
		/// </summary>
		public uint ieSize;
	}

	/// <summary>
	/// Contains the set of supported data rates.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct WlanRateSet
	{
		/// <summary>
		/// The length, in bytes, of <see cref="rateSet"/>.
		/// </summary>
		private uint rateSetLength;
		/// <summary>
		/// An array of supported data transfer rates.
		/// If the rate is a basic rate, the first bit of the rate value is set to 1.
		/// A basic rate is the data transfer rate that all stations in a basic service set (BSS) can use to receive frames from the wireless medium.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 126)]
		private ushort[] rateSet;

		public ushort[] Rates
		{
			get
			{
				ushort[] rates = new ushort[rateSetLength / sizeof(ushort)];
				Array.Copy(rateSet, rates, rates.Length);
				return rates;
			}
		}

		/// <summary>
		/// CalculateS the data transfer rate in Mbps for an arbitrary supported rate.
		/// </summary>
		/// <param name="rate"></param>
		/// <returns></returns>
		public double GetRateInMbps(int rate)
		{
			return (rateSet[rate] & 0x7FFF) * 0.5;
		}
	}


	/// <summary>
	/// Contains information about connection related notifications.
	/// </summary>
	/// <remarks>
	/// Corresponds to the native <c>WLAN_CONNECTION_NOTIFICATION_DATA</c> type.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct WlanConnectionNotificationData
	{
		/// <remarks>
		/// On Windows XP SP 2, only <see cref="WlanConnectionMode.Profile"/> is supported.
		/// </remarks>
		public WlanConnectionMode wlanConnectionMode;
		/// <summary>
		/// The name of the profile used for the connection. Profile names are case-sensitive.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string profileName;
		/// <summary>
		/// The SSID of the association.
		/// </summary>
		public Dot11Ssid dot11Ssid;
		/// <summary>
		/// The BSS network type.
		/// </summary>
		public Dot11BssType dot11BssType;
		/// <summary>
		/// Indicates whether security is enabled for this connection.
		/// </summary>
		public bool securityEnabled;
		/// <summary>
		/// Indicates the reason for an operation failure.
		/// This field has a value of <see cref="WlanReasonCode.Success"/> for all connection-related notifications except <see cref="WlanNotificationCodeAcm.ConnectionComplete"/>.
		/// If the connection fails, this field indicates the reason for the failure.
		/// </summary>
		public WlanReasonCode wlanReasonCode;
		/// <summary>
		/// This field contains the XML presentation of the profile used for discovery, if the connection succeeds.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
		public string profileXml;
	}


	/// <summary>
	/// Contains the SSID of an interface.
	/// </summary>
	public struct Dot11Ssid
	{
		/// <summary>
		/// The length, in bytes, of the <see cref="SSID"/> array.
		/// </summary>
		public uint SSIDLength;
		/// <summary>
		/// The SSID.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public byte[] SSID;
	}

	/// <summary>
	/// Contains association attributes for a connection
	/// </summary>
	/// <remarks>
	/// Corresponds to the native <c>WLAN_ASSOCIATION_ATTRIBUTES</c> type.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	public struct WlanAssociationAttributes
	{
		/// <summary>
		/// The SSID of the association.
		/// </summary>
		public Dot11Ssid dot11Ssid;
		/// <summary>
		/// Specifies whether the network is infrastructure or ad hoc.
		/// </summary>
		public Dot11BssType dot11BssType;
		/// <summary>
		/// The BSSID of the association.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public byte[] dot11Bssid;
		/// <summary>
		/// The physical type of the association.
		/// </summary>
		public Dot11PhyType dot11PhyType;
		/// <summary>
		/// The position of the <see cref="Dot11PhyType"/> value in the structure containing the list of PHY types.
		/// </summary>
		public uint dot11PhyIndex;
		/// <summary>
		/// A percentage value that represents the signal quality of the network.
		/// This field contains a value between 0 and 100.
		/// A value of 0 implies an actual RSSI signal strength of -100 dbm.
		/// A value of 100 implies an actual RSSI signal strength of -50 dbm.
		/// You can calculate the RSSI signal strength value for values between 1 and 99 using linear interpolation.
		/// </summary>
		public uint wlanSignalQuality;
		/// <summary>
		/// The receiving rate of the association.
		/// </summary>
		public uint rxRate;
		/// <summary>
		/// The transmission rate of the association.
		/// </summary>
		public uint txRate;

		/// <summary>
		/// Gets the BSSID of the associated access point.
		/// </summary>
		/// <value>The BSSID.</value>
		public PhysicalAddress Dot11Bssid
		{
			get { return new PhysicalAddress(dot11Bssid); }
		}
	}

	/// <summary>
	/// Defines the security attributes for a wireless connection.
	/// </summary>
	/// <remarks>
	/// Corresponds to the native <c>WLAN_SECURITY_ATTRIBUTES</c> type.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	public struct WlanSecurityAttributes
	{
		/// <summary>
		/// Indicates whether security is enabled for this connection.
		/// </summary>
		[MarshalAs(UnmanagedType.Bool)]
		public bool securityEnabled;
		[MarshalAs(UnmanagedType.Bool)]
		public bool oneXEnabled;
		/// <summary>
		/// The authentication algorithm.
		/// </summary>
		public Dot11AuthAlgorithm dot11AuthAlgorithm;
		/// <summary>
		/// The cipher algorithm.
		/// </summary>
		public Dot11CipherAlgorithm dot11CipherAlgorithm;
	}

	/// <summary>
	/// Defines the attributes of a wireless connection.
	/// </summary>
	/// <remarks>
	/// Corresponds to the native <c>WLAN_CONNECTION_ATTRIBUTES</c> type.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct WlanConnectionAttributes
	{
		/// <summary>
		/// The state of the interface.
		/// </summary>
		public WlanInterfaceState isState;
		/// <summary>
		/// The mode of the connection.
		/// </summary>
		public WlanConnectionMode wlanConnectionMode;
		/// <summary>
		/// The name of the profile used for the connection. Profile names are case-sensitive.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string profileName;
		/// <summary>
		/// The attributes of the association.
		/// </summary>
		public WlanAssociationAttributes wlanAssociationAttributes;
		/// <summary>
		/// The security attributes of the connection.
		/// </summary>
		public WlanSecurityAttributes wlanSecurityAttributes;
	}

	/// <summary>
	/// Contains information about a LAN interface.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct WlanInterfaceInfo
	{
		/// <summary>
		/// The GUID of the interface.
		/// </summary>
		public Guid interfaceGuid;
		/// <summary>
		/// The description of the interface.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string interfaceDescription;
		/// <summary>
		/// The current state of the interface.
		/// </summary>
		public WlanInterfaceState isState;
	}

	/// <summary>
	/// The header of the list returned by <see cref="WlanEnumInterfaces"/>.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct WlanInterfaceInfoListHeader
	{
		public uint numberOfItems;
		public uint index;
	}

	/// <summary>
	/// The header of the list returned by <see cref="WlanGetProfileList"/>.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct WlanProfileInfoListHeader
	{
		public uint numberOfItems;
		public uint index;
	}

	/// <summary>
	/// Contains basic information about a profile.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct WlanProfileInfo
	{
		/// <summary>
		/// The name of the profile. This value may be the name of a domain if the profile is for provisioning. Profile names are case-sensitive.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string profileName;
		/// <summary>
		/// Profile flags.
		/// </summary>
		public WlanProfileFlags profileFlags;
	}
}
