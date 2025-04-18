﻿using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace In_Memory_Database
{
    public static class DatabaseServiceRegistrar
    {
        public static void AddInMemoryDBService(
            IServiceCollection services,
            ISearchManager? searchManager = null,
            IDiskManager? fileManager = null
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
