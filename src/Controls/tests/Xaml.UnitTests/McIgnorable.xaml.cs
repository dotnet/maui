using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class McIgnorable : ContentPage
{
	public McIgnorable() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void DoesNotThrow([Values] XamlInflator inflator)
		{
			var layout = new McIgnorable(inflator);
		}
	}
}