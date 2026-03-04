using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh1554
{
	public Gh1554() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void NestedRDAreOnlyProcessedOnce(XamlInflator inflator)
		{
			var layout = new Gh1554(inflator);
			Assert.Equal("label0", layout.Resources.MergedDictionaries.First().First().Key);
		}
	}
}
