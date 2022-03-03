using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.VisualElement)]
	public partial class VisualElementTests : HandlerTestBase
	{
		[Fact]
		public async Task CanCreateHandler()
		{
			var image = new Image();

			await CreateHandlerAsync<ImageHandler>(image);
		}

		[Fact]
		public async Task SettingHandlerDoesNotThrow()
		{
			var image = new Image();

			var handler = await CreateHandlerAsync<ImageHandler>(image);

			image.Handler = handler;
		}

		[Fact]
		public async Task LoadedAndUnloadedFire()
		{
			var editor = new Editor();

			int unloaded = 0;
			int loaded = 0;
			editor.Loaded += (_, __) => loaded++;
			editor.Unloaded += (_, __) => unloaded++;

			await CreateHandlerAndAddToWindow<EditorHandler>(editor, (handler) =>
			{
				Assert.Equal(1, loaded);
				Assert.Equal(0, unloaded);
			});

			Assert.Equal(1, loaded);
			Assert.Equal(1, unloaded);
		}

		[Fact]
		public async Task LoadedAndUnloadedFireWhenParentRemoved()
		{
			var editor = new Editor();
			var layout = new VerticalStackLayout()
			{
				editor
			};

			var parentLayout = new VerticalStackLayout()
			{
				layout
			};

			int unloaded = 0;
			int loaded = 0;
			editor.Loaded += (_, __) => loaded++;
			editor.Unloaded += (_, __) => unloaded++;

			await CreateHandlerAndAddToWindow<LayoutHandler>(parentLayout, async (handler) =>
			{
				parentLayout.Remove(layout);
				await OnUnloadedAsync(layout);
				await OnUnloadedAsync(editor);

				Assert.Equal(1, loaded);
				Assert.Equal(1, unloaded);

				parentLayout.Add(layout);
				await OnLoadedAsync(layout);
				await OnLoadedAsync(editor);

				Assert.Equal(2, loaded);
				Assert.Equal(1, unloaded);
			});

			await Task.Delay(1000);

			Assert.Equal(2, loaded);
			Assert.Equal(2, unloaded);
		}
	}
}