using SimpleWifi.Win32.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleWifi.Win32.Helpers
{
	internal static class WlanHelpers
	{
		internal static WlanConnectionNotificationData? ParseWlanConnectionNotification(ref WlanNotificationData notifyData)
		{
			int expectedSize = Marshal.SizeOf(typeof(WlanConnectionNotificationData));
			if (notifyData.dataSize < expectedSize)
				return null;

			WlanConnectionNotificationData connNotifyData = (WlanConnectionNotificationData)Marshal.PtrToStructure(notifyData.dataPtr, typeof(WlanConnectionNotificationData));

			if (connNotifyData.wlanReasonCode == WlanReasonCode.Success)
			{
				long profileXmlPtrValue = notifyData.dataPtr.ToInt64() + Marshal.OffsetOf(typeof(WlanConnectionNotificationData), "profileXml").ToInt64();
				connNotifyData.profileXml = Marshal.PtrToStringUni(new IntPtr(profileXmlPtrValue));
			}

			return connNotifyData;
		}

		/// <summary>
		/// Gets a string that describes a specified reason code. NOTE: Not used!
		/// </summary>
		/// <param name="reasonCode">The reason code.</param>
		/// <returns>The string.</returns>
		internal static string GetStringForReasonCode(WlanReasonCode reasonCode)
		{
			StringBuilder sb = new StringBuilder(1024); // the 1024 size here is arbitrary; the WlanReasonCodeToString docs fail to specify a recommended size

			if (WlanInterop.WlanReasonCodeToString(reasonCode, sb.Capacity, sb, IntPtr.Zero) != 0)
			{
				sb.Clear(); // Not sure if we get junk in the stringbuilder buffer from WlanReasonCodeToString, clearing it to be sure. 
				sb.Append("Failed to retrieve reason code, probably too small buffer.");
			}

			return sb.ToString();
		}
	}
}
