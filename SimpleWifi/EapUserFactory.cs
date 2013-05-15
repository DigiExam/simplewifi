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
		internal static string Generate(Dot11CipherAlgorithm cipher, string username, string password, string domain)
		{
			#warning Robin: Probably not properly implemented, only supports WPA- and WPA-2 Enterprise with PEAP-MSCHAPv2

			string profile = string.Empty;
			string template = string.Empty;
			
			switch (cipher)
			{
				case Dot11CipherAlgorithm.CCMP: // WPA-2
				case Dot11CipherAlgorithm.TKIP: // WPA
					template = GetTemplate("PEAP-MS-CHAPv2");
					profile = string.Format(template, username, password, domain);
					break;
				default:
					throw new NotImplementedException("Profile for selected cipher algorithm is not implemented");
			}

			return profile;
		}

		/// <summary>
		/// Fetches the template for an EAP user
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
