using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class ImplicitResourceDictionaries : ContentPage
{
	public ImplicitResourceDictionaries() => InitializeComponent();

	[TestFixture]
	public class Tests
	{
		[Test]
		internal void ImplicitRDonContentViews([Values] XamlInflator inflator)
		{
			var layout = new ImplicitResourceDictionaries(inflator);
			Assert.That(layout.label.TextColor, Is.EqualTo(Colors.Purple));
		}
	}
}
