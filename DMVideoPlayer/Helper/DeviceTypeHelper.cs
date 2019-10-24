using DmVideoPlayer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DmVideoPlayer.Helper
{
    internal static class DeviceTypeHelper
    {
        internal static bool IsXbox
        {
            get { return GetDeviceType() == DeviceTypeEnum.Xbox; }
        }

        internal static DeviceTypeEnum GetDeviceType()
        {
            switch (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily)
            {
                case "Windows.Desktop":
                    return DeviceTypeEnum.Tablet;
                case "Windows.Mobile":
                    return DeviceTypeEnum.Phone;
                case "Windows.Universal":
                    return DeviceTypeEnum.IoT;
                case "Windows.Team":
                    return DeviceTypeEnum.SurfaceHub;
                case "Windows.Xbox":
                    return DeviceTypeEnum.Xbox;
                default:
                    return DeviceTypeEnum.Other;
            }
        }
    }
}
