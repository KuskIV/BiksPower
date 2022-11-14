using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Services
{
    public class WifiService : IWifiService
    {
        private readonly IHardwareHandler _hardwareHandler;

        public WifiService(IHardwareHandler hardwareHandler)
        {
            _hardwareHandler = hardwareHandler;
        }

        public async Task Enable(bool isProd)
        {
            if (isProd)
            {
                _hardwareHandler.EnableWifi();
                await IsWifiEnabled();
            }
        }

        public void Disable(bool isProd)
        {
            if (isProd)
            {
                _hardwareHandler.DisableWifi();
            }
        }

        public bool PingGoogleSuccessfully()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task IsWifiEnabled()
        {
            await Task.Delay(TimeSpan.FromSeconds(15));

            while (!PingGoogleSuccessfully())
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }

    public interface IWifiService
    {
        void Disable(bool isProd);
        Task Enable(bool isProd);
        bool PingGoogleSuccessfully();
    }
}
