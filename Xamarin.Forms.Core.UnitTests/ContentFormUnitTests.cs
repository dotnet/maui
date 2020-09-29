using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;


namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ContentPageUnitTests : BaseTestFixture
	{
		[Test]
		public void PropagateBindingContextBefore()
		{
			var stack = new StackLayout();

			var content = new ContentPage();
			content.Content = stack;

			object context = new object();
			content.BindingContext = context;

			Assert.AreSame(context, stack.BindingContext);
		}

		[Test]
		public void PropagateBindingContextAfter()
		{
			var stack = new StackLayout();

			var content = new ContentPage();

			object context = new object();
			content.BindingContext = context;

			content.Content = stack;

			Assert.AreSame(context, stack.BindingContext);
		}

		[Test]
		public void PropagateToolbarItemBindingContextPreAdd()
		{
			var page = new ContentPage();
			object context = "hello";

			var toolbarItem = new ToolbarItem();
			page.ToolbarItems.Add(toolbarItem);

			page.BindingContext = context;

			Assert.AreEqual(context, toolbarItem.BindingContext);
		}

		[Test]
		public void PropagateToolbarItemBindingContextPostAdd()
		{
			var page = new ContentPage();
			object context = "hello";

			var toolbarItem = new ToolbarItem();
			page.BindingContext = context;

			page.ToolbarItems.Add(toolbarItem);

			Assert.AreEqual(context, toolbarItem.BindingContext);
		}

		[Test]
		public void ContentPage_should_have_the_InternalChildren_correctly_when_Content_changed()
		{
			var sut = new ContentPage();
			IList<Element> internalChildren = ((IControlTemplated)sut).InternalChildren;
			internalChildren.Add(new VisualElement());
			internalChildren.Add(new VisualElement());
			internalChildren.Add(new VisualElement());

			var expected = new View();
			sut.Content = expected;

			Assert.AreEqual(1, internalChildren.Count);
			Assert.AreSame(expected, internalChildren[0]);
		}
	}
}