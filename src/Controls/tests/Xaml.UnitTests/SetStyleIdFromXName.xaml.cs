using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class SetStyleIdFromXName : ContentPage
{
	public SetStyleIdFromXName() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void SetStyleId(XamlInflator inflator)
		{
			var layout = new SetStyleIdFromXName(inflator);
			Assert.Equal("label0", layout.label0.StyleId);
			Assert.Equal("foo", layout.label1.StyleId);
			Assert.Equal("bar", layout.label2.StyleId);
		}
	}
}
