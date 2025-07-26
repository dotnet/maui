using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class McIgnorable : ContentPage
{
	public McIgnorable() => InitializeComponent();

	[TestFixture]
	public class Tests
	{
		[Test]
		public void DoesNotThrow([Values]XamlInflator inflator)
		{
			var layout = new McIgnorable(inflator);
		}
	}
}