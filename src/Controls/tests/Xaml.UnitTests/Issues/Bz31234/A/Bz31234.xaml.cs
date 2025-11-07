using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.A;

public partial class Bz31234 : ContentPage
{
	public Bz31234() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void ShouldPass(XamlInflator inflator)
		{
			new Bz31234(inflator);
			// TODO: XUnit has no // TODO: XUnit has no Assert.Pass() - test passes if no exception is thrown - test passes if no exception is thrown
		}
	}
}