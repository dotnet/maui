using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh3847 : ContentPage
{
	public Gh3847() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void RelativeSourceSelfBinding(XamlInflator inflator)
		{
			var view = new Gh3847(inflator);
			var label = view.FindByName<Label>("SelfBindingLabel");
			Assert.Equal(label.StyleId, label.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void RelativeSourceAncestorLevelBinding(XamlInflator inflator)
		{
			var view = new Gh3847(inflator);
			var stack0 = view.FindByName<StackLayout>("Stack0");
			var stack1 = view.FindByName<StackLayout>("Stack1");
			var level1Label = view.FindByName<Label>("AncestorLevel1Label");
			var level2Label = view.FindByName<Label>("AncestorLevel2Label");
			var level3Label = view.FindByName<Label>("AncestorLevel3Label");
			var ancestorBindingContextLabel = view.FindByName<Label>("AncestorBindingContextLabel");

			Assert.Equal(stack1.StyleId, level1Label.Text);
			Assert.Equal(stack0.StyleId, level2Label.Text);
			Assert.Equal("Foo", ancestorBindingContextLabel.Text);
			Assert.Null(level3Label.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void RelativeSourceTemplatedParentBinding(XamlInflator inflator)
		{
			var view = new Gh3847(inflator);
			var cv = view.FindByName<ContentView>("contentView");
			var label = (cv as IVisualTreeElement).GetVisualChildren()[0] as Label;
			Assert.Equal(cv.StyleId, label.Text);
		}
	}
}

public class Gh3847ViewModel
{
	public string Foo => "Foo";
}
