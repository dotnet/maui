using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class SetStyleIdFromXName : ContentPage
{
	public SetStyleIdFromXName() => InitializeComponent();


	[TestFixture]
	public class Tests
	{
		[Test]
		public void SetStyleId([Values]XamlInflator inflator)
		{
			var layout = new SetStyleIdFromXName(inflator);
			Assert.That(layout.label0.StyleId, Is.EqualTo("label0"));
			Assert.That(layout.label1.StyleId, Is.EqualTo("foo"));
			Assert.That(layout.label2.StyleId, Is.EqualTo("bar"));
		}
	}
}
