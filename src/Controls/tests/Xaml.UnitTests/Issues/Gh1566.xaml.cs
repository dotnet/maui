using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh1566
{
	public Gh1566() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void ObsoletePropsDoNotThrow(XamlInflator inflator)
		{
			var layout = new Gh1566(inflator);
			Assert.Equal(Colors.Red, layout.frame.BorderColor);
		}
	}
}
