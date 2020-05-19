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
using System.Maui;
using System.Maui.CustomAttributes;
using System.Maui.Platform.Android.UnitTests;

[assembly: ExportRenderer(typeof(TestShell), typeof(TestShellRenderer))]
namespace System.Maui.Platform.Android.UnitTests
{
	public class ShellTests : PlatformTestFixture
	{
		[Test, Category("Shell")]
		[Description("Flyout Header Changes When Updated")]
		public async Task FlyoutHeaderReactsToChanges()
		{
			var shell = CreateShell();
			var initialHeader = new Label() { Text = "Hello" };
			var newHeader = new Label() { Text = "Hello part 2" };
			shell.FlyoutHeader = initialHeader;
			var addedView = await Device.InvokeOnMainThreadAsync(() =>
			{
				var r = GetRenderer(shell);
				var window = (Context.GetActivity()).Window;
				(window.DecorView as global::Android.Views.ViewGroup).AddView(r.View);
				return r.View;
			});

			try
			{
				Assert.IsNotNull(addedView);
				Assert.IsNull(newHeader.GetValue(Platform.RendererProperty));
				Assert.IsNotNull(initialHeader.GetValue(Platform.RendererProperty));
				await Device.InvokeOnMainThreadAsync(() => shell.FlyoutHeader = newHeader);

				Assert.IsNotNull(newHeader.GetValue(Platform.RendererProperty), "New Header Not Set Up");
				Assert.IsNull(initialHeader.GetValue(Platform.RendererProperty), "Old Header Still Set Up");
			}
			finally
			{
				await Device.InvokeOnMainThreadAsync(() => addedView?.RemoveFromParent());
			}
		}
		
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

	public class TestShell : Shell { }
	
	public class ShellAppearanceTest : IShellAppearanceElement
	{
		public Color EffectiveTabBarBackgroundColor { get; set; }

		public Color EffectiveTabBarDisabledColor { get; set; }

		public Color EffectiveTabBarForegroundColor { get; set; }

		public Color EffectiveTabBarTitleColor { get; set; }

		public Color EffectiveTabBarUnselectedColor { get; set; }
	}
	
	public class TestShellRenderer : ShellRenderer
	{
		protected override IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer()
		{
			return base.CreateShellFlyoutContentRenderer();
		}

		public TestShellRenderer(Context context) : base(context)
		{
		}
	}
}
