using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz54717 : ContentPage
{
	public Bz54717()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[TearDown] public void TearDown() => Application.Current = null;

		[Test]
		public void FooBz54717([Values] XamlInflator inflator)
		{
			Application.Current = new MockApplication
			{
				Resources = new ResourceDictionary {
					{"Color1", Colors.Red},
					{"Color2", Colors.Blue},
				}
			};
			var layout = new Bz54717(inflator);
			Assert.That(layout.Resources.Count, Is.EqualTo(1));
			var array = layout.Resources["SomeColors"] as Color[];
			Assert.That(array[0], Is.EqualTo(Colors.Red));
			Assert.That(array[1], Is.EqualTo(Colors.Blue));
		}
	}
}
