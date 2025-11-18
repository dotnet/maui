using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class HotReloadStaticResourceException : ContentPage
{
	public HotReloadStaticResourceException()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			Controls.Internals.ResourceLoader.ResourceProvider2 = null;
			Controls.Internals.ResourceLoader.ExceptionHandler2 = null;
		}

#if DEBUG
		[Test]
		public void MissingResourceExceptionAreHandled([Values(XamlInflator.Runtime, XamlInflator.SourceGen)] XamlInflator inflator)
		{
			bool handled = false;
			Controls.Internals.ResourceLoader.ExceptionHandler2 = (ex) =>
			{
				var (exception, filepath) = ex;
				Assert.AreEqual("HotReloadStaticResourceException.xaml", filepath);
				if (exception is XamlParseException xpe && xpe.Message.Contains("StaticResource not found for key MissingResource", System.StringComparison.Ordinal))
				{
					handled = true;
					Assert.AreEqual(13, xpe.XmlInfo.LinePosition);
				}

			};
			var page = new HotReloadStaticResourceException(inflator);
			Assert.IsTrue(handled, "Exception was not handled");
		}
#endif
	}
}