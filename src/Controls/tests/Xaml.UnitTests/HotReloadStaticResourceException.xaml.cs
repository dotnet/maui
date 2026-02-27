using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class HotReloadStaticResourceException : ContentPage
{
	public HotReloadStaticResourceException()
	{
		InitializeComponent();
	}

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		protected internal override void Setup()
		{
			base.Setup();
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		protected internal override void TearDown()
		{
			AppInfo.SetCurrent(null);
			Controls.Internals.ResourceLoader.ResourceProvider2 = null;
			Controls.Internals.ResourceLoader.ExceptionHandler2 = null;
			base.TearDown();
		}

#if DEBUG
		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		internal void MissingResourceExceptionAreHandled(XamlInflator inflator)
		{
			bool handled = false;
			Controls.Internals.ResourceLoader.ExceptionHandler2 = (ex) =>
			{
				var (exception, filepath) = ex;
				Assert.Equal("HotReloadStaticResourceException.xaml", filepath);
				if (exception is XamlParseException xpe && xpe.Message.Contains("StaticResource not found for key MissingResource", System.StringComparison.Ordinal))
				{
					handled = true;
					Assert.Equal(13, xpe.XmlInfo.LinePosition);
				}

			};
			var page = new HotReloadStaticResourceException(inflator);
			Assert.True(handled, "Exception was not handled");
		}
#else
		[Fact(Skip = "This test runs only in debug")]
		public void MissingResourceExceptionAreHandled() { }
#endif
	}
}