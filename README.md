# Custom In-Memory Database

## About

This is a light-weight, in-memory database built as part of a research project at Cal Poly Pomona. It can especially be useful for testing purposes where you want isolated databases with ease of set up, and simulate real database functions.

Basic functionalities include all CRUD operations and aims to follow ACID architecture as closely as possible, inspired by PostgreSQL's implementations. It supports concurrency using Multi-Version Control Concurrency and async read write locks to simulate Read Commited isolation level. It can also perform type-checks, and save to disk on crash by default, and create a B-tree indexing to speed up query search.

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

## Limitations & Potential Plans

- Atomicity is only supported for changing rows but not datatable structure.
- Only includes simple add and retrieve, and no foreign keys or custom constraints.
- Search parameters are simple and only allow one search condition.

## Exception Handling

Most of the exceptions are still recoverable, meaning when an exception is thrown, the current transaction will abort, reverting to its previous working state. However, exceptions relating to File or locks, specifically `LockNotReleasedException` which is a custom exception type, will have persisting impacts, so you should probably restart it.

# Links

Nuget Package = https://www.nuget.org/packages/Custom_In-Memory_Database  
Git Repo = https://github.com/Thet-Wai-123/In-Memory_Database

# Acknowledgments

BTree library by CodeExMachina, licensed under GPLv3.0.

AsyncReaderWriterLock by .NEXT, licensed under MIT.
