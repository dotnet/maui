using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh3847 : ContentPage
{
	public Gh3847() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void RelativeSourceSelfBinding([Values] XamlInflator inflator)
		{
			var view = new Gh3847(inflator);
			var label = view.FindByName<Label>("SelfBindingLabel");
			Assert.AreEqual(label.Text, label.StyleId);
		}

		[Test]
		public void RelativeSourceAncestorLevelBinding([Values] XamlInflator inflator)
		{
			var view = new Gh3847(inflator);
			var stack0 = view.FindByName<StackLayout>("Stack0");
			var stack1 = view.FindByName<StackLayout>("Stack1");
			var level1Label = view.FindByName<Label>("AncestorLevel1Label");
			var level2Label = view.FindByName<Label>("AncestorLevel2Label");
			var level3Label = view.FindByName<Label>("AncestorLevel3Label");
			var ancestorBindingContextLabel = view.FindByName<Label>("AncestorBindingContextLabel");

			Assert.AreEqual(level1Label.Text, stack1.StyleId);
			Assert.AreEqual(level2Label.Text, stack0.StyleId);
			Assert.AreEqual(ancestorBindingContextLabel.Text, "Foo");
			Assert.IsNull(level3Label.Text);
		}

		[Test]
		public void RelativeSourceTemplatedParentBinding([Values] XamlInflator inflator)
		{
			var view = new Gh3847(inflator);
			var cv = view.FindByName<ContentView>("contentView");
			var label = (cv as IVisualTreeElement).GetVisualChildren()[0] as Label;
			Assert.AreEqual(label.Text, cv.StyleId);
		}
	}
}

public class Gh3847ViewModel
{
	public string Foo => "Foo";
}
