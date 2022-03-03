using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		public async Task Basic()
		{
			var editor = new Editor();

			int unloaded = 0;
			int loaded = 0;
			editor.Loaded += (_, __) => loaded++;
			editor.Unloaded += (_, __) => unloaded++;

			await CreateHandlerAndAddToWindow<EditorHandler>(editor, (handler) =>
			{
				Assert.True(loaded == 1);
				Assert.True(unloaded == 0);
			});

			Assert.True(loaded == 1);
			Assert.True(unloaded == 1);
		}

		[Fact]
		public async Task ParentRemovedAndAdded()
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

				Assert.True(loaded == 1);
				Assert.True(unloaded == 1);

				parentLayout.Add(layout);
				await OnLoadedAsync(layout);
				await OnLoadedAsync(editor);

				Assert.True(loaded == 2);
				Assert.True(unloaded == 1);

			});

			Assert.True(loaded == 2);
			Assert.True(unloaded == 2);
		}
	}
}
