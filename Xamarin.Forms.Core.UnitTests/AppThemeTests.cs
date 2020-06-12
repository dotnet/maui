using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	public class AppThemeTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Application.Current = new MockApplication();

			Device.SetFlags(new[] { ExperimentalFlags.AppThemeExperimental });
		}

		[Test]
		public void ThemeChangeUsingSetAppThemeColor()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};

			label.SetAppThemeColor(Label.TextColorProperty, Color.Green, Color.Red);
			Assert.AreEqual(Color.Green, label.TextColor);

			SetAppTheme(OSAppTheme.Dark);

			Assert.AreEqual(Color.Red, label.TextColor);
		}

		[Test]
		public void ThemeChangeUsingSetAppTheme()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};

			label.SetOnAppTheme(Label.TextColorProperty, Color.Green, Color.Red);
			Assert.AreEqual(Color.Green, label.TextColor);

			SetAppTheme(OSAppTheme.Dark);

			Assert.AreEqual(Color.Red, label.TextColor);
		}

		[Test]
		public void ThemeChangeUsingSetBinding()
		{
			var label = new Label
			{
				Text = "Green on Light, Red on Dark"
			};

			label.SetBinding(Label.TextColorProperty, new AppThemeBinding { Light = Color.Green, Dark = Color.Red });
			Assert.AreEqual(Color.Green, label.TextColor);

			SetAppTheme(OSAppTheme.Dark);

			Assert.AreEqual(Color.Red, label.TextColor);
		}

		void SetAppTheme(OSAppTheme theme)
		{
			((MockPlatformServices)Device.PlatformServices).RequestedTheme = theme;
			Application.Current.TriggerThemeChanged(new AppThemeChangedEventArgs(theme));
		}
	}
}