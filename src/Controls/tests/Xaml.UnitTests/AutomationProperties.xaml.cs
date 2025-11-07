using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

partial class AutomationProperties : ContentPage
{
	public AutomationProperties() => InitializeComponent();


	public class Tests
	{
		// TODO: Convert to IDisposable or constructor/IAsyncLifetime for Setup() => Application.Current = new MockApplication();

		[Theory]
		[Values]
		internal void AutomationPropertiesName(XamlInflator inflator)
		{
			var layout = new AutomationProperties(inflator);

			Assert.Equal("Name", (string)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.NameProperty));
		}

		[Theory]
		[Values]
		internal void AutomationPropertiesHelpText(XamlInflator inflator)
		{
			var layout = new AutomationProperties(inflator);

			Assert.Equal("Sets your name", (string)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.HelpTextProperty));
		}

		[Theory]
		[Values]
		internal void AutomationPropertiesIsInAccessibleTree(XamlInflator inflator)
		{
			var layout = new AutomationProperties(inflator);
			Application.Current.LoadPage(layout);

			Assert.Equal(true, (bool?)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.IsInAccessibleTreeProperty));
		}

		[Theory]
		[Values]
		internal void AutomationPropertiesLabeledBy(XamlInflator inflator)
		{
			var layout = new AutomationProperties(inflator);
			Application.Current.LoadPage(layout);

			Assert.Equal(layout.label, (Element)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.LabeledByProperty));
		}
	}
}
