#if IOS && !MACCATALYST
using System;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Entry)]
	public class MauiDoneAccessoryViewTests : TestBase
	{
		[Theory]
		[InlineData(UISemanticContentAttribute.ForceLeftToRight, false)]
		[InlineData(UISemanticContentAttribute.ForceRightToLeft, true)]
		public async Task HitTestOutsideDoneButtonReturnsNull(UISemanticContentAttribute semanticContentAttribute, bool doneButtonIsLeading)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				using var accessoryView = CreateLaidOutAccessoryView(semanticContentAttribute: semanticContentAttribute);
				var transparentHitX = doneButtonIsLeading
					? accessoryView.Bounds.GetMaxX() - 1
					: accessoryView.Bounds.GetMinX() + 1;

				var hitView = accessoryView.HitTest(new CGPoint(transparentHitX, accessoryView.Bounds.GetMidY()), null);

				Assert.Null(hitView);
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
		public async Task AccessorySizesToNaturalButtonHeight()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				using var accessoryView = new MauiDoneAccessoryView();

				// The height should be driven by the button's natural size plus margins, not a zero/clipped frame.
				Assert.True(accessoryView.Frame.Height > 40, $"Unexpected accessory height: {accessoryView.Frame.Height}");
			});
		}

		[Fact]
		public async Task DoneButtonUsesLocalizedAccessibilityLabel()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				using var accessoryView = CreateLaidOutAccessoryView();

				var expected = Foundation.NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("Done");

				Assert.False(string.IsNullOrEmpty(accessoryView.DoneButton?.AccessibilityLabel));
				Assert.Equal(expected, accessoryView.DoneButton?.AccessibilityLabel);
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
