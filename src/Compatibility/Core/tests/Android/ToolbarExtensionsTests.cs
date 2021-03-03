using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.View.Menu;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.UnitTests;
using AToolBar = AndroidX.AppCompat.Widget.Toolbar;
using AView = Android.Views.View;

[assembly: ExportRenderer(typeof(TestShell), typeof(TestShellRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.UnitTests
{
	public class ToolbarExtensionsTests : PlatformTestFixture
	{
		[Test, Category("ToolbarExtensions")]
		[Description("ToolbarItem Text Set Correctly")]
		public void TextSetsCorrectlyWithNoTintColor()
		{
			List<ToolbarItem> sortedItems = new List<ToolbarItem>()
			{
				new ToolbarItem() { IsEnabled = true, Text = "a" },
				new ToolbarItem() { IsEnabled = true, Text = "b" },
				new ToolbarItem() { IsEnabled = true, Text = "c" },
			};

			var settings = new ToolbarSettings(sortedItems);
			SetupToolBar(settings, Context);


			int i = 0;
			foreach(var textView in settings.TextViews)
			{
				Assert.AreEqual(sortedItems[i].Text, textView.Text);
				i++;
			}
		}


		[Test, Category("ToolbarExtensions")]
		[Description("ToolbarItem with Empty String doesn't crash")]
		public void DoesntCrashWithEmptyStringOnText()
		{
			List<ToolbarItem> sortedItems = new List<ToolbarItem>()
			{
				new ToolbarItem(),
			};

			// If this doesn't crash test has passed
			SetupToolBar(new ToolbarSettings(sortedItems), Context);
		}

		[Test, Category("ToolbarExtensions")]
		[Description("Validate ToolBarItem TextColor Changes")]
		public void ToolBarItemsColoredCorrectlyBasedOnEnabledDisabled()
		{
			try
			{
				List<ToolbarItem> sortedItems = new List<ToolbarItem>()
				{
					new ToolbarItem() { IsEnabled = true, Text = "a" },
					new ToolbarItem() { IsEnabled = true, Text = "b" },
					new ToolbarItem() { IsEnabled = true, Text = "c" },
				};

				var settings = new ToolbarSettings(sortedItems) { TintColor = Color.Red };
				SetupToolBar(settings, Context);
				AToolBar aToolBar = settings.ToolBar;
				List<IMenuItem> menuItemsCreated = settings.MenuItemsCreated;
				Assert.IsTrue(menuItemsCreated[2].IsEnabled, "Initial state of menu Item is not enabled");
				sortedItems[2].IsEnabled = false;

				Assert.IsTrue(menuItemsCreated[0].IsEnabled, "Menu Item 1 is incorrectly disabled");
				Assert.IsTrue(menuItemsCreated[1].IsEnabled, "Menu Item 2 is incorrectly disabled");
				Assert.IsFalse(menuItemsCreated[2].IsEnabled, "Menu Item 3 is incorrectly disabled");

				var textViews = settings.TextViews.ToList();
				Assert.AreEqual(3, textViews.Count, $"{textViews.Count} textviews retrieved which it should have been 3");

				settings.Layout();
				for (int i = 0; i < 3; i++)
				{
					global::Android.Graphics.Color androidColor;
					if (i != 2)
					{
						androidColor = Color.Red.ToAndroid();
					}
					else
					{
						androidColor = Color.Red.MultiplyAlpha(0.302).ToAndroid();
					}


					textViews[i].AssertContainsColor(androidColor);
					Assert.AreEqual(sortedItems[i].Text, textViews[i].Text);
				}
			}
			catch(Exception exc)
			{
				Assert.Fail(exc.ToString());
			}
		}
				
		[Test, Category("ToolbarExtensions")]
		[Description("Secondary ToolBarItems don't Change Color based on ForegroundColor")]
		public void SecondaryToolbarItemsDontChangeColor()
		{
			List<ToolbarItem> sortedItems = new List<ToolbarItem>()
			{
				new ToolbarItem() { IsEnabled = true, Text = "a", Order = ToolbarItemOrder.Secondary },
			};

			var settings = new ToolbarSettings(sortedItems) { TintColor = Color.Red };
			SetupToolBar(settings, Context);
			AToolBar aToolBar = settings.ToolBar;
			IMenuItem menuItem = settings.MenuItemsCreated.First();

			MenuItemImpl menuItemImpl = (MenuItemImpl)menuItem;
			Assert.IsNotNull(menuItemImpl, "menuItem is not of type MenuItemImpl");

			if(menuItemImpl.TitleFormatted is SpannableString tf)
			{
				var colorSpan =
					tf.GetSpans(0, tf.Length(), Java.Lang.Class.FromType(typeof(ForegroundColorSpan)))
					.OfType<ForegroundColorSpan>()
					.FirstOrDefault();

				if (colorSpan != null)
				{
					Assert.AreNotEqual(
						colorSpan.ForegroundColor,
						(int)Color.Red.ToAndroid(),
						"Secondary Menu Item Incorrectly set to ForegroundColor");
				}
			}
		}

		static void SetupToolBar(ToolbarSettings settings, Context context)
		{
			foreach(var item in settings.ToolbarItems)
			{
				if (String.IsNullOrWhiteSpace(item.AutomationId) && !String.IsNullOrWhiteSpace(item.Text))
					item.AutomationId = item.Text;
			}

			settings.ToolBar = new AToolBar(context);

			ToolbarExtensions.UpdateMenuItems(
				settings.ToolBar,
				settings.ToolbarItems,
				context,
				settings.TintColor,
				OnToolbarItemPropertyChanged,
				settings.MenuItemsCreated,
				settings.ToolbarItemsCreated
			);

			void OnToolbarItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
			{
				settings.ToolBar.OnToolbarItemPropertyChanged(e,
					(ToolbarItem)sender, settings.ToolbarItems, context, settings.TintColor, OnToolbarItemPropertyChanged,
					settings.MenuItemsCreated,
					settings.ToolbarItemsCreated);
			}
		}

		[Preserve(AllMembers = true)]
		public class ToolbarSettings
		{
			public ToolbarSettings(List<ToolbarItem> toolbarItems)
			{
				ToolbarItems = toolbarItems;
				MenuItemsCreated = new List<IMenuItem>();
				ToolbarItemsCreated = new List<ToolbarItem>();
			}

			public void Layout(int width = 800, int height = 200)
			{
				int widthSpec = AView.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly);
				int heightSpec = AView.MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly);

				ToolBar.Measure(widthSpec, heightSpec);
				ToolBar.Layout(0, 0, width, height);
			}

			public List<ToolbarItem> ToolbarItems;
			public List<ToolbarItem> ToolbarItemsCreated;
			public AToolBar ToolBar;
			public Color? TintColor;
			public List<IMenuItem> MenuItemsCreated;

			public IEnumerable<ActionMenuItemView> TextViews =>
				ToolBar.GetChildrenOfType<ActionMenuItemView>()
					.OrderBy(x => x.Text);
		}
	}
}
