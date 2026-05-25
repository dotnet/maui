using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class SetStyleIdFromXName : ContentPage
{
	public SetStyleIdFromXName() => InitializeComponent();


	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void SetStyleId(XamlInflator inflator)
		{
			var layout = new SetStyleIdFromXName(inflator);
			Assert.Equal("label0", layout.label0.StyleId);
			Assert.Equal("foo", layout.label1.StyleId);
			Assert.Equal("bar", layout.label2.StyleId);
		}
	}
}
