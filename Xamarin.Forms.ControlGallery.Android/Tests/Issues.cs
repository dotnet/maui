using System;
using Android.Views;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.ControlGallery.Android.Tests
{
	[TestFixture]
	public class Issues : PlatformTestFixture
	{
		[Test, Category("Entry")]
		[Description("The HorizontalAlignment of an Entry's renderer should match the Entry")]
		[Issue(IssueTracker.Github, 8137, "[Bug] XF 4.3 Entry HorizontalTextAlignment display wrong position")]
		public void EntryHorizontalAlignmentCenterInRenderer()
		{
			bool supportsRTL = Context.HasRtlSupport();

			try
			{
				// Test with RTL support off 
				ToggleRTLSupport(Context, false);

				var entry1 = new Entry { Text = "foo", HorizontalTextAlignment = TextAlignment.Center };
				using (var editText = GetNativeControl(entry1))
				{
					var centeredHorizontal =
					(editText.Gravity & GravityFlags.HorizontalGravityMask) == GravityFlags.CenterHorizontal;

					Assert.That(centeredHorizontal, Is.True);
				}

				// Now turn it back on and verify it works
				ToggleRTLSupport(Context, true);

				var entry2 = new Entry { Text = "foo", HorizontalTextAlignment = TextAlignment.Center };
				using (var editText = GetNativeControl(entry2))
				{
					Assert.That(editText.TextAlignment, Is.EqualTo(global::Android.Views.TextAlignment.Center));
				}
			}
			finally
			{
				// If something went wrong, make sure we leave the Context as it was when we started
				ToggleRTLSupport(Context, supportsRTL);
			}
		}

		[Test(Description = "No exceptions should be thrown")]
		[Issue(IssueTracker.Github, 9185, "[Bug] Java.Lang.IllegalArgumentException: 'order does not contain a valid category.'")]
		public void ToolbarItemWithReallyHighPriorityDoesntCrash()
		{
			try
			{
				ContentPage page = new ContentPage()
				{
					ToolbarItems =
					{
						new ToolbarItem() { Text = "2", Priority = int.MaxValue },
						new ToolbarItem() { Text = "1", Priority = int.MaxValue - 1 }
					}
				};

				GetRenderer(new NavigationPage(page));
			}
			catch (Exception exc)
			{
				Assert.Fail($"{exc}");
			}
		}
	}
}
