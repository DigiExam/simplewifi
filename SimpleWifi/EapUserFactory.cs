using SimpleWifi.Win32.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleWifi
{
	internal static class EapUserFactory
	{
		/// <summary>
		/// Generates the EAP user XML
		/// </summary>
		internal static string Generate(Dot11CipherAlgorithm cipher, Dot11AuthAlgorithm auth, string username, string password, string domain)
		{
			throw new NotImplementedException();

			string profile = string.Empty;
			string template = string.Empty;
			
			/*switch (ap.Network.dot11DefaultCipherAlgorithm)
			{
				case Dot11CipherAlgorithm.WEP:
					template = GetTemplate("WEP");
					string hex = GetHexString(ap.Network.dot11Ssid.SSID);
					profile = string.Format(template, ap.Name, hex, password);
					break;
				case Dot11CipherAlgorithm.CCMP:
					template = GetTemplate("WPA2-PSK");
					profile = string.Format(template, ap.Name, password);
					break;
				case Dot11CipherAlgorithm.TKIP:
					template = GetTemplate("WPA-PSK");
					profile = string.Format(template, ap.Name, password);
					break;
				// TODO: Implement WPA2 Enterprise

				default:
					throw new NotImplementedException("Profile for selected cipher algorithm is not implemented");
			}*/

			return profile;
		}

		/// <summary>
		/// Fetches the template for an wireless connection profile.
		/// </summary>
		private static string GetTemplate(string name)
		{
			string resourceName = string.Format("SimpleWifi.EapUserXML.{0}.xml", name);

			using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
