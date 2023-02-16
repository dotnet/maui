using System.Threading.Tasks;
using Xunit;
namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class VisualElementLoadedTests : BaseTestFixture
	{
		[Fact]
		public async Task LoadedAndUnloadedFire()
		{
			var editor = new Editor();

			int unloaded = 0;
			int loaded = 0;
			editor.Loaded += (_, __) => loaded++;
			editor.Unloaded += (_, __) => unloaded++;

			var window = new Window()
			{
				Page = new ContentPage()
				{
					Content = editor
				}
			};

			Assert.Equal(1, loaded);
			Assert.Equal(0, unloaded);

			window.Page = null;

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

			var window = new Window()
			{
				Page = new ContentPage()
				{
					Content = parentLayout
				}
			};

			parentLayout.Remove(layout);

			Assert.Equal(1, loaded);
			Assert.Equal(1, unloaded);

			parentLayout.Add(layout);

			Assert.Equal(2, loaded);
			Assert.Equal(1, unloaded);

			window.Page = null;

			Assert.Equal(2, loaded);
			Assert.Equal(2, unloaded);
		}
	}
}
