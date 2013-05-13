using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleWifi.Win32.Interop
{
	/// <summary>
	/// Represents an error occuring during WLAN operations which indicate their failure via a <see cref="WlanReasonCode"/>.
	/// </summary>
	public class WlanException : Exception
	{
		private WlanReasonCode reasonCode;

		WlanException(WlanReasonCode reasonCode)
		{
			this.reasonCode = reasonCode;
		}

		/// <summary>
		/// Gets the WLAN reason code.
		/// </summary>
		/// <value>The WLAN reason code.</value>
		public WlanReasonCode ReasonCode
		{
			get { return reasonCode; }
		}

		/// <summary>
		/// Gets a message that describes the reason code.
		/// </summary>
		/// <value></value>
		/// <returns>The error message that explains the reason for the exception, or an empty string("").</returns>
		public override string Message
		{
			get
			{
				StringBuilder sb = new StringBuilder(1024);

				if (WlanInterop.WlanReasonCodeToString(reasonCode, sb.Capacity, sb, IntPtr.Zero) == 0)
					return sb.ToString();
				else
					return string.Empty;
			}
		}
	}

}
