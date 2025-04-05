# Database Class

### `Database(searchManager, diskManager, saveOnDispose, diskLocation)` - Constructor

| Name                    | Type           | Description                      |
| ----------------------- | -------------- | -------------------------------- |
| searchManager           | `ISearchManager` | Implementation of ISearchManager |
| diskManager             | `IDiskManager`   | Implementation of IDiskManager   |
| saveOnDispose(Optional) | `bool`           | If it should save when closing   |
| diskLocation(Optional)  | `string`         | Location to store when saving    |

### `Tables` - Property

Returns all tables in the database as a read-only dictionary.

### `CreateTable(tableName, columnNames, columnTypes, rows)` - Method

| Name             | Type            | Description                                             |
| ---------------- | --------------- | ------------------------------------------------------- |
| tableName     | `string`        | Name of the new table.                                  |
| columnNames    | `List<string>`  | List of column names names.                             |
| columnTypes    | `List<Type>`    | List of column types corresponding to the column names. |
| rows(Optional) | `List<DataRow>` | List of initial rows to insert into the table.          |

### `this[string tableName]` - Indexer 
Access table by name

| Name        | Type                   | Description                                     |
| ----------- | ---------------------- | ----------------------------------------------- |
| tableName | `string`               | The name of the table to access.              |

### `SaveToDisk()` - Method

Manual call to save to disk



### `LoadFromDisk()` - Method

Loads the database from the specified disk location.

### `ClearDb()` - Method

Clears tables in memory

### `SetDiskLocation(location)` - Method

Manually change where to save in

| Name       | Type     | Description                                           |
| ---------- | -------- | ----------------------------------------------------- |
| location | `string` | New Location |
  

<br> 



# Datatable Class


### `DataTable(Name, columnNames, columnTypes, searchManager)` - Constructor

| Name           | Type            | Description         |
| -------------- | --------------- | ------------------- |
| Name           | `string`          | Table Name            |
| columnNames    | `List<string>`    | Columns names, must match with columnTypes            |
| columnTypes    | `List<Type>`      | Types of columns            |
| searchManager  | `ISearchManager`  | Implementation of ISearchManager            |

### `AddColumn(name, type)` - Method

| Name           | Type            | Description         |
| -------------- | --------------- | ------------------- |
| name           | `string`          | column name         |
| type           | `Type`            | column type         |

### `RemoveColumn(name)` - Method

| Name           | Type            | Description         |
| -------------- | --------------- | ------------------- |
| name           | `string`          | column name         |

### `AddRow(newRow)` - Method

| Name           | Type            | Description         |
| -------------- | --------------- | ------------------- |
| newRow         | `DataRow`         | row to add          |

### `RemoveRow(toBeRemovedrows)` - Method

| Name           | Type            | Description         |
| -------------- | --------------- | ------------------- |
| toBeRemovedrows| `List<DataRow> `  | rows to remove, usually paired with Search      |

### `ClearTable()` - Method

Delete all rows      

### `CreateIndex(targetColumn)` - Method

| Name           | Type            | Description         |
| -------------- | --------------- | ------------------- |
| targetColumn   | string          | key column to create index based on       |

### `DeleteIndex(targetColumn)` - Method

| Name           | Type            | Description         |
| -------------- | --------------- | ------------------- |
| targetColumn   | string          | target column       |

### `Search(conditions)` - Method

| Name           | Type            | Description         |
| -------------- | --------------- | ------------------- |
| conditions     | SearchConditions| Implementation of SearchConditions(ColumnName, Op, Value)     |

### `Size` - Property

Returns "columns x rows"   

### `Width` - Property

Returns number of columns  

### `Height` - Property

Return number of rows      

### `ColumnNames` - Property

Return a `ReadOnlyCollection<string>` of column names 

### `ColumnTypes` - Property

Return a `ReadOnlyCollection<Type>` of column types 
