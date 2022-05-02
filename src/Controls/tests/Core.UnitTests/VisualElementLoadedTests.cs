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

			Assert.AreEqual(1, loaded);
			Assert.AreEqual(0, unloaded);

			window.Page = null;

			Assert.AreEqual(1, loaded);
			Assert.AreEqual(1, unloaded);
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

			Assert.AreEqual(1, loaded);
			Assert.AreEqual(1, unloaded);

			parentLayout.Add(layout);

			Assert.AreEqual(2, loaded);
			Assert.AreEqual(1, unloaded);

			window.Page = null;

			Assert.AreEqual(2, loaded);
			Assert.AreEqual(2, unloaded);
		}
	}
}
