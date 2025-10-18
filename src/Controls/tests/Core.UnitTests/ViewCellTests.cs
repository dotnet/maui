using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ViewCellTests : BaseTestFixture
	{
		[Fact]
		public void SetParentBeforeView()
		{
			var parent = new View();
			var child = new View();
			var viewCell = new ViewCell();

			Assert.Null(viewCell.View);
			viewCell.Parent = parent;

			viewCell.View = child;
			Assert.Same(parent, viewCell.Parent);
			Assert.Same(viewCell, child.Parent);
		}

		[Fact]
		//issue 550
		public void SetBindingContextBeforeParent()
		{
			var parent = new View
			{
				BindingContext = new object(),
			};

			var itemcontext = new object();
			var cell = new ViewCell { View = new Label() };
			cell.BindingContext = itemcontext;
			cell.Parent = parent;

			Assert.Same(itemcontext, cell.View.BindingContext);
		}

		[Fact]
		public void SetBindingContextBeforeView()
		{
			var context = new object();
			var view = new View();
			var cell = new ViewCell();
			cell.BindingContext = context;
			cell.View = view;
			Assert.Same(context, view.BindingContext);
		}

		[Fact]
		public void SetViewBeforeBindingContext()
		{
			var context = new object();
			var view = new View();
			var cell = new ViewCell();
			cell.View = view;
			cell.BindingContext = context;
			Assert.Same(context, view.BindingContext);
		}
	}
}
