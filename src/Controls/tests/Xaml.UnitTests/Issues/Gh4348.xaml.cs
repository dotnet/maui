using System.Collections.ObjectModel;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh4348VM : ObservableCollection<string>
{
	public Gh4348VM()
	{
		Add("foo");
		Add("bar");
	}
}

public partial class Gh4348 : ContentPage
{
	public Gh4348() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void GenericBaseClassResolution([Values] XamlInflator inflator)
		{
			var layout = new Gh4348(inflator) { BindingContext = new Gh4348VM() };
			Assert.That(layout.labelCount.Text, Is.EqualTo("2"));
		}
	}
}
