using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using NUnit.Framework;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Platform.Android.UnitTests
{
	public class ShellTests : PlatformTestFixture
	{
		[Test, Category("Shell")]
		[Description("Ensure Default Colors are White for BottomNavigationView")]
		public async Task ShellTabColorsDefaultToWhite()
		{
			var shell = CreateShell();
			var tracker = new ShellBottomNavViewAppearanceTracker(null, shell.Items[0]);
			BottomNavigationView bottomView = new BottomNavigationView(this.Context);
			bottomView.Menu.Add("test");
			ColorChangeRevealDrawable ccr = 
				await Device.InvokeOnMainThreadAsync(() =>
				{
					tracker.SetAppearance(bottomView, new ShellAppearanceTest());
					return (ColorChangeRevealDrawable)bottomView.Background;
				});

			Assert.AreEqual(Color.White.ToAndroid(), ccr.EndColor);
		}

		public class ShellAppearanceTest : IShellAppearanceElement
		{
			public Color EffectiveTabBarBackgroundColor { get; set; }

			public Color EffectiveTabBarDisabledColor { get; set; }

			public Color EffectiveTabBarForegroundColor { get; set; }

			public Color EffectiveTabBarTitleColor { get; set; }

			public Color EffectiveTabBarUnselectedColor { get; set; }
		}
		Shell CreateShell()
		{
			return new Shell()
			{
				Items =
				{
					new FlyoutItem()
					{
						Items =
						{
							new Tab()
							{
								Items =
								{
									new ShellContent()
									{
										Content = new ContentPage()
									}
								}
							}
						}
					}
				}
			};
		}
	}
}