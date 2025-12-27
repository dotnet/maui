using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33203AppTheme : ContentPage
{
	public Maui33203AppTheme() => InitializeComponent();

	public Maui33203AppTheme(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[Collection("Issue")]
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
		}

		[Theory]
		[XamlInflatorData]
		internal void DerivedStyleOverridesBaseAppThemeBinding(XamlInflator inflator)
		{
			// This test reproduces the additional issue from #33203 comment where
			// when a base style uses AppThemeBinding for a property, the derived style's
			// override of that property is not applied correctly
			
			Application.Current.UserAppTheme = AppTheme.Light;
			var page = new Maui33203AppTheme(inflator);
			Application.Current.MainPage = page;

			// CustomLabel should have Blue background from the LabelBaseAppTheme keyed style's AppThemeBinding
			Assert.Equal(Colors.Blue, page.baseLabel.BackgroundColor);
			Assert.Equal(20d, page.baseLabel.FontSize);

			// DerivedLabel should have Red background from DerivedLabelStyleAppTheme keyed style
			// NOT Blue from LabelBaseAppTheme's AppThemeBinding - the derived style's static value should override
			Assert.Equal(Colors.Red, page.derivedLabel.BackgroundColor);
			// FontSize should still be inherited from base style
			Assert.Equal(20d, page.derivedLabel.FontSize);
		}

		[Theory]
		[XamlInflatorData]
		internal void DerivedStyleOverridesBaseAppThemeBindingInDarkMode(XamlInflator inflator)
		{
			// Test the same scenario in dark mode to ensure AppThemeBinding doesn't interfere
			
			Application.Current.UserAppTheme = AppTheme.Dark;
			var page = new Maui33203AppTheme(inflator);
			Application.Current.MainPage = page;

			// CustomLabel should have Blue background (dark mode value from AppThemeBinding)
			Assert.Equal(Colors.Blue, page.baseLabel.BackgroundColor);

			// DerivedLabel should STILL have Red background from DerivedLabelStyleAppTheme
			// regardless of dark/light mode - static value should override AppThemeBinding
			Assert.Equal(Colors.Red, page.derivedLabel.BackgroundColor);
		}
	}
}

// Custom Label classes for testing AppThemeBinding override scenarios
public class CustomLabel33203AppTheme : Label
{
}

public class DerivedLabel33203AppTheme : CustomLabel33203AppTheme
{
}
