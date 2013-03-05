using SimpleWifi;
using SimpleWifi.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleWifi.Example
{
	class Program
	{
		private static Wifi wifi;

		static void Main(string[] args)
		{
			// Init wifi object and event handlers
			wifi = new Wifi();
			wifi.ConnectionStatusChanged += wifi_ConnectionStatusChanged;
			
			string command = "";
			do
			{
				Console.WriteLine("\r\n-- COMMAND LIST --");
				Console.WriteLine("L. List access points");
				Console.WriteLine("C. Connect");
				Console.WriteLine("D. Disconnect");
				Console.WriteLine("S. Status");
				
				command = Console.ReadLine().ToLower();

				Execute(command);

			} while (command != "q");
		}

		static void Execute(string command)
		{
			switch (command)
			{
				case "l":
					List();
					break;
				case "d":
					Disconnect();
					break;
				case "c":
					Connect();
					break;
				case "s":
					Status();
					break;
				case "q":
					break;
				default:
					Console.WriteLine("\r\nIncorrect command.");
					break;

			}
		}

		private static void Disconnect()
		{
			wifi.Disconnect();
		}

		private static void Status()
		{
			Console.WriteLine("\r\n-- CONNECTION STATUS --");
			if (wifi.ConnectionStatus == WifiStatus.Connected)
				Console.WriteLine("You are connected to a wifi");
			else
				Console.WriteLine("You are not connected to a wifi");
		}
		
		static IEnumerable<AccessPoint> List()
		{
			Console.WriteLine("\r\n-- Access point list --");
			IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints().OrderByDescending(ap => ap.SignalStrength);

			int i = 0;
			foreach (AccessPoint ap in accessPoints)
				Console.WriteLine("{0}. {1} {2}% Connected: {3}", i++, ap.Name, ap.SignalStrength, ap.IsConnected);

			return accessPoints;
		}

		
		static void Connect()
		{
			var accessPoints = List();

			Console.Write("\r\nEnter the index of the network you wish to connect to: ");

			int selectedIndex = int.Parse(Console.ReadLine());
			AccessPoint selectedAP = accessPoints.ToList()[selectedIndex];

			string password = string.Empty;
			bool overwrite = true;

			if (selectedAP.IsSecure)
			{
				if (selectedAP.HasProfile) // If there already is a stored profile for the network, we can either use it or overwrite it with a new password.
				{
					Console.Write("\r\nA network profile already exist, do you want to use it (y/n)? ");

					if (Console.ReadLine().ToLower() == "y")
						overwrite = false;
				}

				if (overwrite)
					password = PasswordPrompt(selectedAP);
			}

			selectedAP.ConnectAsync(password, overwrite, OnConnectedComplete);
		}

		static string PasswordPrompt(AccessPoint selectedAP)
		{
			string password = string.Empty;

			bool validPassFormat = false;

			while (!validPassFormat)
			{
				Console.Write("\r\nPlease enter the wifi password: ");
				password = Console.ReadLine();

				validPassFormat = selectedAP.IsValidPassword(password);

				if (!validPassFormat)
					Console.WriteLine("\r\nPassword is not valid for this network type.");
			}

			return password;
		}
		
		static void wifi_ConnectionStatusChanged(object sender, WifiStatusEventArgs e)
		{
			Console.WriteLine("\nNew status: {0}", e.NewStatus.ToString());
		}

		static void OnConnectedComplete(bool success)
		{
			Console.WriteLine("\nOnConnectedComplete, success: {0}", success);
		}
	}
}
