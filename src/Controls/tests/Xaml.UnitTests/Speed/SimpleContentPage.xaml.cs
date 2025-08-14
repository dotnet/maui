using System.Diagnostics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class SimpleContentPageCode : ContentPage
{
	public SimpleContentPageCode()
	{
		Content = new Label
		{
			Text = "Hello, Microsoft.Maui.Controls!",
			VerticalOptions = LayoutOptions.CenterAndExpand,
			HorizontalOptions = LayoutOptions.CenterAndExpand
		};
	}
}

public partial class SimpleContentPage : ContentPage
{
	public SimpleContentPage() => InitializeComponent();

	[TestFixture]
	public class Tests
	{
		[Test]
		[Ignore(nameof(XamlCIs20TimesFasterThanXaml))]
		public void XamlCIs20TimesFasterThanXaml()
		{
			var swXamlC = new Stopwatch();
			var swXaml = new Stopwatch();

			swXamlC.Start();
			for (var i = 0; i < 1000; i++)
				new SimpleContentPage(XamlInflator.XamlC);
			swXamlC.Stop();

			swXaml.Start();
			for (var i = 0; i < 1000; i++)
				new SimpleContentPage(XamlInflator.Runtime);
			swXaml.Stop();

			Assert.Less(swXamlC.ElapsedMilliseconds * 20, swXaml.ElapsedMilliseconds);
		}

		[Test]
		[Ignore(nameof(XamlCIsNotMuchSlowerThanCode))]
		public void XamlCIsNotMuchSlowerThanCode()
		{
			var swXamlC = new Stopwatch();
			var swCode = new Stopwatch();

			swXamlC.Start();
			for (var i = 0; i < 1000; i++)
				new SimpleContentPage(XamlInflator.XamlC);
			swXamlC.Stop();

			swCode.Start();
			for (var i = 0; i < 1000; i++)
				new SimpleContentPageCode();
			swCode.Stop();

			Assert.LessOrEqual(swXamlC.ElapsedMilliseconds * .2, swCode.ElapsedMilliseconds);
		}
	}
}