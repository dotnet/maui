using System.Diagnostics;
using Xunit;

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


	public class Tests
	{
		[Fact(Skip = "Performance test")]
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

			Assert.True(swXamlC.ElapsedMilliseconds * 20 < swXaml.ElapsedMilliseconds); // TODO: Verify this assertion makes sense
		}

		[Fact(Skip = "Performance test")]
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

			Assert.True(swXamlC.ElapsedMilliseconds * .2 <= swCode.ElapsedMilliseconds); // TODO: Verify this assertion makes sense
		}
	}
}