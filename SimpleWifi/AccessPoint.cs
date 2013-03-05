using SimpleWifi.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SimpleWifi
{
	public class AccessPoint
	{
		private WlanClient.WlanInterface _interface;
		private Wlan.WlanAvailableNetwork _network;

		internal AccessPoint(WlanClient.WlanInterface interfac, SimpleWifi.Win32.Wlan.WlanAvailableNetwork network)
		{
			_interface = interfac;
			_network = network;
		}

		public string Name
		{
			get
			{
				return Encoding.ASCII.GetString(_network.dot11Ssid.SSID, 0, (int)_network.dot11Ssid.SSIDLength);
			}
		}

		public uint SignalStrength
		{
			get
			{
				return _network.wlanSignalQuality;
			}
		}

		/// <summary>
		/// If the computer has a connection profile stored for this access point
		/// </summary>
		public bool HasProfile
		{
			get
			{
				return _interface.GetProfiles().Where(p => p.profileName == Name).Any();
			}
		}
		
		public bool IsSecure
		{
			get
			{
				return _network.securityEnabled;
			}
		}

		public bool IsConnected
		{
			get
			{
				try
				{
					var a = _interface.CurrentConnection; // This prop throws exception if not connected, which forces me to this try catch. Refactor plix.
					return a.profileName == _network.profileName;
				}
				catch
				{
					return false;
				}
			}

		}

		/// <summary>
		/// Returns the underlying network object.
		/// </summary>
		internal Wlan.WlanAvailableNetwork Network
		{
			get
			{
				return _network;
			}
		}

		/// <summary>
		/// Checks that the password format matches this access point's encryption method.
		/// </summary>
		public bool IsValidPassword(string password)
		{
			return PasswordHelper.IsValid(password, _network.dot11DefaultCipherAlgorithm);
		}		
		
		/// <summary>
		/// Connect synchronous to the access point.
		/// </summary>
		public bool Connect(string password, bool overwriteProfile = false)
		{
			// No point to continue with the connect if the password is not valid if overwrite is true or profile is missing.
			if (!IsValidPassword(password) && (!HasProfile || overwriteProfile))
				return false;

			// If we should create or overwrite the profile, do so.
			if (!HasProfile || overwriteProfile)
			{				
				if (HasProfile)
					_interface.DeleteProfile(Name);

				string profileXML = ProfileFactory.Generate(this, password);
				_interface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXML, true);
			}

			return _interface.ConnectSynchronously(Wlan.WlanConnectionMode.Profile, _network.dot11BssType, Name, 3000);			
		}

		/// <summary>
		/// Connect asynchronous to the access point.
		/// </summary>
		public void ConnectAsync(string password, bool overwriteProfile = false, Action<bool> onConnectComplete = null)
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback((o) => {
				bool success = false;

				try
				{
					success = Connect(password, overwriteProfile);
				}
				catch (Win32Exception)
				{
					success = false;
				}

				if (onConnectComplete != null)
					onConnectComplete(success);
			}));
		}
	}
}
