using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Accessibility : ContentPage
	{
		public Accessibility()
		{
			InitializeComponent();
		}

		public Accessibility(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
				Application.Current = new MockApplication();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
				Application.Current = null;
			}

			[TestCase(false)]
			[TestCase(true)]
			public void AccessibilityName(bool useCompiledXaml)
			{
				var layout = new Accessibility(useCompiledXaml);

				Assert.AreEqual("Name", (string)layout.entry.GetValue(Xamarin.Forms.Accessibility.NameProperty));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void AccessibilityHint(bool useCompiledXaml)
			{
				var layout = new Accessibility(useCompiledXaml);

				Assert.AreEqual("Sets your name", (string)layout.entry.GetValue(Xamarin.Forms.Accessibility.HintProperty));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void AccessibilityIsInAccessibleTree(bool useCompiledXaml)
			{
				var layout = new Accessibility(useCompiledXaml);
				Application.Current.MainPage = layout;

				Assert.AreEqual(true, (bool)layout.entry.GetValue(Xamarin.Forms.Accessibility.IsInAccessibleTreeProperty));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void AccessibilityLabeledBy(bool useCompiledXaml)
			{
				var layout = new Accessibility(useCompiledXaml);
				Application.Current.MainPage = layout;

				Assert.AreEqual(layout.label, (Element)layout.entry.GetValue(Xamarin.Forms.Accessibility.LabeledByProperty));
			}
		}
	}
}
