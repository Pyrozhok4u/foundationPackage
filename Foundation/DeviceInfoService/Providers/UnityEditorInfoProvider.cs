
namespace Foundation.DeviceInfoService.Providers
{
    public class UnityEditorInfoProvider : DeviceInfo
    {
        public override Device GetDeviceType()
        {
            return Device.Desktop;
        }
    }
}
