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
		public async Task HitTestOutsideDoneButtonReturnsNull(UISemanticContentAttribute semanticContentAttribute, bool doneToolbarIsLeading)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				using var accessoryView = CreateLaidOutAccessoryView(semanticContentAttribute: semanticContentAttribute);
				var transparentHitX = doneToolbarIsLeading
					? accessoryView.Bounds.GetMaxX() - 1
					: accessoryView.Bounds.GetMinX() + 1;

				var hitView = accessoryView.HitTest(new CGPoint(transparentHitX, accessoryView.Bounds.GetMidY()), null);

				Assert.Null(hitView);
			});
		}

		[Theory]
		[InlineData(UISemanticContentAttribute.ForceLeftToRight, false)]
		[InlineData(UISemanticContentAttribute.ForceRightToLeft, true)]
		public async Task DoneButtonActionStillRuns(UISemanticContentAttribute semanticContentAttribute, bool doneToolbarIsLeading)
		{
			var wasClicked = false;

			await InvokeOnMainThreadAsync(() =>
			{
				using var accessoryView = CreateLaidOutAccessoryView(() => wasClicked = true, semanticContentAttribute);
				var doneButtonHitX = doneToolbarIsLeading
					? accessoryView.Bounds.GetMinX() + 22
					: accessoryView.Bounds.GetMaxX() - 22;
				var doneButtonHitPoint = new CGPoint(doneButtonHitX, accessoryView.Bounds.GetMidY());
				Assert.NotNull(accessoryView.HitTest(doneButtonHitPoint, null));

				var doneButton = Assert.IsType<UIBarButtonItem>(accessoryView.Items[1]);

				UIApplication.SharedApplication.SendAction(doneButton.Action, doneButton.Target, null, null);
			});

			Assert.True(wasClicked);
		}

		[Fact]
		public async Task ItemsFindsToolbarWhenOtherSubviewPrecedesIt()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				using var accessoryView = CreateLaidOutAccessoryView();
				using var placeholder = new UIView();
				accessoryView.InsertSubview(placeholder, 0);

				var items = accessoryView.Items;

				Assert.NotNull(items);
				Assert.Equal(2, items.Length);
				Assert.IsType<UIBarButtonItem>(items[1]);
			});
		}

		static MauiDoneAccessoryView CreateLaidOutAccessoryView(
			Action doneClicked = null,
			UISemanticContentAttribute semanticContentAttribute = UISemanticContentAttribute.Unspecified)
		{
			var accessoryView = doneClicked is null
				? new MauiDoneAccessoryView()
				: new MauiDoneAccessoryView(doneClicked);

			accessoryView.Frame = new CGRect(0, 0, 400, 44);
			accessoryView.SemanticContentAttribute = semanticContentAttribute;

			accessoryView.SetNeedsLayout();
			accessoryView.LayoutIfNeeded();

			return accessoryView;
		}
	}
}
#endif
