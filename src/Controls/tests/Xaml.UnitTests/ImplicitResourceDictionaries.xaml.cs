using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class ImplicitResourceDictionaries : ContentPage
{
	public ImplicitResourceDictionaries() => InitializeComponent();

	[TestFixture]
	public class Tests
	{
		[Test]
		public void ImplicitRDonContentViews([Values]XamlInflator inflator)
		{
			var layout = new ImplicitResourceDictionaries(inflator);
			Assert.That(layout.label.TextColor, Is.EqualTo(Colors.Purple));
		}
	}
}
