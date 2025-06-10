using System;
using System.IO;

namespace RandomPunisher
{
    public class DriveInfoEx
    {
        public string Path { get; set; }

        public DriveType Type { get; set; }

        public double TotalSpace { get; set; }

        public double UsedSpace { get; set; }

        public string FDriveType()
        {
            switch (Type)
            {
                case DriveType.Fixed: return "硬盘驱动器";
                case DriveType.Removable: return "可移动驱动器";
                case DriveType.CDRom: return "CD 驱动器";
                case DriveType.Network: return "网络驱动器";
                case DriveType.Ram: return "内存驱动器";
                case DriveType.NoRootDirectory: return "没有根目录的驱动器";
                default: return "未知";
            }
        }

        public string FFreeSpace { get { return GetFriendlySpace(TotalSpace - UsedSpace); } }

        public string FTotalSpace { get { return GetFriendlySpace(TotalSpace); } }

        public string FUsedSpace { get { return GetFriendlySpace(UsedSpace); } }

        public static string GetFriendlySpace(double Space)
        {
            string[] Units = new string[] { "字节", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double Temp = Space; int i;
            for (i = 0; i < Units.Length; i++)
            { Space = Space / 1024; if (Space < 0.9) { return Math.Round(Temp, 2).ToString() + " " + Units[i]; } else { Temp = Space; } }
            try { return Math.Round(Space, 2).ToString() + " " + Units[i + 1]; }
            catch { return Math.Round(Space, 2).ToString() + " " + Units[i]; }
        }

        public static async void TryReqFSPrivilege()
        { await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess")); }
    }
}
