# Database Class

### `Database(searchManager, diskManager, saveOnDispose, diskLocation)` - Constructor

| Name                    | Type             | Description                      |
| ----------------------- | ---------------- | -------------------------------- |
| searchManager           | `ISearchManager` | Implementation of ISearchManager |
| diskManager             | `IDiskManager`   | Implementation of IDiskManager   |
| saveOnDispose(Optional) | `bool`           | If it should save when closing   |
| diskLocation(Optional)  | `string`         | Location to store when saving    |

### `Tables` - Property

Returns all tables in the database as a read-only dictionary.

### `CreateTable(tableName, columnNames, columnTypes, rows)` - Method

| Name           | Type            | Description                                             |
| -------------- | --------------- | ------------------------------------------------------- |
| tableName      | `string`        | Name of the new table.                                  |
| columnNames    | `List<string>`  | List of column names names.                             |
| columnTypes    | `List<Type>`    | List of column types corresponding to the column names. |
| rows(Optional) | `List<DataRow>` | List of initial rows to insert into the table.          |

### `this[string tableName]` - Indexer

Access table by name

| Name      | Type     | Description                      |
| --------- | -------- | -------------------------------- |
| tableName | `string` | The name of the table to access. |

### `Begin()` - Method

Starts an explicit transaction with assigned transaction id, that is tied with the async context. However, by default, if you call a CRUD method, affecting rows, it'll automatically and implicitly start new transaction and commit in one-line.

Does not support recursive calls.

### `Commit()` - Method

Commit on-going transaction. Throws exception if there is no on-going transactions.

### `Abort()` - Method

Abort and revert the current transaction.

### `SaveToDisk()` - Method

Manual call to save to disk

### `LoadFromDisk()` - Method

Loads the database from the specified disk location. When loading the all the rows will be given an xid of 0, which is a special id for all transactions that came from earlier snapshot.

### `ClearDb()` - Method

Clears tables in memory

### `SetDiskLocation(location)` - Method

Manually change where to save in

| Name     | Type     | Description  |
| -------- | -------- | ------------ |
| location | `string` | New Location |

<br>

# Datatable Class

### `DataTable(Name, columnNames, columnTypes, searchManager)` - Constructor

| Name          | Type             | Description                                |
| ------------- | ---------------- | ------------------------------------------ |
| Name          | `string`         | Table Name                                 |
| columnNames   | `List<string>`   | Columns names, must match with columnTypes |
| columnTypes   | `List<Type>`     | Types of columns                           |
| searchManager | `ISearchManager` | Implementation of ISearchManager           |

### `AddColumn(name, type)` - Async Method

| Name | Type     | Description |
| ---- | -------- | ----------- |
| name | `string` | column name |
| type | `Type`   | column type |

### `RemoveColumn(name)` - Async Method

| Name | Type     | Description |
| ---- | -------- | ----------- |
| name | `string` | column name |

### `AddRow(newRow)` - Async Method

| Name   | Type      | Description |
| ------ | --------- | ----------- |
| newRow | `DataRow` | row to add  |

### `UpdateRow(conditions, targetColumn, newValue)` - Async Method

| Name         | Type               | Description                   |
| ------------ | ------------------ | ----------------------------- |
| conditions   | `SearchConditions` | conditions for rows to update |
| targetColumn | `String`           | the column target to modify   |
| newRow       | `Object`           | new value                     |

### `RemoveRow(toBeRemovedrows)` - Async Method

| Name            | Type             | Description                                |
| --------------- | ---------------- | ------------------------------------------ |
| toBeRemovedrows | `List<DataRow> ` | rows to remove, usually paired with Search |

### `ClearTable()` - Async Method

Delete all rows

### `VaccumInActiveRows()` - Async Method

Delete all rows versions that are already deleted. Acquired an access exclusive lock for the period and filters out rows to only visible version.

### `CreateIndex(targetColumn)` - Async Method

| Name         | Type   | Description                         |
| ------------ | ------ | ----------------------------------- |
| targetColumn | string | key column to create index based on |

### `DeleteIndex(targetColumn)` - Async Method

| Name         | Type   | Description   |
| ------------ | ------ | ------------- |
| targetColumn | string | target column |

### `Search(conditions)` - Async Method

| Name       | Type             | Description                                               |
| ---------- | ---------------- | --------------------------------------------------------- |
| conditions | SearchConditions | Implementation of SearchConditions(ColumnName, Op, Value) |

Returns Task<ReadOnlyCollection\<DataRow\>>

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
