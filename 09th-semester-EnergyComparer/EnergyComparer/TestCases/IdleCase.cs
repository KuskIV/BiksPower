using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Programs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.TestCases
{
    public class IdleCase : ITestCase
    {
        private readonly string _name;
        private readonly string _language;
        private readonly DtoTestCase _program;

        public IdleCase(IDataHandler dataHandler)
        {
            _name = "IdleCase";
            _language = ELanguage.CSharp.ToString();
            _program = dataHandler.GetTestCase(_name).Result;
        }

        public string GetExecutablePath(DirectoryInfo path)
        {
            if (Constants.IsWindows())
            {
                return path.FullName + "\\09th-semester-test-cases\\TestCaseIdle\\TestCaseIdle\\bin\\Release\\net6.0\\TestCaseIdle.exe";
            }
            else
            {
                return path.Parent.FullName +
                       "/09th-semester-test-cases/TestCaseIdle/TestCaseIdle/bin/Release/net6.0/linux-x64/TestCaseIdle";
            }
        }

        public string GetLanguage()
        {
            return _language;
        }

        public string GetName()
        {
            return _name;
        }

        public DtoTestCase GetProgram()
        {
            return _program;
        }

        public void Run()
        {
            Thread.Sleep(TimeSpan.FromSeconds(30));
        }
    }
}
