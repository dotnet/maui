using System;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class AppThemeTests : BaseTestFixture
	{
		MockAppInfo mockAppInfo;
		Application app;

		[SetUp]
		public override void Setup()
		{
			base.Setup();

			AppInfo.SetCurrent(mockAppInfo = new MockAppInfo() { RequestedTheme = AppTheme.Light });
			Application.Current = app = new Application();
		}

		[TearDown]
		public override void TearDown()
		{
			Application.Current = null;

			base.TearDown();
		}

		[Test]
		public void ThemeChangeUsingSetAppThemeColor()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};

			label.SetAppThemeColor(Label.TextColorProperty, Colors.Green, Colors.Red);
			Assert.AreEqual(Colors.Green, label.TextColor);

			SetAppTheme(AppTheme.Dark);

			Assert.AreEqual(Colors.Red, label.TextColor);
		}

		[Test]
		public void ThemeChangeUsingSetAppTheme()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};

			label.SetOnAppTheme(Label.TextColorProperty, Colors.Green, Colors.Red);
			Assert.AreEqual(Colors.Green, label.TextColor);

			SetAppTheme(AppTheme.Dark);

			Assert.AreEqual(Colors.Red, label.TextColor);
		}

		[Test]
		public void ThemeChangeUsingSetBinding()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};

			label.SetBinding(Label.TextColorProperty, new AppThemeBinding { Light = Colors.Green, Dark = Colors.Red });
			Assert.AreEqual(Colors.Green, label.TextColor);

			SetAppTheme(AppTheme.Dark);

			Assert.AreEqual(Colors.Red, label.TextColor);
		}

		[Test]
		public void ThemeChangeUsingUserAppTheme()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};

			label.SetAppThemeColor(Label.TextColorProperty, Colors.Green, Colors.Red);
			Assert.AreEqual(Colors.Green, label.TextColor);

			app.UserAppTheme = AppTheme.Dark;

			Assert.AreEqual(Colors.Red, label.TextColor);
		}

		[Test]
		public void InitialThemeIsCorrect()
		{
			var changed = 0;
			var newTheme = AppTheme.Unspecified;

			app.RequestedThemeChanged += (_, e) =>
			{
				changed++;
				newTheme = e.RequestedTheme;
			};

			Assert.AreEqual(AppTheme.Light, app.RequestedTheme);
			Assert.AreEqual(AppTheme.Light, app.PlatformAppTheme);
			Assert.AreEqual(AppTheme.Unspecified, app.UserAppTheme);

			Assert.AreEqual(0, changed);
			Assert.AreEqual(AppTheme.Unspecified, newTheme);
		}

		[Test]
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

			Assert.AreEqual(AppTheme.Light, app.RequestedTheme);
			Assert.AreEqual(AppTheme.Light, app.PlatformAppTheme);
			Assert.AreEqual(AppTheme.Light, app.UserAppTheme);

			Assert.AreEqual(0, changed);
			Assert.AreEqual(AppTheme.Unspecified, newTheme);
		}

		[Test]
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

			Assert.AreEqual(AppTheme.Dark, app.RequestedTheme);
			Assert.AreEqual(AppTheme.Light, app.PlatformAppTheme);
			Assert.AreEqual(AppTheme.Dark, app.UserAppTheme);

			Assert.AreEqual(1, changed);
			Assert.AreEqual(AppTheme.Dark, newTheme);
		}

		[Test]
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

			Assert.AreEqual(AppTheme.Light, app.RequestedTheme);
			Assert.AreEqual(AppTheme.Light, app.PlatformAppTheme);
			Assert.AreEqual(AppTheme.Unspecified, app.UserAppTheme);

			Assert.AreEqual(2, changed);
			Assert.AreEqual(AppTheme.Light, newTheme);
		}

		void SetAppTheme(AppTheme theme)
		{
			mockAppInfo.RequestedTheme = theme;
			((IApplication)app).ThemeChanged();
		}
	}
}