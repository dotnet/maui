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
		[Fact]
		public async Task HitTestOutsideDoneButtonReturnsNull()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				using var accessoryView = CreateLaidOutAccessoryView();

				var hitView = accessoryView.HitTest(new CGPoint(1, accessoryView.Bounds.GetMidY()), null);

				Assert.Null(hitView);
			});
		}

		[Fact]
		public async Task DoneButtonActionStillRuns()
		{
			var wasClicked = false;

			await InvokeOnMainThreadAsync(() =>
			{
				using var accessoryView = CreateLaidOutAccessoryView(() => wasClicked = true);
				var doneButtonHitPoint = new CGPoint(accessoryView.Bounds.GetMaxX() - 22, accessoryView.Bounds.GetMidY());
				Assert.NotNull(accessoryView.HitTest(doneButtonHitPoint, null));

				var doneButton = Assert.IsType<UIBarButtonItem>(accessoryView.Items[1]);

				UIApplication.SharedApplication.SendAction(doneButton.Action, doneButton.Target, null, null);
			});

			Assert.True(wasClicked);
		}

		static MauiDoneAccessoryView CreateLaidOutAccessoryView(Action doneClicked = null)
		{
			var accessoryView = doneClicked is null
				? new MauiDoneAccessoryView()
				: new MauiDoneAccessoryView(doneClicked);

			accessoryView.Frame = new CGRect(0, 0, 400, 44);

			accessoryView.SetNeedsLayout();
			accessoryView.LayoutIfNeeded();

			return accessoryView;
		}
	}
}
#endif
