---
name: maui-sqlite-database
description: >
  Add SQLite local database storage to .NET MAUI apps using sqlite-net-pcl.
  Covers data models with ORM attributes, async database service with lazy init,
  DI registration, WAL mode, and file management. Works with any UI pattern
  (XAML/MVVM, C# Markup, MauiReactor).
  USE FOR: "SQLite database", "local database", "sqlite-net-pcl", "offline storage",
  "CRUD database", "database service", "WAL mode", "CreateTableAsync",
  "local data persistence", "ORM attributes".
  DO NOT USE FOR: REST API data fetching (use maui-rest-api), secure credential storage
  (use maui-secure-storage), or file picking (use maui-file-handling).
---

# SQLite Local Database for .NET MAUI

## NuGet Package

Install **sqlite-net-pcl** by praeclarum:

```xml
<PackageReference Include="sqlite-net-pcl" Version="1.9.*" />
<PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.*" />
```

> [!WARNING]
> The correct package is **sqlite-net-pcl** (author: praeclarum). Do NOT use
> `Microsoft.Data.Sqlite`, `sqlite-net`, or `SQLitePCL.raw` directly — they
> are different libraries with incompatible APIs.

---

## 1. Constants Class

```csharp
public static class Constants
{
    public const string DatabaseFilename = "app.db3";

    public const SQLite.SQLiteOpenFlags Flags =
        SQLite.SQLiteOpenFlags.ReadWrite |
        SQLite.SQLiteOpenFlags.Create |
        SQLite.SQLiteOpenFlags.SharedCache;

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}
```

- **ReadWrite | Create | SharedCache** — standard flags for mobile apps.
- Use `FileSystem.AppDataDirectory` (not `Environment.GetFolderPath`) for
  cross-platform correctness on all MAUI targets.

---

## 2. Data Model

```csharp
using SQLite;

public class TodoItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(250)]
    public string Name { get; set; } = string.Empty;

    public bool Done { get; set; }
}
```

Key attributes: `[PrimaryKey]`, `[AutoIncrement]`, `[MaxLength(n)]`,
`[Indexed]`, `[Ignore]`, `[Column("name")]`, `[Unique]`, `[NotNull]`,
`[Table("name")]`.

---

## 3. Database Service

Use the **lazy async initialization** pattern — the connection is created once,
on first access, and all callers await the same instance:

```csharp
using SQLite;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;

    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database is not null)
            return _database;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await _database.ExecuteAsync("PRAGMA journal_mode=WAL;");
        await _database.CreateTableAsync<TodoItem>();
        return _database;
    }

    public async Task<List<TodoItem>> GetItemsAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<TodoItem>().ToListAsync();
    }

    public async Task<List<TodoItem>> GetItemsAsync(bool done)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<TodoItem>().Where(i => i.Done == done).ToListAsync();
    }

    public async Task<TodoItem?> GetItemAsync(int id)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<TodoItem>().Where(i => i.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveItemAsync(TodoItem item)
    {
        var db = await GetDatabaseAsync();
        return item.Id != 0
            ? await db.UpdateAsync(item)
            : await db.InsertAsync(item);
    }

    public async Task<int> DeleteItemAsync(TodoItem item)
    {
        var db = await GetDatabaseAsync();
        return await db.DeleteAsync(item);
    }

    public async Task CloseConnectionAsync()
    {
        if (_database is not null)
        {
            await _database.CloseAsync();
            _database = null;
        }
    }
}
```

---

## 4. DI Registration

Register as a **singleton** in `MauiProgram.cs`:

```csharp
builder.Services.AddSingleton<DatabaseService>();
```

Singleton because `SQLiteAsyncConnection` is not thread-safe for multiple
instances. A single connection with WAL handles concurrent reads efficiently.

Inject into view models or pages:

```csharp
public class TodoListViewModel(DatabaseService database)
{
    private readonly DatabaseService _database = database;
}
```

---

## 5. WAL (Write-Ahead Logging)

Enabled in `GetDatabaseAsync()` via `PRAGMA journal_mode=WAL;`.

- Readers do not block writers and vice versa.
- Better performance for concurrent read/write workloads.
- Recommended for all MAUI apps.

---

## 6. Database File Management

**Always close the connection before moving, copying, or deleting the file.**

```csharp
// Delete database
await databaseService.CloseConnectionAsync();
if (File.Exists(Constants.DatabasePath))
    File.Delete(Constants.DatabasePath);

// Export / backup
await databaseService.CloseConnectionAsync();
File.Copy(Constants.DatabasePath,
    Path.Combine(FileSystem.CacheDirectory, "backup.db3"), overwrite: true);
```

Platform paths for `FileSystem.AppDataDirectory`:

| Platform     | Location                                     |
|--------------|----------------------------------------------|
| Android      | `/data/user/0/{package}/files`               |
| iOS          | App sandbox `Library` (iCloud-backed)        |
| Mac Catalyst | App sandbox `Library/Application Support`    |
| Windows      | `%LOCALAPPDATA%\Packages\{id}\LocalState`    |

On iOS, use `FileSystem.CacheDirectory` to exclude from iCloud backup.

---

## 7. Common Patterns

```csharp
// Raw SQL
var items = await db.QueryAsync<TodoItem>(
    "SELECT * FROM TodoItem WHERE Done = ?", 1);

// Multiple tables — add in GetDatabaseAsync()
await _database.CreateTableAsync<TodoItem>();
await _database.CreateTableAsync<Category>();

// Transactions
await db.RunInTransactionAsync(conn =>
{
    conn.Insert(item1);
    conn.Insert(item2);
});

// Drop and recreate
await db.DropTableAsync<TodoItem>();
await db.CreateTableAsync<TodoItem>();
```

---

## Checklist

- [ ] Install `sqlite-net-pcl` + `SQLitePCLRaw.bundle_green`
- [ ] Create `Constants` with `DatabasePath` using `FileSystem.AppDataDirectory`
- [ ] Define models with `[PrimaryKey, AutoIncrement]`
- [ ] Create `DatabaseService` with lazy async init and CRUD methods
- [ ] Enable WAL via `PRAGMA journal_mode=WAL`
- [ ] Register `DatabaseService` as singleton in DI
- [ ] Close connections before moving or deleting the database file
