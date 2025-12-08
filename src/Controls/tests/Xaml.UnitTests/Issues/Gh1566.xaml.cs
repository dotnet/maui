using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh1566
{
	public Gh1566() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ObsoletePropsDoNotThrow(XamlInflator inflator)
		{
			var layout = new Gh1566(inflator);
			Assert.Equal(Colors.Red, layout.frame.BorderColor);
		}
	}
}
