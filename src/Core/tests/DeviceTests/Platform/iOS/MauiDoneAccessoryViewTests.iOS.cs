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
