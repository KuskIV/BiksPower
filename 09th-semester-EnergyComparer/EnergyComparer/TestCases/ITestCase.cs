using CsvHelper;
using EnergyComparer.Models;

namespace EnergyComparer.Programs
{
    public interface ITestCase
    {
        string GetExecutablePath(DirectoryInfo path);
        DtoTestCase GetProgram();
        string GetLanguage();
        void Run();
        public string GetName();
    }
}