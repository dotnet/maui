namespace Microsoft.Maui.Hosting
{
	public static class AppHostExtensions
	{
		public static void SetServiceProvider(this IAppHost host, App app)
		{
			//we do this here because we can't inject the provider on the App ctor
			//before we register the user ConfigureServices should this live in IApp ?
			app?.SetServiceProvider(host.Services);
		}
	}
}