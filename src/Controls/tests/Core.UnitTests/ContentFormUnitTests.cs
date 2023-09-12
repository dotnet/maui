using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ContentPageUnitTests : BaseTestFixture
	{
		[Fact]
		public void PropagateBindingContextBefore()
		{
			var stack = new StackLayout();

			var content = new ContentPage();
			content.Content = stack;

			object context = new object();
			content.BindingContext = context;

			Assert.Same(context, stack.BindingContext);
		}

		[Fact]
		public void PropagateBindingContextAfter()
		{
			var stack = new StackLayout();

			var content = new ContentPage();

			object context = new object();
			content.BindingContext = context;

			content.Content = stack;

			Assert.Same(context, stack.BindingContext);
		}

		[Fact]
		public void PropagateToolbarItemBindingContextPreAdd()
		{
			var page = new ContentPage();
			object context = "hello";

			var toolbarItem = new ToolbarItem();
			page.ToolbarItems.Add(toolbarItem);

			page.BindingContext = context;

			Assert.Equal(context, toolbarItem.BindingContext);
		}

		[Fact]
		public void PropagateToolbarItemBindingContextPostAdd()
		{
			var page = new ContentPage();
			object context = "hello";

			var toolbarItem = new ToolbarItem();
			page.BindingContext = context;

			page.ToolbarItems.Add(toolbarItem);

			Assert.Equal(context, toolbarItem.BindingContext);
		}

		[Fact]
		public void ContentPage_should_have_the_InternalChildren_correctly_when_Content_changed()
		{
			var sut = new ContentPage();
			IList<Element> internalChildren = ((IControlTemplated)sut).InternalChildren;
			internalChildren.Add(new VisualElement());
			internalChildren.Add(new VisualElement());
			internalChildren.Add(new VisualElement());

			var expected = new View();
			sut.Content = expected;

			Assert.Single(internalChildren);
			Assert.Same(expected, internalChildren[0]);
		}
	}
}
