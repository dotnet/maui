using Microsoft.Maui.Graphics;
using Xunit;

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


	public class Tests
	{

		[Theory]
		[Values]
		public void DupeKeyRd(XamlInflator inflator)
		{
			var layout = new Gh2483(inflator);
			// TODO: XUnit has no // TODO: XUnit has no Assert.Pass() - test passes if no exception is thrown - test passes if no exception is thrown
		}
	}
}
