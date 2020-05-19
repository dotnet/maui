using System;
using NUnit.Framework;
using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
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

				Assert.AreEqual("Name", (string)layout.entry.GetValue(System.Maui.AutomationProperties.NameProperty));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void AutomationPropertiesHelpText(bool useCompiledXaml)
			{
				var layout = new AutomationProperties(useCompiledXaml);

				Assert.AreEqual("Sets your name", (string)layout.entry.GetValue(System.Maui.AutomationProperties.HelpTextProperty));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void AutomationPropertiesIsInAccessibleTree(bool useCompiledXaml)
			{
				var layout = new AutomationProperties(useCompiledXaml);
				Application.Current.MainPage = layout;

				Assert.AreEqual(true, (bool)layout.entry.GetValue(System.Maui.AutomationProperties.IsInAccessibleTreeProperty));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void AutomationPropertiesLabeledBy(bool useCompiledXaml)
			{
				var layout = new AutomationProperties(useCompiledXaml);
				Application.Current.MainPage = layout;

				Assert.AreEqual(layout.label, (Element)layout.entry.GetValue(System.Maui.AutomationProperties.LabeledByProperty));
			}
		}
	}
}
