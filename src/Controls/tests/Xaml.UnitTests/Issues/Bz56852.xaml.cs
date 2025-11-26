using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz56852
{
	public Bz56852()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[TearDown] public void TearDown() => Application.Current = null;

		[Test]
		public void DynamicResourceApplyingOrder([Values] XamlInflator inflator)
		{
			var layout = new Bz56852(inflator);
			Assert.That(layout.label.FontSize, Is.EqualTo(50));
		}
	}
}
