using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
{
	[TestFixture]
	public class NavigationTests : PlatformTestFixture
	{
		[Test, Category("Navigation"), Category("Dispose")]
		[Description("Multiple calls to NavigationRenderer.Dispose shouldn't crash")]
		public void NavigationRendererDoubleDisposal()
		{
			var root = new ContentPage()
			{
				Title = "root",
				Content = new Label { Text = "Hello" }
			};

			Device.InvokeOnMainThreadAsync(() => { 
				var navPage = new NavigationPage(root);
				var renderer = GetRenderer(navPage);

				// Calling Dispose more than once should be fine
				renderer.Dispose();
				renderer.Dispose();
			});
		}
	}
}