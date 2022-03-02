using System;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class AppThemeTests : BaseTestFixture
	{
		MockAppInfo mockAppInfo;

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			AppInfo.SetCurrent(mockAppInfo = new MockAppInfo());
			Application.Current = new MockApplication();
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

		void SetAppTheme(AppTheme theme)
		{
			mockAppInfo.RequestedTheme = theme;
			Application.Current.ThemeChanged();
		}
	}
}