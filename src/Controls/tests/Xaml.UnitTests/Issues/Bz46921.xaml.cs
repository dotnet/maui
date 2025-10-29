using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz46921 : ContentPage
{
	public Bz46921()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[Test]
		public void MultipleWaysToCreateAThicknessResource([Values] XamlInflator inflator)
		{
			var page = new Bz46921(inflator);
			foreach (var resname in new string[] { "thickness0", "thickness1", "thickness2", "thickness3", })
			{
				var resource = page.Resources[resname];
				Assert.That(resource, Is.TypeOf<Thickness>());
				var thickness = (Thickness)resource;
				Assert.AreEqual(new Thickness(4, 20, 4, 20), thickness);

			}
		}
	}
}