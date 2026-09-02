#if IOS && !MACCATALYST
using System;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[CollectionDefinition(MauiDoneAccessoryViewAppContextSwitchCollection.Name, DisableParallelization = true)]
	public class MauiDoneAccessoryViewAppContextSwitchCollection
	{
		public const string Name = "MauiDoneAccessoryView AppContext switch";
	}

	[Category(TestCategory.Entry)]
	[Collection(MauiDoneAccessoryViewAppContextSwitchCollection.Name)]
	public class MauiDoneAccessoryViewTests : TestBase
	{
		// iOS 26+ shows the floating, pass-through Liquid Glass close button by default; earlier versions
		// and apps using the compatibility switch keep the original full-width touch-blocking Done toolbar.
		static bool UsesGlassButton =>
			OperatingSystem.IsIOSVersionAtLeast(26) &&
			!(AppContext.TryGetSwitch(MauiDoneAccessoryView.UseLegacyDoneAccessorySwitch, out var useLegacyDoneAccessory) && useLegacyDoneAccessory);

		[Fact]
		public async Task LegacyDoneAccessorySwitchSelectsExpectedImplementation()
		{
			var wasConfigured = AppContext.TryGetSwitch(
				MauiDoneAccessoryView.UseLegacyDoneAccessorySwitch,
				out var previousValue);

			try
			{
				await InvokeOnMainThreadAsync(() =>
				{
					if (!wasConfigured)
					{
						using var defaultAccessoryView = CreateLaidOutAccessoryView();
						AssertUsesGlassButton(defaultAccessoryView, OperatingSystem.IsIOSVersionAtLeast(26));
					}

					AppContext.SetSwitch(MauiDoneAccessoryView.UseLegacyDoneAccessorySwitch, false);
					using var modernAccessoryView = CreateLaidOutAccessoryView();

					AssertUsesGlassButton(modernAccessoryView, OperatingSystem.IsIOSVersionAtLeast(26));

					var wasClicked = false;
					AppContext.SetSwitch(MauiDoneAccessoryView.UseLegacyDoneAccessorySwitch, true);
					using var legacyAccessoryView = CreateLaidOutAccessoryView(() => wasClicked = true);

					AssertUsesGlassButton(modernAccessoryView, OperatingSystem.IsIOSVersionAtLeast(26));
					AssertUsesGlassButton(legacyAccessoryView, false);

					legacyAccessoryView.SendDoneClicked();
					Assert.True(wasClicked);

					AppContext.SetSwitch(MauiDoneAccessoryView.UseLegacyDoneAccessorySwitch, false);
					AssertUsesGlassButton(legacyAccessoryView, false);
				});
			}
			finally
			{
				AppContext.SetSwitch(
					MauiDoneAccessoryView.UseLegacyDoneAccessorySwitch,
					wasConfigured && previousValue);
			}
		}

		[Theory]
		[InlineData(UISemanticContentAttribute.ForceLeftToRight, false)]
		[InlineData(UISemanticContentAttribute.ForceRightToLeft, true)]
		public async Task HitTestOutsideDoneButtonMatchesPlatformBehavior(UISemanticContentAttribute semanticContentAttribute, bool doneButtonIsLeading)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				using var accessoryView = CreateLaidOutAccessoryView(semanticContentAttribute: semanticContentAttribute);
				var transparentHitX = doneButtonIsLeading
					? accessoryView.Bounds.GetMaxX() - 1
					: accessoryView.Bounds.GetMinX() + 1;

				var hitView = accessoryView.HitTest(new CGPoint(transparentHitX, accessoryView.Bounds.GetMidY()), null);

				if (UsesGlassButton)
					Assert.Null(hitView); // floating button lets taps pass through to the field behind
				else
					Assert.NotNull(hitView); // classic toolbar keeps blocking taps across its full width
			});
		}

		[Theory]
		[InlineData(UISemanticContentAttribute.ForceLeftToRight, false)]
		[InlineData(UISemanticContentAttribute.ForceRightToLeft, true)]
		public async Task DoneButtonActionStillRuns(UISemanticContentAttribute semanticContentAttribute, bool doneButtonIsLeading)
		{
			var wasClicked = false;

			await InvokeOnMainThreadAsync(() =>
			{
				using var accessoryView = CreateLaidOutAccessoryView(() => wasClicked = true, semanticContentAttribute);
				var doneButtonHitPoint = FindDoneButtonHitPoint(accessoryView, doneButtonIsLeading);
				Assert.NotNull(accessoryView.HitTest(doneButtonHitPoint, null));

				accessoryView.SendDoneClicked();
			});

			Assert.True(wasClicked);
		}

		[Fact]
		public async Task DoneButtonActionStillRunsWhenOtherSubviewPrecedesButton()
		{
			var wasClicked = false;

			await InvokeOnMainThreadAsync(() =>
			{
				using var accessoryView = CreateLaidOutAccessoryView(() => wasClicked = true);
				using var placeholder = new UIView();
				accessoryView.InsertSubview(placeholder, 0);

				accessoryView.SendDoneClicked();
			});

			Assert.True(wasClicked);
		}

		[Fact]
		public async Task AccessoryPreservesKeyboardLayoutHeight()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				using var accessoryView = CreateLaidOutAccessoryView();

				Assert.Equal(44, accessoryView.Frame.Height);

				if (UsesGlassButton)
				{
					var button = Assert.IsType<UIButton>(accessoryView.DoneButton);
					const double expectedBottomSpacing = 4;

					Assert.True(button.Frame.Width >= 44, $"Unexpected button width: {button.Frame.Width}");
					Assert.True(button.Frame.Height >= 44, $"Unexpected button height: {button.Frame.Height}");
					Assert.Equal(accessoryView.Bounds.GetMinY() - expectedBottomSpacing, button.Frame.GetMinY());
					Assert.Equal(accessoryView.Bounds.GetMaxY() - expectedBottomSpacing, button.Frame.GetMaxY());

					var bottomGapPoint = new CGPoint(button.Frame.GetMidX(), accessoryView.Bounds.GetMaxY() - 1);
					Assert.Null(accessoryView.HitTest(bottomGapPoint, null));
				}
			});
		}

		[Fact]
		public async Task AccessoryHasStableAccessibilityIdentifier()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				using var accessoryView = CreateLaidOutAccessoryView();

				Assert.Equal("DoneAccessory", accessoryView.AccessibilityIdentifier);
			});
		}

		[Fact]
		public async Task DoneAffordanceUsesLocalizedLabel()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				using var accessoryView = CreateLaidOutAccessoryView();

				if (UsesGlassButton)
				{
					// The floating close button supplies the localized "Done" label explicitly.
					var expected = Foundation.NSBundle.FromIdentifier("com.apple.UIKit")?.GetLocalizedString("Done") ?? "Done";

					Assert.False(string.IsNullOrEmpty(accessoryView.DoneButton?.AccessibilityLabel));
					Assert.Equal(expected, accessoryView.DoneButton?.AccessibilityLabel);
				}
				else
				{
					// The classic UIToolbar hosts a UIBarButtonSystemItem.Done, which UIKit localizes
					// for us, so the accessory is a translucent toolbar and needs no manual label.
					var toolbar = Assert.IsType<UIToolbar>(Assert.Single(accessoryView.Subviews));
					Assert.True(toolbar.Translucent);
				}
			});
		}

		static MauiDoneAccessoryView CreateLaidOutAccessoryView(
			Action doneClicked = null,
			UISemanticContentAttribute semanticContentAttribute = UISemanticContentAttribute.Unspecified)
		{
			var accessoryView = doneClicked is null
				? new MauiDoneAccessoryView()
				: new MauiDoneAccessoryView(doneClicked);

			accessoryView.Frame = new CGRect(0, 0, 400, accessoryView.Frame.Height);
			accessoryView.SemanticContentAttribute = semanticContentAttribute;

			accessoryView.SetNeedsLayout();
			accessoryView.LayoutIfNeeded();

			return accessoryView;
		}

		static void AssertUsesGlassButton(MauiDoneAccessoryView accessoryView, bool expected)
		{
			var backgroundPoint = new CGPoint(accessoryView.Bounds.GetMinX() + 1, accessoryView.Bounds.GetMidY());

			if (expected)
			{
				Assert.IsType<UIButton>(Assert.Single(accessoryView.Subviews));
				Assert.Null(accessoryView.HitTest(backgroundPoint, null));
			}
			else
			{
				Assert.IsType<UIToolbar>(Assert.Single(accessoryView.Subviews));
				Assert.NotNull(accessoryView.HitTest(backgroundPoint, null));
			}
		}

		static CGPoint FindDoneButtonHitPoint(MauiDoneAccessoryView accessoryView, bool doneButtonIsLeading)
		{
			var y = accessoryView.Bounds.GetMidY();
			var minX = (int)accessoryView.Bounds.GetMinX();
			var maxX = (int)accessoryView.Bounds.GetMaxX();

			if (doneButtonIsLeading)
			{
				for (var x = minX; x <= maxX; x++)
				{
					var point = new CGPoint(x, y);
					if (accessoryView.HitTest(point, null) is not null)
						return point;
				}
			}
			else
			{
				for (var x = maxX; x >= minX; x--)
				{
					var point = new CGPoint(x, y);
					if (accessoryView.HitTest(point, null) is not null)
						return point;
				}
			}

			throw new InvalidOperationException("Could not find a hittable Done button point.");
		}
	}
}
#endif
