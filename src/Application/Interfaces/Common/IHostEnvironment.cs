namespace Application.Interfaces.Common
{
    public interface IHostEnvironment
    {
        string EnvironmentName { get; }

        bool IsDevelopment();

        bool IsProduction();

        bool IsLocal();
    }
}
