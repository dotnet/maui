using System;
using System.Collections.Generic;
using NUnit.Framework;

using System.Maui;
using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
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

		[TestFixture]
		class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(false), TestCase(true)]
			public void RelativeSourceSelfBinding(bool useCompiledXaml)
			{
				var view = new Gh3847(useCompiledXaml);
				var label = view.FindByName<Label>("SelfBindingLabel");
				Assert.AreEqual(label.Text, label.StyleId);
			}

			[TestCase(false), TestCase(true)]
			public void RelativeSourceAncestorLevelBinding(bool useCompiledXaml)
			{
				var view = new Gh3847(useCompiledXaml);
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

			[TestCase(false), TestCase(true)]
			public void RelativeSourceTemplatedParentBinding(bool useCompiledXaml)
			{
				var view = new Gh3847(useCompiledXaml);
				var cv = view.FindByName<ContentView>("contentView");
				var label = cv.Children[0] as Label;
				Assert.AreEqual(label.Text, cv.StyleId);
			}
		}
	}

	public class Gh3847ViewModel
	{
		public string Foo => "Foo";
	}
}
