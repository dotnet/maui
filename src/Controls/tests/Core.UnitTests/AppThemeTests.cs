using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class AppThemeTests : BaseTestFixture
	{
		MockAppInfo mockAppInfo;
		Application app;

		public AppThemeTests()
		{
			AppInfo.SetCurrent(mockAppInfo = new MockAppInfo() { RequestedTheme = AppTheme.Light });
			Application.Current = app = new Application();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Application.Current = null;
			}

			base.Dispose(disposing);
		}

		[Fact]
		public void ThemeChangeUsingSetAppThemeColor()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};
			app.LoadPage(new ContentPage { Content = label });

			label.SetAppThemeColor(Label.TextColorProperty, Colors.Green, Colors.Red);
			Assert.Equal(Colors.Green, label.TextColor);

			SetAppTheme(AppTheme.Dark);

			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void ThemeChangeUsingSetAppTheme()
		{

			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};

			app.LoadPage(new ContentPage { Content = label });

			label.SetAppTheme(Label.TextColorProperty, Colors.Green, Colors.Red);
			Assert.Equal(Colors.Green, label.TextColor);

			SetAppTheme(AppTheme.Dark);

			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void ThemeChangeUsingSetBinding()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};
			app.LoadPage(new ContentPage { Content = label });

			label.SetBinding(Label.TextColorProperty, new AppThemeBinding { Light = Colors.Green, Dark = Colors.Red });
			Assert.Equal(Colors.Green, label.TextColor);

			SetAppTheme(AppTheme.Dark);

			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void ThemeChangeUsingUserAppTheme()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};
			app.LoadPage(new ContentPage { Content = label });

			label.SetAppThemeColor(Label.TextColorProperty, Colors.Green, Colors.Red);
			Assert.Equal(Colors.Green, label.TextColor);

			app.UserAppTheme = AppTheme.Dark;

			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void InitialThemeIsCorrect()
		{
			var changed = 0;
			var newTheme = AppTheme.Unspecified;

			app.RequestedThemeChanged += (_, e) =>
			{
				changed++;
				newTheme = e.RequestedTheme;
			};

			Assert.Equal(AppTheme.Light, app.RequestedTheme);
			Assert.Equal(AppTheme.Light, app.PlatformAppTheme);
			Assert.Equal(AppTheme.Unspecified, app.UserAppTheme);

			Assert.Equal(0, changed);
			Assert.Equal(AppTheme.Unspecified, newTheme);
		}

		[Fact]
		public void SettingSameUserThemeDoesNotFireEvent()
		{
			var changed = 0;
			var newTheme = AppTheme.Unspecified;

			app.RequestedThemeChanged += (_, e) =>
			{
				changed++;
				newTheme = e.RequestedTheme;
			};

			app.UserAppTheme = AppTheme.Light;

			Assert.Equal(AppTheme.Light, app.RequestedTheme);
			Assert.Equal(AppTheme.Light, app.PlatformAppTheme);
			Assert.Equal(AppTheme.Light, app.UserAppTheme);

			Assert.Equal(0, changed);
			Assert.Equal(AppTheme.Unspecified, newTheme);
		}

		[Fact]
		public void SettingDifferentUserThemeDoesNotFireEvent()
		{
			var changed = 0;
			var newTheme = AppTheme.Unspecified;

			app.RequestedThemeChanged += (_, e) =>
			{
				changed++;
				newTheme = e.RequestedTheme;
			};

			app.UserAppTheme = AppTheme.Dark;

			Assert.Equal(AppTheme.Dark, app.RequestedTheme);
			Assert.Equal(AppTheme.Light, app.PlatformAppTheme);
			Assert.Equal(AppTheme.Dark, app.UserAppTheme);

			Assert.Equal(1, changed);
			Assert.Equal(AppTheme.Dark, newTheme);
		}

		[Fact]
		public void UnsettingUserThemeReverts()
		{
			var changed = 0;
			var newTheme = AppTheme.Unspecified;

			app.RequestedThemeChanged += (_, e) =>
			{
				changed++;
				newTheme = e.RequestedTheme;
			};

			app.UserAppTheme = AppTheme.Dark;
			app.UserAppTheme = AppTheme.Unspecified;

			Assert.Equal(AppTheme.Light, app.RequestedTheme);
			Assert.Equal(AppTheme.Light, app.PlatformAppTheme);
			Assert.Equal(AppTheme.Unspecified, app.UserAppTheme);

			Assert.Equal(2, changed);
			Assert.Equal(AppTheme.Light, newTheme);
		}

		void SetAppTheme(AppTheme theme)
		{
			mockAppInfo.RequestedTheme = theme;
			((IApplication)app).ThemeChanged();
		}

		[Fact]
		//https://github.com/dotnet/maui/issues/3188
		public void ThemeBindingRemovedOnOneTimeBindablePropertyWhenPropertySet()
		{
			var shell = new Shell();
			shell.SetAppThemeColor(Shell.FlyoutBackgroundProperty, Colors.White, Colors.Black);
			shell.FlyoutBackgroundColor = Colors.Pink;
			SetAppTheme(AppTheme.Dark);
			Assert.Equal(Colors.Pink, shell.FlyoutBackgroundColor);
		}

		void validateRadioButtonColors(RadioButton button, SolidColorBrush desiredBrush)
		{
			var border = (Border)button.Children[0];
			var grid = (Grid)border.Content;
			var outerEllipse = (Shapes.Ellipse)grid.Children[0];
			var innerEllipse = (Shapes.Ellipse)grid.Children[1];

			Assert.Equal(desiredBrush, outerEllipse.Stroke);
			Assert.Equal(desiredBrush, innerEllipse.Fill);
		}

		[Fact]
		public void CorrectDefaultRadioButtonThemeColorsInLightAndDarkModes()
		{
			validateRadioButtonColors(
				new RadioButton() { ControlTemplate = RadioButton.DefaultTemplate },
				Brush.Black);
			SetAppTheme(AppTheme.Dark);
			validateRadioButtonColors(
				new RadioButton() { ControlTemplate = RadioButton.DefaultTemplate },
				Brush.White);
		}

		[Fact]
		public void NullApplicationCurrentFallsBackToEssentials()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};
			app.LoadPage(new ContentPage { Content = label });

			label.SetAppThemeColor(Label.TextColorProperty, Colors.Green, Colors.Red);

			Application.Current = null;

			Assert.Equal(Colors.Green, label.TextColor);

			SetAppTheme(AppTheme.Dark);

			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		//https://github.com/dotnet/maui/issues/17478
		public void BindingConversion()
		{
			var border = new Border();
			border.SetAppTheme(Border.StrokeProperty, Colors.Red, Colors.Black);
		}
	}
}