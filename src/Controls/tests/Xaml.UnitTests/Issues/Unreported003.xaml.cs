using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Unreported003 : ContentPage
{
	public Unreported003() => InitializeComponent();

	class Tests
	{
		[Test]
		public void AllowCtorArgsForValueTypes([Values] XamlInflator inflator)
		{
			var page = new Unreported003(inflator);
		}
	}
}