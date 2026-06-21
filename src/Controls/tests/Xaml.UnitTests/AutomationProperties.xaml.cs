using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;
partial class AutomationProperties : ContentPage
{
	public AutomationProperties() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		public Tests() => Application.Current = new MockApplication();
		public void Dispose() => Application.Current = null;

		[Theory]
		[XamlInflatorData]
		internal void AutomationPropertiesName(XamlInflator inflator)
		{
			var layout = new AutomationProperties(inflator);

			Assert.Equal("Name", (string)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.NameProperty));
		}

		[Theory]
		[XamlInflatorData]
		internal void AutomationPropertiesHelpText(XamlInflator inflator)
		{
			var layout = new AutomationProperties(inflator);

			Assert.Equal("Sets your name", (string)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.HelpTextProperty));
		}

		[Theory]
		[XamlInflatorData]
		internal void AutomationPropertiesIsInAccessibleTree(XamlInflator inflator)
		{
			var layout = new AutomationProperties(inflator);
			Application.Current.LoadPage(layout);

			Assert.Equal(true, (bool?)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.IsInAccessibleTreeProperty));
		}

		[Theory]
		[XamlInflatorData]
		internal void AutomationPropertiesLabeledBy(XamlInflator inflator)
		{
			var layout = new AutomationProperties(inflator);
			Application.Current.LoadPage(layout);

			Assert.Equal(layout.label, (Element)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.LabeledByProperty));
		}
	}
}
