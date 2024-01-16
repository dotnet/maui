using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
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

		[Fact(Skip = "The current implementation actively choses to have different values.")]
		public void UnattachedVisualElementBindingIsConsistent()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};

			label.SetAppThemeColor(Label.TextColorProperty, Colors.Green, Colors.Red);
			Assert.Equal(Colors.Green, label.TextColor);

			EmulatePlatformThemeChange(AppTheme.Dark);

			label.SetAppThemeColor(Label.TextColorProperty, Colors.Green, Colors.Red);
			Assert.Equal(Colors.Green, label.TextColor);
		}

		[Fact]
		public void ThemeChangeUsingSetAppThemeColor()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};

			label.SetAppThemeColor(Label.TextColorProperty, Colors.Green, Colors.Red);
			Assert.Equal(Colors.Green, label.TextColor);

			app.LoadPage(new ContentPage { Content = label });

			Assert.Equal(Colors.Green, label.TextColor);

			EmulatePlatformThemeChange(AppTheme.Dark);

			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void ThemeChangeUsingSetAppThemeColorNonVisualElement()
		{
			var element = new NonVisualElement
			{
				Text = "Green on Light, Red on Dark"
			};

			element.SetAppThemeColor(NonVisualElement.ColorProperty, Colors.Green, Colors.Red);
			Assert.Equal(Colors.Green, element.Color);

			EmulatePlatformThemeChange(AppTheme.Dark);

			Assert.Equal(Colors.Red, element.Color);
		}

		[Fact]
		public void ThemeChangeUsingSetAppTheme()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};

			label.SetAppTheme(Label.TextColorProperty, Colors.Green, Colors.Red);
			Assert.Equal(Colors.Green, label.TextColor);

			app.LoadPage(new ContentPage { Content = label });

			Assert.Equal(Colors.Green, label.TextColor);

			EmulatePlatformThemeChange(AppTheme.Dark);

			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void ThemeChangeUsingSetAppThemeNonVisualElement()
		{
			var element = new NonVisualElement
			{
				Text = "Green on Light, Red on Dark"
			};

			element.SetAppTheme(NonVisualElement.ColorProperty, Colors.Green, Colors.Red);
			Assert.Equal(Colors.Green, element.Color);

			EmulatePlatformThemeChange(AppTheme.Dark);

			Assert.Equal(Colors.Red, element.Color);
		}

		[Fact]
		public void ThemeChangeUsingSetBinding()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};

			label.SetBinding(Label.TextColorProperty, new AppThemeBinding { Light = Colors.Green, Dark = Colors.Red });
			Assert.Equal(Colors.Green, label.TextColor);

			app.LoadPage(new ContentPage { Content = label });

			Assert.Equal(Colors.Green, label.TextColor);

			EmulatePlatformThemeChange(AppTheme.Dark);

			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void ThemeChangeUsingSetBindingNonVisualElement()
		{
			var element = new NonVisualElement
			{
				Text = "Green on Light, Red on Dark"
			};

			element.SetBinding(NonVisualElement.ColorProperty, new AppThemeBinding { Light = Colors.Green, Dark = Colors.Red });
			Assert.Equal(Colors.Green, element.Color);

			EmulatePlatformThemeChange(AppTheme.Dark);

			Assert.Equal(Colors.Red, element.Color);
		}

		[Fact]
		public void ThemeChangeUsingUserAppTheme()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};

			label.SetAppThemeColor(Label.TextColorProperty, Colors.Green, Colors.Red);
			Assert.Equal(Colors.Green, label.TextColor);

			app.LoadPage(new ContentPage { Content = label });

			Assert.Equal(Colors.Green, label.TextColor);

			EmulatePlatformThemeChange(AppTheme.Dark);

			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void ThemeChangeUsingUserAppThemeNonVisualElement()
		{
			var element = new NonVisualElement
			{
				Text = "Green on Light, Red on Dark"
			};

			element.SetAppThemeColor(NonVisualElement.ColorProperty, Colors.Green, Colors.Red);
			Assert.Equal(Colors.Green, element.Color);

			app.UserAppTheme = AppTheme.Dark;

			Assert.Equal(Colors.Red, element.Color);
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

		[Fact]
		//https://github.com/dotnet/maui/issues/3188
		public void ThemeBindingRemovedOnOneTimeBindablePropertyWhenPropertySet()
		{
			var shell = new Shell();
			shell.SetAppThemeColor(Shell.FlyoutBackgroundProperty, Colors.White, Colors.Black);
			shell.FlyoutBackgroundColor = Colors.Pink;
			EmulatePlatformThemeChange(AppTheme.Dark);
			Assert.Equal(Colors.Pink, shell.FlyoutBackgroundColor);
		}

		[Fact]
		public void CorrectDefaultRadioButtonThemeColorsInLightAndDarkModes()
		{
			validateRadioButtonColors(
				new RadioButton() { ControlTemplate = RadioButton.DefaultTemplate },
				Brush.Black);
			EmulatePlatformThemeChange(AppTheme.Dark);
			validateRadioButtonColors(
				new RadioButton() { ControlTemplate = RadioButton.DefaultTemplate },
				Brush.White);

			static void validateRadioButtonColors(RadioButton button, SolidColorBrush desiredBrush)
			{
				var border = (Border)button.Children[0];
				var grid = (Grid)border.Content;
				var outerEllipse = (Shapes.Ellipse)grid.Children[0];
				var innerEllipse = (Shapes.Ellipse)grid.Children[1];

				Assert.Equal(desiredBrush, outerEllipse.Stroke);
				Assert.Equal(desiredBrush, innerEllipse.Fill);
			}
		}

		[Fact]
		public void NullApplicationCurrentFallsBackToEssentials()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};

			label.SetAppThemeColor(Label.TextColorProperty, Colors.Green, Colors.Red);

			Application.Current = null;

			app.LoadPage(new ContentPage { Content = label });

			Assert.Equal(Colors.Green, label.TextColor);

			EmulatePlatformThemeChange(AppTheme.Dark);

			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void NullApplicationCurrentFallsBackToEssentialsNonVisualElement()
		{
			var element = new NonVisualElement
			{
				Text = "Green on Light, Red on Dark"
			};

			element.SetAppThemeColor(NonVisualElement.ColorProperty, Colors.Green, Colors.Red);

			Application.Current = null;

			Assert.Equal(Colors.Green, element.Color);

			EmulatePlatformThemeChange(AppTheme.Dark);

			Assert.Equal(Colors.Red, element.Color);
		}

		[Fact]
		//https://github.com/dotnet/maui/issues/17478
		public void BindingConversion()
		{
			var border = new Border();
			border.SetAppTheme(Border.StrokeProperty, Colors.Red, Colors.Black);
		}

		[Fact, Category(TestCategory.Memory)]
		public async Task VisualElementDoesNotLeak()
		{
			(WeakReference Label, WeakReference Binding) CreateReference()
			{
				var element = new Label { Text = "Green on Light, Red on Dark" };

				var binding = new AppThemeBinding { Light = Colors.Green, Dark = Colors.Red };

				element.SetBinding(Label.TextColorProperty, binding);

				Assert.True(binding.IsApplied);

				return (new WeakReference(element), new WeakReference(binding));
			}

			var (element, binding) = CreateReference();

			// GC
			await TestHelpers.Collect();

			Assert.False(element.IsAlive, "Label should not be alive!");
			Assert.False(binding.IsAlive, "AppThemeBinding should not be alive!");
		}

		void EmulatePlatformThemeChange(AppTheme theme)
		{
			mockAppInfo.RequestedTheme = theme;
			((IApplication)app).ThemeChanged();
		}

		class NonVisualElement : Element
		{
			public static readonly BindableProperty ColorProperty = BindableProperty.Create(
				nameof(Color), typeof(Color), typeof(NonVisualElement), null);

			public static readonly BindableProperty TextProperty = BindableProperty.Create(
				nameof(Text), typeof(string), typeof(NonVisualElement), null);

			public Color Color
			{
				get => (Color)GetValue(ColorProperty);
				set => SetValue(ColorProperty, value);
			}

			public string Text
			{
				get => (string)GetValue(TextProperty);
				set => SetValue(TextProperty, value);
			}
		}
	}
}