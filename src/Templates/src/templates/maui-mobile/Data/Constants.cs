namespace MauiApp._1.Data;

public static class Constants
{
	public const string DatabaseFilename = "AppSQLite.db3";

	public static string DatabasePath =>
		$"Data Source={Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename)}";
}