using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Unreported003 : ContentPage
{
	public Unreported003() => InitializeComponent();

	public class Tests
	{
		[Theory]
		[Values]
		public void AllowCtorArgsForValueTypes(XamlInflator inflator)
		{
			var page = new Unreported003(inflator);
		}
	}
}