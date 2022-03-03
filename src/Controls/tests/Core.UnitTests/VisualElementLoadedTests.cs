using System.Threading.Tasks;
using NUnit.Framework;
namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class VisualElementLoadedTests : BaseTestFixture
	{
		[Test]
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

			Assert.True(loaded == 1);
			Assert.True(unloaded == 0);

			window.Page = null;

			Assert.True(loaded == 1);
			Assert.True(unloaded == 1);
		}

		[Test]
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

			Assert.True(loaded == 1);
			Assert.True(unloaded == 1);

			parentLayout.Add(layout);

			Assert.True(loaded == 2);
			Assert.True(unloaded == 1);

			window.Page = null;

			Assert.True(loaded == 2);
			Assert.True(unloaded == 2);
		}
	}
}
