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

		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		internal void MissingResourceThrowsEvenWithExceptionHandler(XamlInflator inflator)
		{
			// Issue #23903: StaticResourceExtension should always throw when resource is not found,
			// regardless of whether an exception handler is present. This ensures consistent behavior
			// across debug/release and prevents crashes when relaunching apps.
			bool handlerCalled = false;
			Controls.Internals.ResourceLoader.ExceptionHandler2 = (ex) =>
			{
				handlerCalled = true;
			};

			// Exception should be thrown even though handler is present
			var exception = Assert.Throws<XamlParseException>(() => new HotReloadStaticResourceException(inflator));

			// Verify handler was called (for logging purposes)
			Assert.True(handlerCalled, "Exception handler should be called before throwing");

			// Verify the exception contains the expected message
			Assert.Contains("StaticResource not found for key", exception.Message, System.StringComparison.Ordinal);
		}
	}
}