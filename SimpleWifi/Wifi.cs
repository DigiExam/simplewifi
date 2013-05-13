using SimpleWifi.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SimpleWifi.Win32.Interop;

using NotifCodeACM = SimpleWifi.Win32.Interop.WlanNotificationCodeAcm;
using NotifCodeMSM = SimpleWifi.Win32.Interop.WlanNotificationCodeMsm;

namespace SimpleWifi
{
	public class Wifi
	{
		public event EventHandler<WifiStatusEventArgs> ConnectionStatusChanged;
		
		private WlanClient _client;
		private WifiStatus _connectionStatus;
		private bool _isConnectionStatusSet = false;

		public Wifi()
		{
			_client = new WlanClient();
			
			foreach (var inte in _client.Interfaces)
				inte.WlanNotification += inte_WlanNotification;
		}
		
		/// <summary>
		/// Returns a list over all available access points
		/// </summary>
		public List<AccessPoint> GetAccessPoints()
		{
			List<AccessPoint> accessPoints = new List<AccessPoint>();
			
			foreach (WlanInterface wlanIface in _client.Interfaces)
			{
				WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);

				foreach (WlanAvailableNetwork network in networks)				
					accessPoints.Add(new AccessPoint(wlanIface, network));				
			}

			return accessPoints;
		}

		/// <summary>
		/// Disconnect all wifi interfaces
		/// </summary>
		public void Disconnect()
		{
			foreach (WlanInterface wlanIface in _client.Interfaces)
			{
				wlanIface.Disconnect();
			}		
		}
		public WifiStatus ConnectionStatus
		{
			get
			{
				if (!_isConnectionStatusSet)
					ConnectionStatus = GetForcedConnectionStatus();

				return _connectionStatus;
			}
			private set
			{
				_isConnectionStatusSet = true;
				_connectionStatus = value;
			}
		}

		private void inte_WlanNotification(WlanNotificationData notifyData)
		{
			if (notifyData.notificationSource == WlanNotificationSource.ACM && (NotifCodeACM)notifyData.NotificationCode == NotifCodeACM.Disconnected)
				OnConnectionStatusChanged(WifiStatus.Disconnected);
			else if (notifyData.notificationSource == WlanNotificationSource.MSM && (NotifCodeMSM)notifyData.NotificationCode == NotifCodeMSM.Connected)
				OnConnectionStatusChanged(WifiStatus.Connected);
		}

		private void OnConnectionStatusChanged(WifiStatus newStatus)
		{
			ConnectionStatus = newStatus;

			if (ConnectionStatusChanged != null)
				ConnectionStatusChanged(this, new WifiStatusEventArgs(newStatus));
		}

		// I don't like this method, it's slow, ugly and should be refactored ASAP.
		private WifiStatus GetForcedConnectionStatus() {
			bool connected = false;

			foreach (var i in _client.Interfaces)
			{
				try
				{
					var a = i.CurrentConnection; // Current connection throws an exception if disconnected.
					connected = true;
				}
				catch {	}
			}

			if (connected)
				return WifiStatus.Connected;
			else
				return WifiStatus.Disconnected;
		}
	}

	public class WifiStatusEventArgs : EventArgs
	{
		public WifiStatus NewStatus { get; private set; }

		internal WifiStatusEventArgs(WifiStatus status) : base()
		{
			this.NewStatus = status;
		}

	}

	public enum WifiStatus
	{
		Disconnected,
		Connected
	}
}
