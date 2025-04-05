using In_Memory_Database.Classes.Dependencies.Managers;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace In_Memory_Database
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<ISearchManager, SearchManager>();
            services.TryAddSingleton<IDiskManager, DiskManager>();
        }
    }
}
