using System;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class AppThemeTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Application.Current = new MockApplication();
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

			SetAppTheme(OSAppTheme.Dark);

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

			SetAppTheme(OSAppTheme.Dark);

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

			SetAppTheme(OSAppTheme.Dark);

			Assert.AreEqual(Colors.Red, label.TextColor);
		}

		void SetAppTheme(OSAppTheme theme)
		{
			((MockPlatformServices)Device.PlatformServices).RequestedTheme = theme;
			Application.Current.TriggerThemeChanged(new AppThemeChangedEventArgs(theme));
		}
	}
}