using SimpleWifi.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SimpleWifi.Win32.Interop;

namespace SimpleWifi
{
	public class AccessPoint
	{
		private WlanInterface _interface;
		private WlanAvailableNetwork _network;

		internal AccessPoint(WlanInterface interfac, WlanAvailableNetwork network)
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
				try
				{
					return _interface.GetProfiles().Where(p => p.profileName == Name).Any();
				}
				catch 
				{ 
					return false; 
				}
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
		internal WlanAvailableNetwork Network
		{
			get
			{
				return _network;
			}
		}


		/// <summary>
		/// Returns the underlying interface object.
		/// </summary>
		internal WlanInterface Interface
		{
			get
			{
				return _interface;
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
		public bool Connect(AuthRequest request, bool overwriteProfile = false)
		{
			// No point to continue with the connect if the password is not valid if overwrite is true or profile is missing.
			if (!request.IsPasswordValid && (!HasProfile || overwriteProfile))
				return false;

			// If we should create or overwrite the profile, do so.
			if (!HasProfile || overwriteProfile)
			{				
				if (HasProfile)
					_interface.DeleteProfile(Name);

				if (request.IsPasswordRequired)
					request.Process();				
			}
			
			return _interface.ConnectSynchronously(WlanConnectionMode.Profile, _network.dot11BssType, Name, 6000);			
		}

		/// <summary>
		/// Connect asynchronous to the access point.
		/// </summary>
		public void ConnectAsync(AuthRequest request, bool overwriteProfile = false, Action<bool> onConnectComplete = null)
		{
			// TODO: Refactor -> Use async connect in wlaninterface.
			ThreadPool.QueueUserWorkItem(new WaitCallback((o) => {
				bool success = false;

				try
				{
					success = Connect(request, overwriteProfile);
				}
				catch (Win32Exception)
				{
					// Cipher = NONE throws an error.
					success = false;
				}

				if (onConnectComplete != null)
					onConnectComplete(success);
			}));
		}
				
		public string GetProfileXML()
		{
			if (HasProfile)
				return _interface.GetProfileXml(Name);
			else
				return string.Empty;
		}

		public void DeleteProfile()
		{
			try
			{
				if (HasProfile)
					_interface.DeleteProfile(Name);
			}
			catch { }
		}

		public override sealed string ToString()
		{
			StringBuilder info = new StringBuilder();
			info.AppendLine("Interface: " + _interface.InterfaceName);
			info.AppendLine("Auth algorithm: " + _network.dot11DefaultAuthAlgorithm);
			info.AppendLine("Cipher algorithm: " + _network.dot11DefaultCipherAlgorithm);
			info.AppendLine("BSS type: " + _network.dot11BssType);
			info.AppendLine("Connectable: " + _network.networkConnectable);
			
			if (!_network.networkConnectable)
				info.AppendLine("Reason to false: " + _network.wlanNotConnectableReason);

			return info.ToString();
		}
	}
}
