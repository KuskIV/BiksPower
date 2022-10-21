using CsvHelper;
using EnergyComparer.Models;

namespace EnergyComparer.Programs
{
    public interface ISoftwareEntity
    {
        DtoProgram GetProgram();
        string GetLanguage();
        void Run();
        public string GetName();
    }
}