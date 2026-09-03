using System;
using System.Collections;
using System.Linq;
using Microsoft.Maui.Graphics;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

public class ViewUnitTests : BaseTestFixture
{

	[Fact]
	public void BindingsApplyAfterViewAddedToParentWithContextSet()
	{
		var parent = new NaiveLayout();
		parent.BindingContext = new MockViewModel { Text = "test" };

		var child = new Entry();
		child.SetBinding(Entry.TextProperty, new Binding("Text"));

		parent.Children.Add(child);

		Assert.Same(child.BindingContext, parent.BindingContext);
		Assert.Equal("test", child.Text);
	}

	[Fact]
	public void TestBindingContextChaining()
	{
		View child;
		var group = new NaiveLayout
		{
			Children = { (child = new View()) }
		};

		var context = new object();
		group.BindingContext = context;

		Assert.Equal(context, child.BindingContext);
	}

	[Fact]
	public void TestAncestorRemoved()
	{
		var ancestor = new View();
		var child = new NaiveLayout { Children = { ancestor } };
		var view = new NaiveLayout { Children = { child } };

		bool removed = false;
		view.DescendantRemoved += (sender, arg) => removed = true;

		child.Children.Remove(ancestor);
		Assert.True(removed, "AncestorRemoved must fire when removing a child from an ancestor of a view.");
	}

	[Fact]
	public void TestDoubleSetParent()
	{
		var view = new ParentSignalView();
		var parent = new NaiveLayout { Children = { view } };

		view.ParentSet = false;
		view.Parent = parent;

		Assert.False(view.ParentSet, "OnParentSet should not be called in the event the parent is already properly set");
	}

	[Fact]
	public void TestAncestorAdded()
	{
		var child = new NaiveLayout();
		var view = new NaiveLayout { Children = { child } };

		bool added = false;
		view.DescendantAdded += (sender, arg) => added = true;

		child.Children.Add(new View());

		Assert.True(added, "AncestorAdded must fire when adding a child to an ancestor of a view.");
	}

	class ParentSignalView : View
	{
		public bool ParentSet { get; set; }

		protected override void OnParentSet()
		{
			ParentSet = true;
			base.OnParentSet();
		}
	}
}