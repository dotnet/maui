using ImAVendor.Forms.PlatformConfiguration.iOS;
using NUnit.Framework;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class PlatformSpecificsTests
	{
		[Test]
		public void VendorPlatformProperty()
		{
			var x = new FlyoutPage();

			Assert.IsTrue(x.On<iOS>().GetVendorFoo());

			x.On<iOS>().SetVendorFoo(false);

			Assert.IsFalse(x.On<iOS>().GetVendorFoo());
		}

		[Test]
		public void ConsumeVendorSetting()
		{
			var x = new FlyoutPage();
			x.On<iOS>().SetVendorFoo(false);

			Assert.IsFalse(x.On<iOS>().GetVendorFoo());
		}

		[Test]
		public void Properties()
		{
			var x = new FlyoutPage();
			x.On<Android>().SetSomeAndroidThing(42);

			Assert.IsTrue(x.On<Android>().GetSomeAndroidThing() == 42);
		}

		[Test]
		public void ConvenienceConfiguration()
		{
			var x = new FlyoutPage();

			x.On<Android>().UseTabletDefaults();

			Assert.IsTrue(x.On<Android>().GetSomeAndroidThing() == 10);
			Assert.IsTrue(x.On<Android>().GetSomeOtherAndroidThing() == 45);

			x.On<Android>().UsePhabletDefaults();

			Assert.IsTrue(x.On<Android>().GetSomeAndroidThing() == 8);
			Assert.IsTrue(x.On<Android>().GetSomeOtherAndroidThing() == 40);
		}

		[Test]
		public void NavigationPageiOSConfiguration()
		{
			var x = new NavigationPage();

			x.On<iOS>().SetIsNavigationBarTranslucent(true);

			Assert.IsTrue(x.On<iOS>().IsNavigationBarTranslucent());
		}
	}
}

namespace ImAVendor.Forms.PlatformConfiguration.iOS
{
	using Microsoft.Maui.Controls;
	using Microsoft.Maui.Controls.PlatformConfiguration;
	using FormsElement = Microsoft.Maui.Controls.FlyoutPage;

	public static class FlyoutPage
	{
		public static readonly BindableProperty FooProperty =
			BindableProperty.Create("VendorFoo", typeof(bool),
			typeof(FlyoutPage), true);

		public static void SetVendorFoo(BindableObject element, bool value)
		{
			element.SetValue(FooProperty, value);
		}

		public static bool GetVendorFoo(BindableObject element)
		{
			return (bool)element.GetValue(FooProperty);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetVendorFoo(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetVendorFoo(config.Element, value);
			return config;
		}

		public static bool GetVendorFoo(this IPlatformElementConfiguration<iOS, FormsElement> mdp)
		{
			return GetVendorFoo(mdp.Element);
		}
	}
}

namespace ImAVendor.Forms.PlatformConfiguration.iOS
{
	using Microsoft.Maui.Controls;
	using Microsoft.Maui.Controls.PlatformConfiguration;
	using FormsElement = Microsoft.Maui.Controls.NavigationPage;

	public static class NavigationPage
	{
		const string NavBarTranslucentEffectName = "XamControl.NavigationPageTranslucentEffect";

		public static readonly BindableProperty IsNavigationBarTranslucentProperty =
			BindableProperty.CreateAttached("IsNavigationBarTranslucent", typeof(bool),
			typeof(NavigationPage), false, propertyChanging: IsNavigationBarTranslucentPropertyChanging);

		public static bool GetIsNavigationBarTranslucent(BindableObject element)
		{
			return (bool)element.GetValue(IsNavigationBarTranslucentProperty);
		}

		public static void SetIsNavigationBarTranslucent(BindableObject element, bool value)
		{
			element.SetValue(IsNavigationBarTranslucentProperty, value);
		}

		public static bool IsNavigationBarTranslucentVendor(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetIsNavigationBarTranslucent(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> EnableTranslucentNavigationBarVendor(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetIsNavigationBarTranslucent(config.Element, value);
			return config;
		}

		static void IsNavigationBarTranslucentPropertyChanging(BindableObject bindable, object oldValue, object newValue)
		{
			AttachEffect(bindable as FormsElement);
		}

		static void AttachEffect(FormsElement element)
		{
			IElementController controller = element;
			if (controller == null || controller.EffectIsAttached(NavBarTranslucentEffectName))
				return;

			element.Effects.Add(Effect.Resolve(NavBarTranslucentEffectName));
		}
	}
}

namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Microsoft.Maui.Controls.FlyoutPage;

	public static class FlyoutPage
	{
		public static readonly BindableProperty SomeAndroidThingProperty =
			BindableProperty.Create("SomeAndroidThing", typeof(int),
			typeof(FlyoutPage), 1);

		public static readonly BindableProperty SomeOtherAndroidThingProperty =
			BindableProperty.Create("SomeOtherAndroidThing", typeof(int),
			typeof(FlyoutPage), 1);

		public static int GetSomeAndroidThing(BindableObject element)
		{
			return (int)element.GetValue(SomeAndroidThingProperty);
		}

		public static void SetSomeAndroidThing(BindableObject element, int value)
		{
			element.SetValue(SomeAndroidThingProperty, value);
		}

		public static int GetSomeOtherAndroidThing(BindableObject element)
		{
			return (int)element.GetValue(SomeOtherAndroidThingProperty);
		}

		public static void SetSomeOtherAndroidThing(BindableObject element, int value)
		{
			element.SetValue(SomeOtherAndroidThingProperty, value);
		}

		public static int GetSomeAndroidThing(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return (int)config.Element.GetValue(SomeAndroidThingProperty);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetSomeAndroidThing(this IPlatformElementConfiguration<Android, FormsElement> config,
			int value)
		{
			config.Element.SetValue(SomeAndroidThingProperty, value);
			return config;
		}

		public static int GetSomeOtherAndroidThing(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return (int)config.Element.GetValue(SomeOtherAndroidThingProperty);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetSomeOtherAndroidThing(this IPlatformElementConfiguration<Android, FormsElement> config, int value)
		{
			config.Element.SetValue(SomeOtherAndroidThingProperty, value);
			return config;
		}

		public static IPlatformElementConfiguration<Android, FormsElement> UseTabletDefaults(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			config.SetSomeAndroidThing(10);
			config.SetSomeOtherAndroidThing(45);
			return config;
		}

		public static IPlatformElementConfiguration<Android, FormsElement> UsePhabletDefaults(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			config.SetSomeAndroidThing(8);
			config.SetSomeOtherAndroidThing(40);
			return config;
		}
	}
}
