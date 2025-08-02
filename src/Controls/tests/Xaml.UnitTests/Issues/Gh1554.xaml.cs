using System.Linq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh1554
{
	public Gh1554() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void NestedRDAreOnlyProcessedOnce([Values] XamlInflator inflator)
		{
			var layout = new Gh1554(inflator);
			Assert.That(layout.Resources.MergedDictionaries.First().First().Key, Is.EqualTo("label0"));
		}
	}
}
