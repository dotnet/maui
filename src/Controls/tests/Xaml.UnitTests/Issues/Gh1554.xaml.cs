using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh1554
{
	public Gh1554() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void NestedRDAreOnlyProcessedOnce(XamlInflator inflator)
		{
			var layout = new Gh1554(inflator);
			Assert.Equal("label0", layout.Resources.MergedDictionaries.First().First().Key);
		}
	}
}
