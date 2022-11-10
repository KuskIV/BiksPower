using EnergyComparer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Handlers
{
    public class PlugHandlers
    {
        Dictionary<string, Plug> plugs = new Dictionary<string, Plug>() 
        {
            {"SurfacePro" , new Plug("192.168.1.150") },
            {"SurfaceBook" , new Plug("192.168.1.182") },
        };

        public Dictionary<string, Plug> GetPlugs() 
        {
            return plugs;
        }
    }
}
