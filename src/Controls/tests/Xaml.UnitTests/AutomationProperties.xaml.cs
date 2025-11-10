using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

partial class AutomationProperties : ContentPage
{
	public AutomationProperties() => InitializeComponent();


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

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
