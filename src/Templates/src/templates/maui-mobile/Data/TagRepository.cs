using MauiApp._1.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MauiApp._1.Data;

/// <summary>
/// Repository class for managing tags in the database.
/// </summary>
public class TagRepository
{
	private bool _hasBeenInitialized = false;
	private readonly ILogger _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="TagRepository"/> class.
	/// </summary>
	/// <param name="logger">The logger instance.</param>
	public TagRepository(ILogger<TagRepository> logger)
	{
		_logger = logger;
	}

	/// <summary>
	/// Initializes the database connection and creates the Tag and ProjectsTags tables if they do not exist.
	/// </summary>
	private async Task Init()
	{
		if (_hasBeenInitialized)
			return;

		await using var connection = new SqliteConnection(Constants.DatabasePath);
		await connection.OpenAsync();

		try
		{
			var createTableCmd = connection.CreateCommand();
			createTableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Tag (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Color TEXT NOT NULL
            );";
			await createTableCmd.ExecuteNonQueryAsync();

			createTableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS ProjectsTags (
                ProjectID INTEGER NOT NULL,
                TagID INTEGER NOT NULL,
                PRIMARY KEY(ProjectID, TagID)
            );";
			await createTableCmd.ExecuteNonQueryAsync();
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Error creating tables");
			throw;
		}

		_hasBeenInitialized = true;
	}

	/// <summary>
	/// Retrieves a list of all tags from the database.
	/// </summary>
	/// <returns>A list of <see cref="Tag"/> objects.</returns>
	public async Task<List<Tag>> ListAsync()
	{
		await Init();
		await using var connection = new SqliteConnection(Constants.DatabasePath);
		await connection.OpenAsync();

		var selectCmd = connection.CreateCommand();
		selectCmd.CommandText = "SELECT * FROM Tag";
		var tags = new List<Tag>();

		await using var reader = await selectCmd.ExecuteReaderAsync();
		while (await reader.ReadAsync())
		{
			tags.Add(new Tag
			{
				ID = reader.GetInt32(0),
				Title = reader.GetString(1),
				Color = reader.GetString(2)
			});
		}

		return tags;
	}

	/// <summary>
	/// Retrieves a list of tags associated with a specific project.
	/// </summary>
	/// <param name="projectID">The ID of the project.</param>
	/// <returns>A list of <see cref="Tag"/> objects.</returns>
	public async Task<List<Tag>> ListAsync(int projectID)
	{
		await Init();
		await using var connection = new SqliteConnection(Constants.DatabasePath);
		await connection.OpenAsync();

		var selectCmd = connection.CreateCommand();
		selectCmd.CommandText = @"
        SELECT t.*
        FROM Tag t
        JOIN ProjectsTags pt ON t.ID = pt.TagID
        WHERE pt.ProjectID = @ProjectID";
		selectCmd.Parameters.AddWithValue("ProjectID", projectID);

		var tags = new List<Tag>();

		await using var reader = await selectCmd.ExecuteReaderAsync();
		while (await reader.ReadAsync())
		{
			tags.Add(new Tag
			{
				ID = reader.GetInt32(0),
				Title = reader.GetString(1),
				Color = reader.GetString(2)
			});
		}

		return tags;
	}

	/// <summary>
	/// Retrieves a specific tag by its ID.
	/// </summary>
	/// <param name="id">The ID of the tag.</param>
	/// <returns>A <see cref="Tag"/> object if found; otherwise, null.</returns>
	public async Task<Tag?> GetAsync(int id)
	{
		await Init();
		await using var connection = new SqliteConnection(Constants.DatabasePath);
		await connection.OpenAsync();

		var selectCmd = connection.CreateCommand();
		selectCmd.CommandText = "SELECT * FROM Tag WHERE ID = @id";
		selectCmd.Parameters.AddWithValue("@id", id);

		await using var reader = await selectCmd.ExecuteReaderAsync();
		if (await reader.ReadAsync())
		{
			return new Tag
			{
				ID = reader.GetInt32(0),
				Title = reader.GetString(1),
				Color = reader.GetString(2)
			};
		}

		return null;
	}

