using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleWifi
{
	public static class PasswordHelper
	{
		/// <summary>
		/// Checks if a password is valid for a cipher type.
		/// </summary>
		public static bool IsValid(string password, Dot11CipherAlgorithm cipherAlgorithm)
		{
			switch (cipherAlgorithm)
			{
				case Dot11CipherAlgorithm.None:
					return true;
				case Dot11CipherAlgorithm.WEP: // WEP key is 10, 26 or 40 hex digits long.
					if (string.IsNullOrEmpty(password))
						return false;

					int len = password.Length;

					bool correctLength = len == 10 || len == 26 || len == 40;
					bool onlyHex = new Regex("^[0-9A-F]+$").IsMatch(password);

					return correctLength && onlyHex;
				case Dot11CipherAlgorithm.CCMP: // WPA2-PSK 8 to 63 ASCII characters					
				case Dot11CipherAlgorithm.TKIP: // WPA-PSK 8 to 63 ASCII characters
					if (string.IsNullOrEmpty(password))
						return false;

					return 8 <= password.Length && password.Length <= 63;
				default:
					return true;
			}
		}		
	}
}
