using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16569
{
	public Maui16569() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		static readonly Color Primary = Color.FromArgb("#512BD4");
		static readonly Color Gray200 = Color.FromArgb("#C8C8C8");
		static readonly Color Gray600 = Color.FromArgb("#404040");
		static readonly Color Gray950 = Color.FromArgb("#141414");

		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void KeyedStyleDisabledStateDoesNotDependOnAttributeOrder(XamlInflator inflator)
		{
			Application.Current.UserAppTheme = AppTheme.Dark;
			var page = new Maui16569(inflator);
			Application.Current.MainPage = page;
			var buttons = new[] { page.isEnabledBeforeStyle, page.styleBeforeIsEnabled, page.implicitStyle };

			foreach (var button in buttons)
			{
				Assert.Equal(Gray600, button.BackgroundColor);
				Assert.Equal(Gray200, button.TextColor);
			}

			Application.Current.UserAppTheme = AppTheme.Light;
			foreach (var button in buttons)
			{
				Assert.Equal(Gray200, button.BackgroundColor);
				Assert.Equal(Gray950, button.TextColor);
			}

			page.isEnabledBeforeStyle.IsEnabled = true;
			Assert.Equal(Primary, page.isEnabledBeforeStyle.BackgroundColor);
			Assert.Equal(Colors.White, page.isEnabledBeforeStyle.TextColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void KeyedStyleDisabledStateAppliesWhenCommandCannotExecute(XamlInflator inflator)
		{
			Application.Current.UserAppTheme = AppTheme.Dark;
			var page = new Maui16569(inflator);
			Application.Current.MainPage = page;

			var canExecute = false;
			var command = new Command(() => { }, () => canExecute);
			var button = new Button { Command = command };
			button.Style = (Style)page.Resources["DefaultButtonStyle"];
			((VerticalStackLayout)page.Content).Add(button);

			Assert.False(button.IsEnabled);
			Assert.Equal(Gray600, button.BackgroundColor);
			Assert.Equal(Gray200, button.TextColor);

			canExecute = true;
			command.ChangeCanExecute();
			Assert.Equal(Colors.White, button.BackgroundColor);
			Assert.Equal(Primary, button.TextColor);
		}
	}
}
