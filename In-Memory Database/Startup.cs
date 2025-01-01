using In_Memory_Database.Classes;

namespace In_Memory_Database
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISearchManager, SearchManager>();
        }
    }
}
