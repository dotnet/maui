using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class DO817710 : ContentPage
{
	public DO817710() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void EmptyResourcesElement([Values] XamlInflator inflator)
		{
			Assert.DoesNotThrow(() => new DO817710(inflator));
		}
	}
}
