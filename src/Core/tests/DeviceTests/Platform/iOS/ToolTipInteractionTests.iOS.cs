using System.Threading.Tasks;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.View)]
	public class ToolTipInteractionTests : TestBase
	{
		[Fact]
		public Task UpdateToolTipReusesExistingInteraction() =>
			InvokeOnMainThreadAsync(() =>
			{
				using var view = new CountingView();
				var existing = new UIToolTipInteraction("old");
				view.AddInteraction(existing);

				view.UpdateToolTip(new ToolTip { Content = "new" });

				Assert.Single(view.CountedInteractions());
				Assert.Equal("new", existing.DefaultToolTip);
			});

		[Fact]
		public Task UpdateToolTipClearsExistingInteraction() =>
			InvokeOnMainThreadAsync(() =>
			{
				using var view = new CountingView();
				var existing = new UIToolTipInteraction("old");
				view.AddInteraction(existing);

				view.UpdateToolTip(new ToolTip { Content = "new" });
				view.UpdateToolTip(null);

				Assert.Null(existing.DefaultToolTip);
				Assert.Single(view.CountedInteractions());
			});

		[Fact]
		public Task UpdateToolTipWithoutToolTipDoesNotReadInteractions() =>
			InvokeOnMainThreadAsync(() =>
			{
				using var view = new CountingView();

				view.UpdateToolTip(null);

				Assert.Equal(0, view.Reads);
			});

		class CountingView : UIView
		{
			public int Reads { get; private set; }

			public override IUIInteraction[] Interactions
			{
				get
				{
					Reads++;
					return base.Interactions;
				}
			}

			public IUIInteraction[] CountedInteractions() => base.Interactions;
		}
	}
}
