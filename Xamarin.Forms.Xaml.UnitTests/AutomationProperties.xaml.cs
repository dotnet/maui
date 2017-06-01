using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class AutomationProperties : ContentPage
	{
		public AutomationProperties()
		{
			InitializeComponent();
		}

		public AutomationProperties(bool useCompiledXaml)
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
			public void AutomationPropertiesName(bool useCompiledXaml)
			{
				var layout = new AutomationProperties(useCompiledXaml);

				Assert.AreEqual("Name", (string)layout.entry.GetValue(Xamarin.Forms.AutomationProperties.NameProperty));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void AutomationPropertiesHelpText(bool useCompiledXaml)
			{
				var layout = new AutomationProperties(useCompiledXaml);

				Assert.AreEqual("Sets your name", (string)layout.entry.GetValue(Xamarin.Forms.AutomationProperties.HelpTextProperty));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void AutomationPropertiesIsInAccessibleTree(bool useCompiledXaml)
			{
				var layout = new AutomationProperties(useCompiledXaml);
				Application.Current.MainPage = layout;

				Assert.AreEqual(true, (bool)layout.entry.GetValue(Xamarin.Forms.AutomationProperties.IsInAccessibleTreeProperty));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void AutomationPropertiesLabeledBy(bool useCompiledXaml)
			{
				var layout = new AutomationProperties(useCompiledXaml);
				Application.Current.MainPage = layout;

				Assert.AreEqual(layout.label, (Element)layout.entry.GetValue(Xamarin.Forms.AutomationProperties.LabeledByProperty));
			}
		}
	}
}
