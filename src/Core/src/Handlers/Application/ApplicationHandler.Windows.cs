namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, UI.Xaml.Application>
	{
		public static void MapRequestTerminate(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.NativeView.Exit();
		}
	}
}