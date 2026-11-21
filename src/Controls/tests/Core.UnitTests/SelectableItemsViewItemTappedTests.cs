using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class SelectableItemsViewItemTappedTests : BaseTestFixture
	{
		[Fact]
		public void UngroupedTapReportsItemAndSourceIndex()
		{
			var collectionView = new CollectionView { ItemsSource = new[] { "a", "b", "c" } };
			var taps = new List<ItemTappedEventArgs>();
			collectionView.ItemTapped += (_, e) => taps.Add(e);

			collectionView.SendItemTapped("c", null, -1, 2);

			var tap = Assert.Single(taps);
			Assert.Equal("c", tap.Item);
			Assert.Null(tap.Group);
			Assert.Equal(2, tap.ItemIndex);
		}

		[Theory]
		[InlineData(0, 0, "a")]
		[InlineData(0, 1, "b")]
		[InlineData(1, 0, "c")]
		[InlineData(1, 2, "e")]
		[InlineData(2, 0, "f")]
		public void GroupedTapIndexMatchesListView(int groupIndex, int indexInGroup, string item)
		{
			var groups = CreateGroups();
			var collectionView = new CollectionView { IsGrouped = true, ItemsSource = groups };
			var taps = new List<ItemTappedEventArgs>();
			collectionView.ItemTapped += (_, e) => taps.Add(e);

			var listView = new ListView { IsGroupingEnabled = true, ItemsSource = groups };

			collectionView.SendItemTapped(item, groups[groupIndex], groupIndex, indexInGroup);

			var tap = Assert.Single(taps);
			Assert.Equal(item, tap.Item);
			Assert.Same(groups[groupIndex], tap.Group);
			Assert.Equal(listView.TemplatedItems.GetGlobalIndexOfItem(item), tap.ItemIndex);
		}

		[Fact]
		public void UnknownPositionIsResolvedFromItemsSource()
		{
			var groups = CreateGroups();
			var collectionView = new CollectionView { IsGrouped = true, ItemsSource = groups };
			ItemTappedEventArgs tap = null;
			collectionView.ItemTapped += (_, e) => tap = e;

			collectionView.SendItemTapped("d", null, -1, -1);

			Assert.Equal("d", tap.Item);
			Assert.Same(groups[1], tap.Group);
			Assert.Equal(5, tap.ItemIndex);
		}

		[Fact]
		public void KnownPositionIsUsedForRepeatedItems()
		{
			var groups = new List<List<string>> { new() { "a" }, new() { "a", "a" } };
			var collectionView = new CollectionView { IsGrouped = true, ItemsSource = groups };
			var taps = new List<ItemTappedEventArgs>();
			collectionView.ItemTapped += (_, e) => taps.Add(e);

			// Handlers can know the group, or only its index
			collectionView.SendItemTapped("a", groups[1], -1, 1);
			collectionView.SendItemTapped("a", null, 1, 0);

			Assert.Same(groups[1], taps[0].Group);
			Assert.Equal(4, taps[0].ItemIndex);
			Assert.Same(groups[1], taps[1].Group);
			Assert.Equal(3, taps[1].ItemIndex);
		}

		[Fact]
		public void CommandExecutesWithItsParameterBeforeEvent()
		{
			var order = new List<string>();
			object commandParameter = null;
			var collectionView = new CollectionView
			{
				ItemsSource = new[] { "a", "b" },
				ItemTappedCommand = new Command(p => { commandParameter = p; order.Add("command"); }),
				ItemTappedCommandParameter = "parameter",
			};
			collectionView.ItemTapped += (_, e) => order.Add("event");

			collectionView.SendItemTapped("b", null, -1, 1);

			Assert.Equal(new[] { "command", "event" }, order);
			Assert.Equal("parameter", commandParameter);
		}

		[Fact]
		public void CommandReceivesTheTappedItemWhenNoParameterIsSet()
		{
			object commandParameter = "unset";
			var collectionView = new CollectionView
			{
				ItemsSource = new[] { "a", "b" },
				ItemTappedCommand = new Command(p => commandParameter = p),
			};

			collectionView.SendItemTapped("b", null, -1, 1);
			Assert.Equal("b", commandParameter);

			collectionView.ItemTappedCommandParameter = null;
			collectionView.SendItemTapped("b", null, -1, 1);
			Assert.Null(commandParameter);
		}

		[Fact]
		public void CommandIsNotExecutedWhenItCannotExecute()
		{
			var executed = false;
			var tapped = false;
			var collectionView = new CollectionView
			{
				ItemsSource = new[] { "a" },
				ItemTappedCommand = new Command(() => executed = true, () => false),
			};
			collectionView.ItemTapped += (_, e) => tapped = true;

			collectionView.SendItemTapped("a", null, -1, 0);

			Assert.False(executed);
			Assert.True(tapped);
		}

		[Fact]
		public void HandlerIsUpdatedOnlyWhenItemTapEnabledChanges()
		{
			var collectionView = new CollectionView();
			var handler = new UpdateRecordingHandler();
			collectionView.Handler = handler;
			handler.Updates.Clear();

			Assert.False(collectionView.IsItemTapEnabled);

			void First(object sender, ItemTappedEventArgs e)
			{ }
			void Second(object sender, ItemTappedEventArgs e)
			{ }

			collectionView.ItemTapped += First;
			Assert.True(collectionView.IsItemTapEnabled);
			collectionView.ItemTapped += Second;
			collectionView.ItemTappedCommand = new Command(() => { });
			collectionView.ItemTapped -= First;
			collectionView.ItemTapped -= Second;
			Assert.True(collectionView.IsItemTapEnabled);
			collectionView.ItemTappedCommand = null;
			Assert.False(collectionView.IsItemTapEnabled);

			Assert.Equal(2, handler.Updates.Count(u => u == SelectableItemsView.IsItemTapEnabledMapperKey));
			Assert.DoesNotContain(nameof(SelectableItemsView.SelectionMode), handler.Updates);
		}

		[Fact]
		public void TapCapturedBeforeTheSourceChangesKeepsItsPosition()
		{
			var groups = CreateGroups();
			var collectionView = new CollectionView { IsGrouped = true, ItemsSource = groups };
			ItemTappedEventArgs tap = null;
			collectionView.ItemTapped += (_, e) => tap = e;

			var captured = collectionView.CreateItemTappedEventArgs("d", groups[1], 1, 1);
			collectionView.ItemsSource = new List<List<string>>();
			collectionView.SendItemTapped(captured);

			Assert.Equal("d", tap.Item);
			Assert.Same(groups[1], tap.Group);
			Assert.Equal(5, tap.ItemIndex);
		}

		static List<List<string>> CreateGroups() => new()
		{
			new() { "a", "b" },
			new() { "c", "d", "e" },
			new() { "f" },
		};

		class UpdateRecordingHandler : ViewHandler<CollectionView, object>
		{
			public UpdateRecordingHandler() : base(new PropertyMapper<IView>())
			{
			}

			public List<string> Updates { get; } = new();

			protected override object CreatePlatformView() => new object();

			public override void UpdateValue(string property)
			{
				Updates.Add(property);
				base.UpdateValue(property);
			}
		}
	}
}
