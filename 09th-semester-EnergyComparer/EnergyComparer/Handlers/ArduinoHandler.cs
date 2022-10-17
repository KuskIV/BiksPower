using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace EnergyComparer.Handlers
{
    public class ArduinoHandler
    {
        SerialPort serialPort;
        public ArduinoHandler(string portName = "COM3", int baud = 9600)
        {
            serialPort = new SerialPort(portName, baud);
            serialPort.Open();
        }

        public void Start() 
        {
            while (true)
            {
                serialPort.Write("1"); //1 is on 0 is off
                string responds = serialPort.ReadExisting();
                if (responds == "HIGN") 
                {
                    break;
                }
                Thread.Sleep(200);        
            }
        }

        public void Stop()
        {
            while (true)
            {
                serialPort.Write("0"); //1 is on 0 is off
                string responds = serialPort.ReadExisting();
                if (responds == "LOW")
                {
                    break;
                }
                Thread.Sleep(200);
            }
        }
    }
}
