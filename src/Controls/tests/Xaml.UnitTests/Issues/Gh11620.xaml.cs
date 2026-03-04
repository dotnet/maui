using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh11620 : ContentPage
{
	public Gh11620() => InitializeComponent();


	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void AddValueType(XamlInflator inflator)
		{
			var layout = new Gh11620(inflator);
			var arr = layout.Resources["myArray"];
			Assert.IsType<object[]>(arr);
			Assert.Equal(3, ((object[])arr).Length);
			Assert.Equal(32, ((object[])arr)[2]);
		}
	}
}