using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleWifi
{
	public class AuthRequest
	{
		private bool _isPasswordRequired, _isUsernameRequired, _isDomainSupported, _isEAPStore;
		private string _password, _username, _domain;
		private WlanAvailableNetwork _network;
		private WlanInterface _interface;

		public AuthRequest(AccessPoint ap)
		{	
			_network	= ap.Network;
			_interface	= ap.Interface;

			_isPasswordRequired = 
				_network.securityEnabled &&
				_network.dot11DefaultCipherAlgorithm != Dot11CipherAlgorithm.None;

			_isEAPStore =
				_network.dot11DefaultAuthAlgorithm == Dot11AuthAlgorithm.RSNA ||
				_network.dot11DefaultAuthAlgorithm == Dot11AuthAlgorithm.WPA;

			_isUsernameRequired = _isEAPStore;
			_isDomainSupported	= _isEAPStore;
		}
		
		public bool IsPasswordRequired	{ get { return _isPasswordRequired; } }
		public bool IsUsernameRequired	{ get { return _isUsernameRequired; } }
		public bool IsDomainSupported	{ get { return _isDomainSupported; } }

		public string Password
		{
			get { return _password; }
			set { _password = value; }
		}

		public string Username
		{
			get { return _username; }
			set { _username = value; }
		}

		public string Domain
		{
			get { return _domain; }
			set { _domain = value; }
		}

		public bool IsPasswordValid
		{
			get
			{
				#warning Robin: Not sure that Enterprise networks have the same requirements on the password complexity as standard ones.
				return PasswordHelper.IsValid(_password, _network.dot11DefaultCipherAlgorithm);
			}
		}
		
		private bool SaveToEAP() 
		{
			if (!_isEAPStore || !IsPasswordValid)
				return false;

			string userXML = EapUserFactory.Generate(_network.dot11DefaultCipherAlgorithm, _username, _password, _domain);
			_interface.SetEAP(_network.profileName, userXML);

			return true;		
		}

		internal bool Process()
		{
			if (!IsPasswordValid)
				return false;

			if (_isEAPStore && !SaveToEAP())
				return false;			

			string profileXML = ProfileFactory.Generate(_network, _password);
			_interface.SetProfile(WlanProfileFlags.AllUser, profileXML, true);

			return true;
		}
	}

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
