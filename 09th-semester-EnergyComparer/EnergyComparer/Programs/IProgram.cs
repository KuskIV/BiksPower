using EnergyComparer.Models;

namespace EnergyComparer.Programs
{
    public interface IProgram
    {
        DtoProgram GetProgram();
        string GetLanguage();
        void Run();
        public string GetName();
    }
}