namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	static class ApplicationExtensions
	{
		public static Window LoadPage(this Application app, Page page)
		{
			app.MainPage = page;

			return ((IApplication)app).CreateWindow(null) as Window;
		}
	}
}