using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class MergedResourceDictionaries : ContentPage
{
	public MergedResourceDictionaries() => InitializeComponent();

	class Tests
	{
		[Test]
		public void MergedResourcesAreFound([Values] XamlInflator inflator)
		{
			MockCompiler.Compile(typeof(MergedResourceDictionaries));
			var layout = new MergedResourceDictionaries(inflator);
			Assert.That(layout.label0.Text, Is.EqualTo("Foo"));
			Assert.That(layout.label0.TextColor, Is.EqualTo(Colors.Pink));
			Assert.That(layout.label0.BackgroundColor, Is.EqualTo(Color.FromArgb("#111")));
		}
	}
}