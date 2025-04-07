using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using In_Memory_Database;
using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace In_Memory_Database_Testing.Integration_Tests
{
    public class DependencyInjectionTest
    {
        [Fact]
        public void DependencyInjectionWithoutGivingImplementations()
        {
            //Arrange
            //Create a new service collection
            var serviceCollection = new ServiceCollection();

            //Act
            DatabaseServiceRegistrar.AddInMemoryDBService(serviceCollection);
            var _serviceProvider = serviceCollection.BuildServiceProvider();

            var db = _serviceProvider.GetRequiredService<Database>();
            var searchManager = _serviceProvider.GetService<ISearchManager>();
            var diskManager = _serviceProvider.GetService<IDiskManager>();

            //Assert
            Assert.NotNull(db);
            Assert.NotNull(searchManager);
            Assert.NotNull(diskManager);
        }

        [Fact]
        public void DependencyInjectionWithGivenImplementations()
        {
            //Arrange
            //Create a new service collection
            var serviceCollection = new ServiceCollection();

            var mockSearchManager = new Mock<ISearchManager>();
            var mockDiskManager = new Mock<IDiskManager>();

            //Act
            serviceCollection.AddSingleton(mockSearchManager.Object);
            serviceCollection.AddSingleton(mockDiskManager.Object);
            DatabaseServiceRegistrar.AddInMemoryDBService(serviceCollection);
            var _serviceProvider = serviceCollection.BuildServiceProvider();

            var db = _serviceProvider.GetRequiredService<Database>();
            var searchManager = _serviceProvider.GetService<ISearchManager>();
            var diskManager = _serviceProvider.GetService<IDiskManager>();

            //Assert
            //If they are the instance, then it means that the startup didn't add the default implementation and using and provided implementation whatever it may be
            Assert.NotNull(db);
            Assert.Same(mockSearchManager.Object, searchManager);
            Assert.Same(mockDiskManager.Object, diskManager);
        }
    }
}
