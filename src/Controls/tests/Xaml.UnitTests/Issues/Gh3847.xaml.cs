using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh3847 : ContentPage
{
	public Gh3847() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void RelativeSourceSelfBinding(XamlInflator inflator)
		{
			var view = new Gh3847(inflator);
			var label = view.FindByName<Label>("SelfBindingLabel");
			Assert.Equal(label.Text, label.StyleId);
		}

		[Theory]
		[Values]
		public void RelativeSourceAncestorLevelBinding(XamlInflator inflator)
		{
			var view = new Gh3847(inflator);
			var stack0 = view.FindByName<StackLayout>("Stack0");
			var stack1 = view.FindByName<StackLayout>("Stack1");
			var level1Label = view.FindByName<Label>("AncestorLevel1Label");
			var level2Label = view.FindByName<Label>("AncestorLevel2Label");
			var level3Label = view.FindByName<Label>("AncestorLevel3Label");
			var ancestorBindingContextLabel = view.FindByName<Label>("AncestorBindingContextLabel");

			Assert.Equal(level1Label.Text, stack1.StyleId);
			Assert.Equal(level2Label.Text, stack0.StyleId);
			Assert.Equal("Foo", ancestorBindingContextLabel.Text);
			Assert.Null(level3Label.Text);
		}

		[Theory]
		[Values]
		public void RelativeSourceTemplatedParentBinding(XamlInflator inflator)
		{
			var view = new Gh3847(inflator);
			var cv = view.FindByName<ContentView>("contentView");
			var label = (cv as IVisualTreeElement).GetVisualChildren()[0] as Label;
			Assert.Equal(label.Text, cv.StyleId);
		}
	}
}

public class Gh3847ViewModel
{
	public string Foo => "Foo";
}
