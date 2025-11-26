using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh2483Rd : ResourceDictionary
{
}

public class Gh2483Custom : ResourceDictionary
{
	public Gh2483Custom() => Add("foo", Colors.Orange);
}

public partial class Gh2483 : ContentPage
{
	public Gh2483() => InitializeComponent();

	[TestFixture]
	class Tests
	{

		[Test]
		public void DupeKeyRd([Values] XamlInflator inflator)
		{
			var layout = new Gh2483(inflator);
			Assert.Pass();
		}
	}
}
