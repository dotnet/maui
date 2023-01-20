using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Picker)]
	public partial class PickerTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ItemsUpdateWithCollectionChanges()
		{
			var items = new ObservableCollection<string>()
			{
				"1",
				"2"
			};

			Picker picker = new Picker()
			{
				ItemsSource = items,
				SelectedIndex = 0
			};

			var handler = await CreateHandlerAsync<PickerHandler>(picker);

			Assert.Equal("1", await GetPlatformControlText(handler.PlatformView));
			await InvokeOnMainThreadAsync(() => items.Remove("1"));
			Assert.Equal("2", await GetPlatformControlText(handler.PlatformView));
		}

		[Fact]
		public async Task ItemsUpdateWithNewItemSource()
		{
			var newItems = new List<string>()
			{
				"1"
			};

			Picker picker = new Picker()
			{
				ItemsSource = new List<string> { "2" },
				SelectedIndex = 0
			};

			var handler = await CreateHandlerAsync<PickerHandler>(picker);
			Assert.Equal("2", await GetPlatformControlText(handler.PlatformView));
			await InvokeOnMainThreadAsync(() => picker.ItemsSource = newItems);
			Assert.NotEqual("2", await GetPlatformControlText(handler.PlatformView));
		}
	}
}