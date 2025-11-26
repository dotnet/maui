using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class ButtonCornerRadius : ContentPage
{
	public ButtonCornerRadius() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => Application.Current = new MockApplication();
		[TearDown] public void TearDown() => Application.Current = null;

		[Test]
		public void EscapedStringsAreTreatedAsLiterals([Values] XamlInflator inflator)
		{
			var layout = new ButtonCornerRadius(inflator);
			Assert.AreEqual(0, layout.Button0.CornerRadius);
		}
	}
}