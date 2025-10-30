using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue1415 : ContentPage
{
	public Issue1415() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void NestedMarkupExtension([Values] XamlInflator inflator)
		{
			var page = new Issue1415(inflator);
			var label = page.FindByName<Label>("label");
			Assert.NotNull(label);
			label.BindingContext = "foo";
			Assert.AreEqual("oof", label.Text);
		}
	}
}