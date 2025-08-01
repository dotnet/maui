using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh5240 : ContentPage
{
	public Gh5240() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void FailOnUnresolvedDataType([Values] XamlInflator inflator)
		{
			new Gh5240(inflator);
		}
	}
}