	/// <summary>
	/// Saves a tag to the database. If the tag ID is 0, a new tag is created; otherwise, the existing tag is updated.
	/// </summary>
	/// <param name="item">The tag to save.</param>
	/// <returns>The ID of the saved tag.</returns>
	public async Task<int> SaveItemAsync(Tag item)
	{
		await Init();
		await using var connection = new SqliteConnection(Constants.DatabasePath);
		await connection.OpenAsync();

		var saveCmd = connection.CreateCommand();
		if (item.ID == 0)
		{
			saveCmd.CommandText = @"
            INSERT INTO Tag (Title, Color) VALUES (@Title, @Color);
            SELECT last_insert_rowid();";
		}
		else
		{
			saveCmd.CommandText = @"
            UPDATE Tag SET Title = @Title, Color = @Color WHERE ID = @ID";
			saveCmd.Parameters.AddWithValue("@ID", item.ID);
		}

		saveCmd.Parameters.AddWithValue("@Title", item.Title);
		saveCmd.Parameters.AddWithValue("@Color", item.Color);

		var result = await saveCmd.ExecuteScalarAsync();
		if (item.ID == 0)
		{
			item.ID = Convert.ToInt32(result);
		}

		return item.ID;
	}

	/// <summary>
	/// Saves a tag to the database and associates it with a specific project.
	/// </summary>
	/// <param name="item">The tag to save.</param>
	/// <param name="projectID">The ID of the project.</param>
	/// <returns>The number of rows affected.</returns>
	public async Task<int> SaveItemAsync(Tag item, int projectID)
	{
		await Init();
		await SaveItemAsync(item);

		var isAssociated = await IsAssociated(item, projectID);
		if (isAssociated)
		{
			return 0; // No need to save again if already associated
		}

		await using var connection = new SqliteConnection(Constants.DatabasePath);
		await connection.OpenAsync();

		var saveCmd = connection.CreateCommand();
		saveCmd.CommandText = @"
        INSERT INTO ProjectsTags (ProjectID, TagID) VALUES (@projectID, @tagID)";
		saveCmd.Parameters.AddWithValue("@projectID", projectID);
		saveCmd.Parameters.AddWithValue("@tagID", item.ID);

		return await saveCmd.ExecuteNonQueryAsync();
	}

	/// <summary>
	/// Checks if a tag is already associated with a specific project.
	/// </summary>
	/// <param name="item">The tag to save.</param>
	/// <param name="projectID">The ID of the project.</param>
	/// <returns>If tag is already associated with this project</returns>
	async Task<bool> IsAssociated(Tag item, int projectID)
	{
		await Init();

		await using var connection = new SqliteConnection(Constants.DatabasePath);
		await connection.OpenAsync();

		// First check if the association already exists
		var checkCmd = connection.CreateCommand();
		checkCmd.CommandText = @"
			SELECT COUNT(*) FROM ProjectsTags 
			WHERE ProjectID = @projectID AND TagID = @tagID";
		checkCmd.Parameters.AddWithValue("@projectID", projectID);
		checkCmd.Parameters.AddWithValue("@tagID", item.ID);

		int existingCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

		return existingCount != 0;
	}

	/// <summary>
	/// Deletes a tag from the database.
	/// </summary>
	/// <param name="item">The tag to delete.</param>
	/// <returns>The number of rows affected.</returns>
	public async Task<int> DeleteItemAsync(Tag item)
	{
		await Init();
		await using var connection = new SqliteConnection(Constants.DatabasePath);
		await connection.OpenAsync();

		var deleteCmd = connection.CreateCommand();
		deleteCmd.CommandText = "DELETE FROM Tag WHERE ID = @id";
		deleteCmd.Parameters.AddWithValue("@id", item.ID);

		return await deleteCmd.ExecuteNonQueryAsync();
	}

	/// <summary>
	/// Deletes a tag from a specific project in the database.
	/// </summary>
	/// <param name="item">The tag to delete.</param>
	/// <param name="projectID">The ID of the project.</param>
	/// <returns>The number of rows affected.</returns>
	public async Task<int> DeleteItemAsync(Tag item, int projectID)
	{
		await Init();
		await using var connection = new SqliteConnection(Constants.DatabasePath);
		await connection.OpenAsync();

		var deleteCmd = connection.CreateCommand();
		deleteCmd.CommandText = "DELETE FROM ProjectsTags WHERE ProjectID = @projectID AND TagID = @tagID";
		deleteCmd.Parameters.AddWithValue("@projectID", projectID);
		deleteCmd.Parameters.AddWithValue("@tagID", item.ID);

		return await deleteCmd.ExecuteNonQueryAsync();
	}

	/// <summary>
	/// Drops the Tag and ProjectsTags tables from the database.
	/// </summary>
	public async Task DropTableAsync()
	{
		await Init();
		await using var connection = new SqliteConnection(Constants.DatabasePath);
		await connection.OpenAsync();

		var dropTableCmd = connection.CreateCommand();
		dropTableCmd.CommandText = "DROP TABLE IF EXISTS Tag";
		await dropTableCmd.ExecuteNonQueryAsync();

		dropTableCmd.CommandText = "DROP TABLE IF EXISTS ProjectsTags";
		await dropTableCmd.ExecuteNonQueryAsync();

		_hasBeenInitialized = false;
	}
}