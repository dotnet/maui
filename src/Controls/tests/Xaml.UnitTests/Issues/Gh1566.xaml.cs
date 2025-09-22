using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh1566
{
	public Gh1566() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void ObsoletePropsDoNotThrow([Values] XamlInflator inflator)
		{
			var layout = new Gh1566(inflator);
			Assert.That(layout.frame.BorderColor, Is.EqualTo(Colors.Red));
		}
	}
}
