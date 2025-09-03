using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue3106 : ContentPage
{
	public Issue3106() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void NewDoesNotThrow([Values] XamlInflator inflator)
		{
			var p = new Issue3106(inflator);
		}
	}
}

