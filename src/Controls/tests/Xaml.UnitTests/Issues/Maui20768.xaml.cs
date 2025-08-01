using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui20768
{
	public Maui20768() => InitializeComponent();

	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void BindingsDoNotResolveStaticProperties([Values] XamlInflator inflator)
		{
			var page = new Maui20768(inflator);
			page.TitleLabel.BindingContext = new ViewModel20768();
			Assert.Null(page.TitleLabel.Text);
		}
	}
}

public class ViewModel20768
{
	public static string Title => "Title from static property";
}