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

You can register the in-memory database service in your application by passing your service collection like this. `DatabaseServiceRegistrar.AddInMemoryDBService(IServiceCollection service);`  
By default, it will register the necessary dependecies, FileManager and SearchManager, and the main Database class.

or

You can implement your own custom IFileManager and ISearchManager and either pass it along as parameters as below. Or if an instance is already registered in the DI container, it will be used.
`DatabaseServiceRegistrar.AddInMemoryDBService(IServiceCollection services,ISearchManager searchManager,IDiskManager diskManager);`

### 2. Manually creating a Database Instance

Since the database class is public and available, you can directly create a database object and start using it:  
`var database = new Database(ISearchManager searchManager, IDiskManager diskManager);`

See [Reference Page](API.md) for documentation

## Future Plans

- Concurreny using locks
- Exception handling for when a transaction fails
- More complex queries and reference keys

# Links

Nuget Package = https://www.nuget.org/packages/Custom_In-Memory_Database  
Git Repo = https://github.com/Thet-Wai-123/In-Memory_Database

# Acknowledgments

This project uses the BTree library by CodeExMachina, licensed under GPLv3.0.
