using SimpleWifi.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleWifi
{
	internal static class ProfileFactory
	{
		/// <summary>
		/// Generates the profile XML for the access point and password
		/// </summary>
		internal static string Generate(AccessPoint ap, string password)
		{
			string profile	= string.Empty;
			string template = string.Empty;

			switch (ap.Network.dot11DefaultCipherAlgorithm)
			{
				case Wlan.Dot11CipherAlgorithm.WEP:
					template = GetTemplate("WEP");
					string hex = GetHexString(ap.Network.dot11Ssid.SSID);
					profile = string.Format(template, ap.Name, hex, password);
					break;
				case Wlan.Dot11CipherAlgorithm.CCMP: 
					template = GetTemplate("WPA2-PSK");
					profile = string.Format(template, ap.Name, password);
					break;
				case Wlan.Dot11CipherAlgorithm.TKIP:
					template = GetTemplate("WPA-PSK");
					profile = string.Format(template, ap.Name, password);
					break;			
				default:
					throw new NotImplementedException("Profile for selected cipher algorithm is not implemented");
			}					

			return profile;
		}

		/// <summary>
		/// Fetches the template for an wireless connection profile.
		/// </summary>
		private static string GetTemplate(string name)
		{
			string resourceName = string.Format("SimpleWifi.ProfileXML.{0}.xml", name);

			using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)))
			{
				return reader.ReadToEnd();
			}
		}

		/// <summary>
		/// Converts an byte array into the hex representation, ex: [255, 255] -> "FFFF"
		/// </summary>
		private static string GetHexString(byte[] ba)
		{
			StringBuilder sb = new StringBuilder(ba.Length * 2);

			foreach (byte b in ba)
				sb.AppendFormat("{0:x2}", b);
			
			return sb.ToString();
		}
	}
}
