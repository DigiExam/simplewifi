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
	internal class Program
	{
		private static Wifi wifi;

		private static void Main(string[] args)
		{
			// Init wifi object and event handlers
			wifi = new Wifi();
			wifi.ConnectionStatusChanged += wifi_ConnectionStatusChanged;

			if (wifi.NoWifiAvailable)
				Console.WriteLine("\r\n-- NO WIFI CARD WAS FOUND --");

			string command = "";
			do
			{
				Console.WriteLine("\r\n-- COMMAND LIST --");
				Console.WriteLine("L. List access points");
				Console.WriteLine("C. Connect");
				Console.WriteLine("D. Disconnect");
				Console.WriteLine("S. Status");
				Console.WriteLine("X. Print profile XML");
				Console.WriteLine("R. Remove profile");
				Console.WriteLine("I. Show access point information");
				Console.WriteLine("Q. Quit");

				command = Console.ReadLine().ToLower();

				Execute(command);
			} while (command != "q");
		}

		private static void Execute(string command)
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
				case "x":
					ProfileXML();
					break;
				case "r":
					DeleteProfile();
					break;
				case "i":
					ShowInfo();
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

		private static IEnumerable<AccessPoint> List()
		{
			Console.WriteLine("\r\n-- Access point list --");
			IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints().OrderByDescending(ap => ap.SignalStrength);

			int i = 0;
			foreach (AccessPoint ap in accessPoints)
				Console.WriteLine("{0}. {1} {2}% Connected: {3}", i++, ap.Name, ap.SignalStrength, ap.IsConnected);

			return accessPoints;
		}


		private static void Connect()
		{
			var accessPoints = List();

			Console.Write("\r\nEnter the index of the network you wish to connect to: ");

			int selectedIndex = int.Parse(Console.ReadLine());
			if (selectedIndex > accessPoints.ToArray().Length || accessPoints.ToArray().Length == 0)
			{
				Console.Write("\r\nIndex out of bounds");
				return;
			}
			AccessPoint selectedAP = accessPoints.ToList()[selectedIndex];

			// Auth
			AuthRequest authRequest = new AuthRequest(selectedAP);
			bool overwrite = true;

			if (authRequest.IsPasswordRequired)
			{
				if (selectedAP.HasProfile)
					// If there already is a stored profile for the network, we can either use it or overwrite it with a new password.
				{
					Console.Write("\r\nA network profile already exist, do you want to use it (y/n)? ");
					if (Console.ReadLine().ToLower() == "y")
					{
						overwrite = false;
					}
				}

				if (overwrite)
				{
					if (authRequest.IsUsernameRequired)
					{
						Console.Write("\r\nPlease enter a username: ");
						authRequest.Username = Console.ReadLine();
					}

					authRequest.Password = PasswordPrompt(selectedAP);

					if (authRequest.IsDomainSupported)
					{
						Console.Write("\r\nPlease enter a domain: ");
						authRequest.Domain = Console.ReadLine();
					}
				}
			}

			selectedAP.ConnectAsync(authRequest, overwrite, OnConnectedComplete);
		}

		private static string PasswordPrompt(AccessPoint selectedAP)
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

		private static void ProfileXML()
		{
			var accessPoints = List();

			Console.Write("\r\nEnter the index of the network you wish to print XML for: ");

			int selectedIndex = int.Parse(Console.ReadLine());
			if (selectedIndex > accessPoints.ToArray().Length || accessPoints.ToArray().Length == 0)
			{
				Console.Write("\r\nIndex out of bounds");
				return;
			}
			AccessPoint selectedAP = accessPoints.ToList()[selectedIndex];

			Console.WriteLine("\r\n{0}\r\n", selectedAP.GetProfileXML());
		}

		private static void DeleteProfile()
		{
			var accessPoints = List();

			Console.Write("\r\nEnter the index of the network you wish to delete the profile: ");

			int selectedIndex = int.Parse(Console.ReadLine());
			if (selectedIndex > accessPoints.ToArray().Length || accessPoints.ToArray().Length == 0)
			{
				Console.Write("\r\nIndex out of bounds");
				return;
			}
			AccessPoint selectedAP = accessPoints.ToList()[selectedIndex];

			selectedAP.DeleteProfile();
			Console.WriteLine("\r\nDeleted profile for: {0}\r\n", selectedAP.Name);
		}


		private static void ShowInfo()
		{
			var accessPoints = List();

			Console.Write("\r\nEnter the index of the network you wish to see info about: ");

			int selectedIndex = int.Parse(Console.ReadLine());
			if (selectedIndex > accessPoints.ToArray().Length || accessPoints.ToArray().Length == 0)
			{
				Console.Write("\r\nIndex out of bounds");
				return;
			}
			AccessPoint selectedAP = accessPoints.ToList()[selectedIndex];

			Console.WriteLine("\r\n{0}\r\n", selectedAP.ToString());
		}

		private static void wifi_ConnectionStatusChanged(object sender, WifiStatusEventArgs e)
		{
			Console.WriteLine("\nNew status: {0}", e.NewStatus.ToString());
		}

		private static void OnConnectedComplete(bool success)
		{
			Console.WriteLine("\nOnConnectedComplete, success: {0}", success);
		}
	}
}
