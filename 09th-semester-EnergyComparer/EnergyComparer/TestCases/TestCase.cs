using EnergyComparer.Handlers;
using EnergyComparer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.TestCases
{
    public class TestCase : ITestCase
    {
        public string _name;
        public string _language;
        public DtoTestCase _program;

        public TestCase(IDataHandler dataHandler, string name, string language)
        {
            _name = name;
            _language = language;
            _program = dataHandler.GetTestCase(_name).Result;
        }


        public string GetExecutablePath(DirectoryInfo path)
        {
            var name = _name;
            return Constants.GetExecutablePathForOs(path, name);
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
    }

    public interface ITestCase
    {
        string GetExecutablePath(DirectoryInfo path);
        string GetLanguage();
        string GetName();
        DtoTestCase GetProgram();
    }
}
