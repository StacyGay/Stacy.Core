﻿using System;
using System.Runtime.InteropServices;

namespace Stacy.Core.File
{
	public class DriveMapper
	{
		private enum ResourceScope
		{
			RESOURCE_CONNECTED = 1,
			RESOURCE_GLOBALNET,
			RESOURCE_REMEMBERED,
			RESOURCE_RECENT,
			RESOURCE_CONTEXT
		}
		private enum ResourceType
		{
			RESOURCETYPE_ANY,
			RESOURCETYPE_DISK,
			RESOURCETYPE_PRINT,
			RESOURCETYPE_RESERVED
		}
		private enum ResourceUsage
		{
			RESOURCEUSAGE_CONNECTABLE = 0x00000001,
			RESOURCEUSAGE_CONTAINER = 0x00000002,
			RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
			RESOURCEUSAGE_SIBLING = 0x00000008,
			RESOURCEUSAGE_ATTACHED = 0x00000010
		}
		private enum ResourceDisplayType
		{
			RESOURCEDISPLAYTYPE_GENERIC,
			RESOURCEDISPLAYTYPE_DOMAIN,
			RESOURCEDISPLAYTYPE_SERVER,
			RESOURCEDISPLAYTYPE_SHARE,
			RESOURCEDISPLAYTYPE_FILE,
			RESOURCEDISPLAYTYPE_GROUP,
			RESOURCEDISPLAYTYPE_NETWORK,
			RESOURCEDISPLAYTYPE_ROOT,
			RESOURCEDISPLAYTYPE_SHAREADMIN,
			RESOURCEDISPLAYTYPE_DIRECTORY,
			RESOURCEDISPLAYTYPE_TREE,
			RESOURCEDISPLAYTYPE_NDSCONTAINER
		}
		[StructLayout(LayoutKind.Sequential)]
		private struct NETRESOURCE
		{
			public ResourceScope oResourceScope;
			public ResourceType oResourceType;
			public ResourceDisplayType oDisplayType;
			public ResourceUsage oResourceUsage;
			public string sLocalName;
			public string sRemoteName;
			public string sComments;
			public string sProvider;
		}
		[DllImport("mpr.dll")]
		private static extern int WNetAddConnection2
			(ref NETRESOURCE oNetworkResource, string sPassword,
			string sUserName, int iFlags);

		[DllImport("mpr.dll")]
		private static extern int WNetCancelConnection2
			(string sLocalName, uint iFlags, int iForce);

		public static void MapNetworkDrive(string sNetworkPath, string sDriveLetter)
		{
			//Checks if the last character is \ as this causes error on mapping a drive.
			if (sNetworkPath.Substring(sNetworkPath.Length - 1, 1) == @"\")
			{
				sNetworkPath = sNetworkPath.Substring(0, sNetworkPath.Length - 1);
			}

			NETRESOURCE oNetworkResource = new NETRESOURCE();
			oNetworkResource.oResourceType = ResourceType.RESOURCETYPE_DISK;
			oNetworkResource.sLocalName = sDriveLetter + ":";
			oNetworkResource.sRemoteName = sNetworkPath;


			//If Drive is already mapped disconnect the current 
			//mapping before adding the new mapping
			if (IsDriveMapped(sDriveLetter))
			{
				DisconnectNetworkDrive(sDriveLetter, true);
			}

			WNetAddConnection2(ref oNetworkResource, null, null, 0);
		}

		public static string MapNetworkDrive(string sNetworkPath)
		{
			char cDriveLetter = 'Z';

			while (cDriveLetter > 'C' && IsDriveMapped(cDriveLetter.ToString()))
				cDriveLetter--;

			MapNetworkDrive(sNetworkPath, cDriveLetter.ToString());

			return cDriveLetter.ToString();
		}

		public static int DisconnectNetworkDrive(string sDriveLetter, bool bForceDisconnect)
		{
			if (bForceDisconnect)
			{
				return WNetCancelConnection2(sDriveLetter + ":", 0, 1);
			}
			else
			{
				return WNetCancelConnection2(sDriveLetter + ":", 0, 0);
			}
		}

		public static bool IsDriveMapped(string sDriveLetter)
		{
			string[] DriveList = Environment.GetLogicalDrives();
			for (int i = 0; i < DriveList.Length; i++)
			{
				if (sDriveLetter + ":\\" == DriveList[i].ToString())
				{
					return true;
				}
			}
			return false;
		}
	}
}
