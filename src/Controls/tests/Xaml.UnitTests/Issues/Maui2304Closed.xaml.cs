using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui2304Closed
{
	public Maui2304Closed() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void XamlCDoesntFail(XamlInflator inflator)
		{
			var layout = new Maui2304Closed(inflator);
			Assert.Equal(typeof(OnPlatform<string>), typeof(Maui2304Closed).BaseType);
		}
	}
}
