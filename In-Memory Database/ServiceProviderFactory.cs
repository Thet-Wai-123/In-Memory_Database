namespace In_Memory_Database
{
    //Note: This is a locator anti-pattern, so only using this class for the jsonconstructor to get its dependecies. Was not able to find any good solutions to deal with this.
    //Consequences include existing two instances of the search manager for singletons.
    public class ServiceProviderFactory
    {
        public static IServiceProvider ServiceProvider { get; }

        static ServiceProviderFactory()
        {
            var startup = new Startup();
            ServiceCollection sc = new();
            startup.ConfigureServices(sc);
            ServiceProvider = sc.BuildServiceProvider();
        }
    }
}
