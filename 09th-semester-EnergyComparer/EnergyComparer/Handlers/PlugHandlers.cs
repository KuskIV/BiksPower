using EnergyComparer.Models;
using SmartHomeController.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Handlers
{
    public static class PlugHandlers
    {

        private static Dictionary<string, Plug> plugs = new Dictionary<string, Plug>() 
        {
            {"SurfacePro" , new Plug("192.168.1.150") },
            {"SurfaceBook" , new Plug("192.168.1.182") },
        };

        public static void EnableSurfacePro()
        {
            plugs["SurfacePro"].TurnOn();
        }

        public static void DisableSurfacePro()
        {
            plugs["SurfacePro"].TurnOff();
        }

        public static void EnableSurfaceBook()
        {
            plugs["SurfaceBook"].TurnOn();
        }

        public static void DisableSurfaceBook()
        {
            plugs["SurfaceBook"].TurnOff();
        }

        public static Dictionary<string, Plug> GetPlugs() 
        {
            return plugs;
        }
    }
}
