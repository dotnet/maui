using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XNull : ContentPage
{
	public XNull() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void SupportsXNull([Values] XamlInflator inflator)
		{
			var layout = new XNull(inflator);
			Assert.True(layout.Resources.ContainsKey("null"));
			Assert.Null(layout.Resources["null"]);
		}
	}
}