using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class ImplicitResourceDictionaries : ContentPage
{
	public ImplicitResourceDictionaries() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void ImplicitRDonContentViews(XamlInflator inflator)
		{
			var layout = new ImplicitResourceDictionaries(inflator);
			Assert.Equal(Colors.Purple, layout.label.TextColor);
		}
	}
}
