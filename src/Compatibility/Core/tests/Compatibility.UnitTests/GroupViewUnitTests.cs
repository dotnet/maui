using System;
using System.Collections;
using System.Linq;
using Microsoft.Maui.Graphics;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
	internal class NaiveLayout : Compatibility.Layout<View>
	{
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			foreach (var child in ((IElementController)this).LogicalChildren.Cast<View>())
			{
				var result = new Rect(x, y, 0, 0);
				var request = child.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);
				result.Width = request.Request.Width;
				result.Height = request.Request.Height;

				child.Layout(result);
			}
		}
	}


	public class LayoutUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestRaiseChild()
		{
			var view = new NaiveLayout();

			var child1 = new View();
			var child2 = new View();
			var child3 = new View();

			view.Children.Add(child1);
			view.Children.Add(child2);
			view.Children.Add(child3);

			bool reordered = false;
			view.ChildrenReordered += (sender, args) => reordered = true;

			view.RaiseChild(child1);

			Assert.Equal(child1, ((IElementController)view).LogicalChildren[2]);
			Assert.True(reordered);

			view.RaiseChild(child2);
			Assert.Equal(child2, ((IElementController)view).LogicalChildren[2]);
		}

		[Fact]
		public void TestRaiseUnownedChild()
		{
			var view = new NaiveLayout();

			var child1 = new View();
			var child2 = new View();
			var child3 = new View();

			view.Children.Add(child1);
			view.Children.Add(child3);

			bool reordered = false;
			view.ChildrenReordered += (sender, args) => reordered = true;

			view.RaiseChild(child2);

			Assert.False(reordered);
		}

		[Fact]
		public void TestLowerChild()
		{
			var view = new NaiveLayout();

			var child1 = new View();
			var child2 = new View();
			var child3 = new View();

			view.Children.Add(child1);
			view.Children.Add(child2);
			view.Children.Add(child3);

			bool reordered = false;
			view.ChildrenReordered += (sender, args) => reordered = true;

			view.LowerChild(child3);

			Assert.Equal(child3, ((IElementController)view).LogicalChildren[0]);
			Assert.True(reordered);

			view.LowerChild(child2);
			Assert.Equal(child2, ((IElementController)view).LogicalChildren[0]);
		}

		[Fact]
		public void TestLowerUnownedChild()
		{
			var view = new NaiveLayout();

			var child1 = new View();
			var child2 = new View();
			var child3 = new View();

			view.Children.Add(child1);
			view.Children.Add(child3);

			bool reordered = false;
			view.ChildrenReordered += (sender, args) => reordered = true;

			view.LowerChild(child2);

			Assert.False(reordered);
		}

		[Fact]
		public void TestAdd()
		{
			var view = new NaiveLayout();
			var child1 = new View();

			bool added = false;
			view.ChildAdded += (sender, args) => added = true;

			view.Children.Add(child1);

			Assert.True(added);
			Assert.Equal(child1, ((IElementController)view).LogicalChildren[0]);
		}

		[Fact]
		public void TestDoubleAdd()
		{
			var view = new NaiveLayout();
			var child1 = new View();
			view.Children.Add(child1);

			bool added = false;
			view.ChildAdded += (sender, args) => added = true;

			view.Children.Add(child1);

			Assert.False(added);
			Assert.Equal(child1, ((IElementController)view).LogicalChildren[0]);
		}

		[Fact]
		public void TestRemove()
		{
			var view = new NaiveLayout();
			var child1 = new View();

			view.Children.Add(child1);

			bool removed = false;
			view.ChildRemoved += (sender, args) => removed = true;

			view.Children.Remove(child1);

			Assert.True(removed);
			Assert.False(((IElementController)view).LogicalChildren.Any());
		}

		[Fact]
		public void TestGenericEnumerator()
		{
			var view = new NaiveLayout();

			var children = new[] {
				new View (),
				new View (),
				new View ()
			};

			foreach (var child in children)
				view.Children.Add(child);

			int i = 0;
			foreach (var child in ((IElementController)view).LogicalChildren)
			{
				Assert.Equal(children[i], child);
				i++;
			}
		}

		[Fact]
		public void TestEnumerator()
		{
			var view = new NaiveLayout();

			var children = new[] {
				new View (),
				new View (),
				new View ()
			};

			foreach (var child in children)
				view.Children.Add(child);

			int i = 0;
			var enumerator = (((IElementController)view).LogicalChildren as IEnumerable).GetEnumerator();
			while (enumerator.MoveNext())
			{
				Assert.Equal(children[i], enumerator.Current as View);
				i++;
			}
		}

		[Fact]
		public void TestInitializerSyntax()
		{
			View view1, view2;
			var group = new NaiveLayout
			{
				Children = {
					(view1 = new View ()),
					(view2 = new View ())
				}
			};

			Assert.Equal(2, ((IElementController)group).LogicalChildren.Count);
			Assert.Contains(view1, ((IElementController)group).LogicalChildren);
			Assert.Contains(view2, ((IElementController)group).LogicalChildren);
			Assert.Equal(view1, ((IElementController)group).LogicalChildren[0]);
		}

		[Fact]
		public void TestChildren()
		{
			View view1, view2;
			var group = new NaiveLayout
			{
				Children = {
					(view1 = new View ()),
					(view2 = new View ())
				}
			};

			Assert.Equal(2, group.Children.Count);
			Assert.True(group.Children.Contains(view1));
			Assert.True(group.Children.Contains(view2));
			Assert.Equal(view1, group.Children[0]);
		}

		[Fact]
		public void TestDefaultLayout()
		{
			View view;
			var group = new NaiveLayout
			{
				IsPlatformEnabled = true,
				Children = {
					(view = new View {
						WidthRequest = 50,
						HeightRequest = 20,
						IsPlatformEnabled = true,
					})
				}
			};

			group.Layout(new Rect(0, 0, 400, 400));

			Assert.Equal(new Rect(0, 0, 50, 20), view.Bounds);
		}

		[Fact]
		public void ThrowsInvalidOperationOnSelfAdd()
		{
			var group = new NaiveLayout();
			Assert.Throws<InvalidOperationException>(() => group.Children.Add(group));
		}

		[Fact]
		public void ReorderChildrenDoesNotRaiseChildAddedOrRemoved()
		{
			var child1 = new BoxView();
			var child2 = new BoxView();
			var layout = new NaiveLayout
			{
				Children = { child1, child2 }
			};

			var added = false;
			var removed = false;

			layout.ChildAdded += (sender, args) => added = true;
			layout.ChildRemoved += (sender, args) => removed = true;

			layout.RaiseChild(child1);

			Assert.False(added);
			Assert.False(removed);
		}

		[Fact]
		public void AddToSecondLayoutRemovesFromOriginal()
		{
			var child = new BoxView();
			var layout1 = new NaiveLayout();
			var layout2 = new NaiveLayout();

			layout1.Children.Add(child);
			layout2.Children.Add(child);

			Assert.False(layout1.Children.Contains(child));
		}
	}
}
