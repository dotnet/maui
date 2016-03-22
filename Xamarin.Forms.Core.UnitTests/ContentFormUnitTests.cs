using System;
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

			Assert.AreSame (context, stack.BindingContext);
		}

		[Test]
		public void PropagateBindingContextAfter()
		{
			var stack = new StackLayout();

			var content = new ContentPage();

			object context = new object();
			content.BindingContext = context;

			content.Content = stack;

			Assert.AreSame (context, stack.BindingContext);
		}

		[Test]
		public void PropagateToolbarItemBindingContextPreAdd ()
		{
			var page = new ContentPage ();
			object context = "hello";

			var toolbarItem = new ToolbarItem ();
			page.ToolbarItems.Add (toolbarItem);

			page.BindingContext = context;

			Assert.AreEqual (context, toolbarItem.BindingContext);
		}

		[Test]
		public void PropagateToolbarItemBindingContextPostAdd ()
		{
			var page = new ContentPage ();
			object context = "hello";

			var toolbarItem = new ToolbarItem ();
			page.BindingContext = context;

			page.ToolbarItems.Add (toolbarItem);

			Assert.AreEqual (context, toolbarItem.BindingContext);
		}
	}
}
