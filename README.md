SimpleWifi
==========


[![NuGet](https://img.shields.io/nuget/dt/SimpleWifi.svg)](https://www.nuget.org/packages/SimpleWifi)

SimpleWifi is a .NET 4 library written in C# to manage wifi connections in Windows.  It is basically a layer of abstraction above Managed Wifi API that takes care of wifi connection profile creation etc, built to be easy to use.

An example console application is supplied.

Available on NuGet: https://www.nuget.org/packages/SimpleWifi

Currently supported ciphers
---------------------------
- NONE (open networks)
- WEP
- WPA-PSK
- WPA2-PSK
 
Supported operating systems
---------------------------
- Windows XP SP2 (with hotfix [KB918997](http://support.microsoft.com/kb/918997))
- Windows Vista
- Windows 7
- Windows 8

Related links
-------------
- Profile examples: http://msdn.microsoft.com/en-us/library/windows/desktop/aa369853(v=vs.85).aspx
- Managed Wifi API: http://managedwifi.codeplex.com/
