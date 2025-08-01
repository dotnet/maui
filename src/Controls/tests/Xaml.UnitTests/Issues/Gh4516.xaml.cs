using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh4516VM
{
	public Uri[] Images { get; } = Array.Empty<Uri>();
}

public partial class Gh4516 : ContentPage
{
	public Gh4516() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void BindingToEmptyCollection([Values] XamlInflator inflator)
		{
			Gh4516 layout = null;
			Assert.DoesNotThrow(() => layout = new Gh4516(inflator) { BindingContext = new Gh4516VM() });
			Assert.That((layout.image.Source as FileImageSource).File, Is.EqualTo("foo.jpg"));
		}
	}
}