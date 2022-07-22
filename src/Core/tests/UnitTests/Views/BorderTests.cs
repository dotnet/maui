using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.UnitTests.Views
{
	[Category(TestCategory.Core, TestCategory.View)]
	public class BorderTests
	{
		[Fact]
		public void TestDefaultBorderStrokeShape()
		{
			Border border = new Border();

			Assert.NotNull(border);
			Assert.Null(border.StrokeShape);
		}

		[Fact]
		public void TestBorderPropagateBindingContext()
		{
			Border border = new Border();

			var bindingContext = new object();
			border.BindingContext = bindingContext;

			var content = new ContentView();
			border.Content = content;

			Assert.True(border.BindingContext == bindingContext);
		}

		[Fact]
		public void TestSetChild()
		{
			Frame frame = new Frame();

			var child1 = new Label();

			bool added = false;

			frame.ChildAdded += (sender, e) => added = true;

			frame.Content = child1;

			Assert.True(added);
			Assert.Equal(child1, frame.Content);

			added = false;
			frame.Content = child1;

			Assert.False(added);
		}

		[Fact]
		public void TestReplaceChild()
		{
			Frame frame = new Frame();

			var child1 = new Label();
			var child2 = new Label();

			frame.Content = child1;

			bool removed = false;
			bool added = false;

			frame.ChildRemoved += (sender, e) => removed = true;
			frame.ChildAdded += (sender, e) => added = true;

			frame.Content = child2;

			Assert.True(removed);
			Assert.True(added);
			Assert.Equal(child2, frame.Content);
		}
	}
}