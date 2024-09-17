using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class Issue3090 : ContentPage
{
	public Issue3090() => InitializeComponent();

	[TestFixture]
	public class Tests
	{
		[Test]
		public void NewDoesNotThrow([Values] XamlInflator inflator)
		{
			var p = new Issue3090(inflator);
		}
	}
}