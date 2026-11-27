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
		public Task UpdateToolTipNullClearsExistingInteractionOnFirstUpdate() =>
			InvokeOnMainThreadAsync(() =>
			{
				using var view = new CountingView();
				var existing = new UIToolTipInteraction("old");
				view.AddInteraction(existing);

				view.UpdateToolTip(null);

				Assert.True(string.IsNullOrEmpty(existing.DefaultToolTip));
				Assert.Single(view.CountedInteractions());
			});

		[Fact]
		public Task UpdateToolTipNullWithoutInteractionDoesNotAddOne() =>
			InvokeOnMainThreadAsync(() =>
			{
				using var view = new CountingView();

				view.UpdateToolTip(null);

				Assert.Empty(view.CountedInteractions());
			});

		[Fact]
		public Task UpdateToolTipNullClearsInteractionReplacedNatively() =>
			InvokeOnMainThreadAsync(() =>
			{
				using var view = new CountingView();
				view.UpdateToolTip(new ToolTip { Content = "first" });
				view.RemoveInteraction(Assert.Single(view.CountedInteractions()));
				var replacement = new UIToolTipInteraction("old");
				view.AddInteraction(replacement);

				view.UpdateToolTip(null);

				Assert.True(string.IsNullOrEmpty(replacement.DefaultToolTip));
			});

		[Fact]
		public Task UpdateToolTipWithEmptyTextClearsExistingInteraction() =>
			InvokeOnMainThreadAsync(() =>
			{
				using var view = new CountingView();
				var existing = new UIToolTipInteraction("old");
				view.AddInteraction(existing);

				view.UpdateToolTip(new ToolTip { Content = "" });

				Assert.True(string.IsNullOrEmpty(existing.DefaultToolTip));
				Assert.Single(view.CountedInteractions());
			});

		[Fact]
		public Task UpdateToolTipDoesNotReadInteractionsAfterDiscovery() =>
			InvokeOnMainThreadAsync(() =>
			{
				using var view = new CountingView();
				view.AddInteraction(new UIToolTipInteraction("old"));

				view.UpdateToolTip(new ToolTip { Content = "first" });
				var readsAfterDiscovery = view.Reads;

				view.UpdateToolTip(new ToolTip { Content = "second" });
				view.UpdateToolTip(null);

				Assert.Equal(readsAfterDiscovery, view.Reads);
			});

		[Fact]
		public Task UpdateToolTipReusesInteractionItCreated() =>
			InvokeOnMainThreadAsync(() =>
			{
				using var view = new CountingView();

				view.UpdateToolTip(new ToolTip { Content = "first" });
				var readsAfterCreation = view.Reads;

				view.UpdateToolTip(new ToolTip { Content = "second" });

				var interaction = Assert.IsType<UIToolTipInteraction>(Assert.Single(view.CountedInteractions()));
				Assert.Equal("second", interaction.DefaultToolTip);
				Assert.Equal(readsAfterCreation, view.Reads);
			});

		[Fact]
		public Task UpdateToolTipRecreatesInteractionRemovedNatively() =>
			InvokeOnMainThreadAsync(() =>
			{
				using var view = new CountingView();

				view.UpdateToolTip(new ToolTip { Content = "first" });
				view.RemoveInteraction(Assert.Single(view.CountedInteractions()));

				view.UpdateToolTip(new ToolTip { Content = "second" });

				var interaction = Assert.IsType<UIToolTipInteraction>(Assert.Single(view.CountedInteractions()));
				Assert.Equal("second", interaction.DefaultToolTip);
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
