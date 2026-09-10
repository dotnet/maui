using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	// ItemsSource is owned by the app and routinely outlives the control bound to it -- a view model
	// collection survives the page that displayed it. A non-weak CollectionChanged subscription
	// therefore roots every control ever bound to that collection.
	public class ItemsSourceWeakEventTests : BaseTestFixture
	{
		// https://github.com/dotnet/maui/issues/37662
		[Fact]
		public async Task SharedItemsSourceDoesNotRootIndicatorView()
		{
			var shared = new ObservableCollection<string> { "a", "b" };

			var reference = CreateIndicatorView(shared);

			Assert.False(await reference.WaitForCollect(), "IndicatorView should not be alive!");
			GC.KeepAlive(shared);
		}

		// https://github.com/dotnet/maui/issues/37636
		[Fact]
		public async Task SharedItemsSourceDoesNotRootPicker()
		{
			var shared = new ObservableCollection<string> { "a", "b" };

			var reference = CreatePicker(shared);

			Assert.False(await reference.WaitForCollect(), "Picker should not be alive!");
			GC.KeepAlive(shared);
		}

		[Fact]
		public void IndicatorViewStillTracksItemsSourceChanges()
		{
			var items = new ObservableCollection<string> { "a", "b" };
			var indicatorView = new IndicatorView { ItemsSource = items };

			Assert.Equal(2, indicatorView.Count);

			items.Add("c");

			Assert.Equal(3, indicatorView.Count);
			GC.KeepAlive(indicatorView);
		}

		[Fact]
		public void PickerStillTracksItemsSourceChanges()
		{
			var items = new ObservableCollection<string> { "a", "b" };
			var picker = new Picker { ItemsSource = items };

			Assert.Equal(2, picker.Items.Count);

			items.Add("c");

			Assert.Equal(3, picker.Items.Count);
			Assert.Equal("c", picker.Items[2]);
			GC.KeepAlive(picker);
		}

		[Fact]
		public void PickerStillTracksItemsSourceRemoval()
		{
			var items = new ObservableCollection<string> { "a", "b", "c" };
			var picker = new Picker { ItemsSource = items };

			items.Remove("b");

			Assert.Equal(2, picker.Items.Count);
			Assert.Equal("c", picker.Items[1]);
			GC.KeepAlive(picker);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateIndicatorView(ObservableCollection<string> itemsSource) =>
			new WeakReference(new IndicatorView { ItemsSource = itemsSource });

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreatePicker(ObservableCollection<string> itemsSource) =>
			new WeakReference(new Picker { ItemsSource = itemsSource });
	}
}
