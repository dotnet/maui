namespace UITest.Core
{
	public interface IApp : IDisposable
	{
		IConfig Config { get; }
		IUIElementQueryable Query { get; }
		ApplicationState AppState { get; }

		IUIElement FindElement(string id);
		IUIElement FindElement(IQuery query);
		IReadOnlyCollection<IUIElement> FindElements(string id);
		IReadOnlyCollection<IUIElement> FindElements(IQuery query);
		string ElementTree { get; }

		ICommandExecution CommandExecutor { get; }
	}

	public interface IScreenshotSupportedApp : IApp
	{
		FileInfo Screenshot(string fileName);
		byte[] Screenshot();
	}

	public static class AppExtensions
	{
		public static void Click(this IApp app, float x, float y)
		{
			app.CommandExecutor.Execute("click", new Dictionary<string, object>()
			{
				{ "x", x },
				{ "y", y }
			});
		}


		internal static T As<T>(this IApp app)
			where T : IApp
		{
			if (app is not T derivedApp)
				throw new NotImplementedException($"The app '{app}' does not implement '{typeof(T).FullName}'.");

			return derivedApp;
		}
	}

	public static class ScreenshotSupportedAppExtensions
	{
		public static FileInfo Screenshot(this IApp app, string fileName) =>
			app.As<IScreenshotSupportedApp>().Screenshot(fileName);

		public static byte[] Screenshot(this IApp app) =>
			app.As<IScreenshotSupportedApp>().Screenshot();
	}
}
