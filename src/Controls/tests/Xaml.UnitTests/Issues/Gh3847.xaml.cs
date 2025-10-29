using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh3847 : ContentPage
	{
		public Gh3847()
		{
			InitializeComponent();
		}

		public Gh3847(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(false), InlineData(true)]
			public void RelativeSourceSelfBinding(bool useCompiledXaml)
			{
				var view = new Gh3847(useCompiledXaml);
				var label = view.FindByName<Label>("SelfBindingLabel");
				Assert.Equal(label.Text, label.StyleId);
			}

			[Theory]

			[InlineData(false), InlineData(true)]
			public void RelativeSourceAncestorLevelBinding(bool useCompiledXaml)
			{
				var view = new Gh3847(useCompiledXaml);
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

			[InlineData(false), InlineData(true)]
			public void RelativeSourceTemplatedParentBinding(bool useCompiledXaml)
			{
				var view = new Gh3847(useCompiledXaml);
				var cv = view.FindByName<ContentView>("contentView");
				var label = cv.Children[0] as Label;
				Assert.Equal(label.Text, cv.StyleId);
			}
		}
	}

	public class Gh3847ViewModel
	{
		public string Foo => "Foo";
	}
}
