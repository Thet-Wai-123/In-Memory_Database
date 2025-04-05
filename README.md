
# Custom In-Memory Database


## About 
This is a custom in-memory database built for learning purposes as part of a senior project, aiming to follow ACID architecture as closely as possible.

For use cases, it could be used for for simple and inspectable database system in small-scale projects or testing purposes.

It supports additional features such as B-tree indexing and data persistence to disk. Future improvements include support for concurrency and atomic transactions.




## Installation

On the command line interface:  
`dotnet add package Custom_In-Memory_Database`

## How to Setup

### 1. Dependency Injection

You can register the in-memory database service in your application by adding `DatabaseServiceRegistrar.AddInMemoryDBService(IServiceCollection service);`

or

You can implement your own custom IFileManager and ISearchManager; otherwise, it'll register the implemention in this project into the service collection.
`DatabaseServiceRegistrar.AddInMemoryDBService(IServiceCollection services,ISearchManager searchManager,IFileManager fileManager);`

### 2. Manually creating a Database Instance

Since the database class is public and available, you can directly create a database object and start using it:  
`var database = new Database(ISearchManager searchManager, IFileManager fileManager);`

See [Reference Page](API.md) for documentation

## Future Plans

- Concurreny using locks
- Exception handling for when a transaction fails
- More complex queries and reference keys


# Acknowledgments

This project uses the BTree library by CodeExMachina, licensed under GPLv3.0.