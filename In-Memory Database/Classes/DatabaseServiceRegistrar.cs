using Microsoft.Extensions.DependencyInjection.Extensions;

namespace In_Memory_Database.Classes
{
    public class DatabaseServiceRegistrar
    {
        public static void AddInMemoryDBService(
            IServiceCollection services,
            ISearchManager searchManager = null,
            IFileManager fileManager = null
        )
        {
            if (searchManager != null)
            {
                services.TryAddSingleton(searchManager);
            }
            if (fileManager != null)
            {
                services.TryAddSingleton(fileManager);
            }
            var startup = new Startup();
            startup.ConfigureServices(services);
            services.AddSingleton<Database>();
        }
    }
}
