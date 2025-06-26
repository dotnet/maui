using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
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
		public class Tests
		{
			// Constructor
			public void Setup()
			{
				Application.Current = new MockApplication();
			}
			public void TearDown()
			{
				Application.Current = null;
			}

			[Theory]
			[InlineData(true)]
			public void AutomationPropertiesName(bool useCompiledXaml)
			{
				var layout = new AutomationProperties(useCompiledXaml);

				Assert.Equal("Name", (string)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.NameProperty));
			}

			[Theory]
			[InlineData(true)]
			public void AutomationPropertiesHelpText(bool useCompiledXaml)
			{
				var layout = new AutomationProperties(useCompiledXaml);

				Assert.Equal("Sets your name", (string)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.HelpTextProperty));
			}

			[Theory]
			[InlineData(true)]
			public void AutomationPropertiesIsInAccessibleTree(bool useCompiledXaml)
			{
				var layout = new AutomationProperties(useCompiledXaml);
				Application.Current.LoadPage(layout);

				Assert.Equal(true, (bool)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.IsInAccessibleTreeProperty));
			}

			[Theory]
			[InlineData(true)]
			public void AutomationPropertiesLabeledBy(bool useCompiledXaml)
			{
				var layout = new AutomationProperties(useCompiledXaml);
				Application.Current.LoadPage(layout);

				Assert.Equal(layout.label, (Element)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.LabeledByProperty));
			}
		}
	}
}
