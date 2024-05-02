using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Primitives;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.UnitTests.Layouts
{
	[Category(TestCategory.Core, TestCategory.Layout)]
	public class ZIndexTests
	{
		// These tests need a real collection to work with; we can't reasonably use a substitute here
		class FakeLayout : ILayout, IList<IView>
		{
			public bool ClipsToBounds { get; set; }


			#region IView stuff

			public string AutomationId { get; }
			public FlowDirection FlowDirection { get; }
			public LayoutAlignment HorizontalLayoutAlignment { get; }
			public LayoutAlignment VerticalLayoutAlignment { get; }
			public Semantics Semantics { get; }
			public IShape Clip { get; }
			public IShadow Shadow { get; }
			public bool IsEnabled { get; }
			public bool IsFocused { get; set; }
			public Visibility Visibility { get; }
			public double Opacity { get; }
			public Paint Background { get; }
			public Rect Frame { get; set; }
			public double Width { get; }
			public double MinimumWidth { get; }
			public double MaximumWidth { get; }
			public double Height { get; }
			public double MinimumHeight { get; }
			public double MaximumHeight { get; }
			public Thickness Margin { get; }
			public IViewHandler Handler { get; set; }
			public Size DesiredSize { get; }
			public int ZIndex { get; }
			public IElement Parent { get; }
			public double TranslationX { get; }
			public double TranslationY { get; }
			public double Scale { get; }
			public double ScaleX { get; }
			public double ScaleY { get; }
			public double Rotation { get; }
			public double RotationX { get; }
			public double RotationY { get; }
			public double AnchorX { get; }
			public double AnchorY { get; }
			public bool IgnoreSafeArea { get; }
			public Thickness Padding { get; }
			public bool InputTransparent { get; set; }

			IElementHandler IElement.Handler { get; set; }

			public void InvalidateArrange()
			{
				throw new System.NotImplementedException();
			}

			public void InvalidateMeasure()
			{
				throw new System.NotImplementedException();
			}

			public Size Measure(double widthConstraint, double heightConstraint)
			{
				throw new System.NotImplementedException();
			}

			public bool Focus() => false;

			public void Unfocus()
			{
			}

			#endregion

			#region IList stuff

			IList<IView> _views = new List<IView>();

			public int Count => _views.Count;

			public bool IsReadOnly => _views.IsReadOnly;

			public IView this[int index] { get => _views[index]; set => _views[index] = value; }

			public void Add(IView item)
			{
				_views.Add(item);
			}

			public Size Arrange(Rect bounds)
			{
				throw new System.NotImplementedException();
			}

			public void Clear()
			{
				_views.Clear();
			}

			public bool Contains(IView item)
			{
				return _views.Contains(item);
			}

			public void CopyTo(IView[] array, int arrayIndex)
			{
				_views.CopyTo(array, arrayIndex);
			}

			public Size CrossPlatformArrange(Rect bounds)
			{
				throw new System.NotImplementedException();
			}

			public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
			{
				throw new System.NotImplementedException();
			}

			public IEnumerator<IView> GetEnumerator()
			{
				return _views.GetEnumerator();
			}

			public int IndexOf(IView item)
			{
				return _views.IndexOf(item);
			}

			public void Insert(int index, IView item)
			{
				_views.Insert(index, item);
			}

			public bool Remove(IView item)
			{
				return _views.Remove(item);
			}

			public void RemoveAt(int index)
			{
				_views.RemoveAt(index);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable)_views).GetEnumerator();
			}

			#endregion
		}

		static IView CreateTestView(int zIndex = 0)
		{
			var view = Substitute.For<IView>();
			view.ZIndex.Returns(zIndex);
			return view;
		}

		[Fact]
		public void LayoutHandlerIndexFollowsZOrder()
		{
			var layout = new FakeLayout();
			var view0 = CreateTestView(zIndex: 10);
			var view1 = CreateTestView(zIndex: 0);
			layout.Add(view0);
			layout.Add(view1);

			Assert.Equal(0, layout.GetLayoutHandlerIndex(view1));
			Assert.Equal(1, layout.GetLayoutHandlerIndex(view0));
		}

		[Fact]
		public void LayoutHandlerIndexFollowsAddOrderWhenZIndexesAreEqual()
		{
			var layout = new FakeLayout();
			var view0 = CreateTestView(zIndex: 0);
			var view1 = CreateTestView(zIndex: 10);
			var view2 = CreateTestView(zIndex: 10);
			var view3 = CreateTestView(zIndex: 100);

			layout.Add(view0);
			layout.Add(view1);
			layout.Add(view2);
			layout.Add(view3);

			Assert.Equal(0, layout.GetLayoutHandlerIndex(view0));
			Assert.Equal(1, layout.GetLayoutHandlerIndex(view1));
			Assert.Equal(2, layout.GetLayoutHandlerIndex(view2));
			Assert.Equal(3, layout.GetLayoutHandlerIndex(view3));
		}

		[Fact]
		public void LayoutHandlerIndexIsNegativeWhenChildIsNotFound()
		{
			var layout = new FakeLayout();
			var view0 = CreateTestView(zIndex: 0);

			Assert.Equal(-1, layout.GetLayoutHandlerIndex(view0));

			layout.Add(CreateTestView(zIndex: 0));
			Assert.Equal(-1, layout.GetLayoutHandlerIndex(view0));


			layout.Add(CreateTestView(zIndex: 0));
			Assert.Equal(-1, layout.GetLayoutHandlerIndex(view0));
		}

		[Fact]
		public void LayoutHandlerIndexPreservesAddOrderForEqualZIndexes()
		{
			var layout = new FakeLayout();
			var view0 = CreateTestView(zIndex: 10);
			var view1 = CreateTestView(zIndex: 10);
			var view2 = CreateTestView(zIndex: 10);
			var view3 = CreateTestView(zIndex: 5);
			layout.Add(view0);
			layout.Add(view1);
			layout.Add(view2);
			layout.Add(view3);

			Assert.Equal(1, layout.GetLayoutHandlerIndex(view0));
			Assert.Equal(2, layout.GetLayoutHandlerIndex(view1));
			Assert.Equal(3, layout.GetLayoutHandlerIndex(view2));
			Assert.Equal(0, layout.GetLayoutHandlerIndex(view3));
		}

		[Fact]
		public void ItemsOrderByZIndex()
		{
			var layout = new FakeLayout();
			var view0 = CreateTestView(zIndex: 10);
			var view1 = CreateTestView(zIndex: 0);

			layout.Add(view0);
			layout.Add(view1);

			var zordered = layout.OrderByZIndex().ToArray();
			Assert.Equal(view1, zordered[0]);
			Assert.Equal(view0, zordered[1]);
		}

		[Fact]
		public void ZIndexUpdatePreservesAddOrderForEqualZIndexes()
		{
			var layout = new FakeLayout();
			var view0 = CreateTestView(zIndex: 0);
			var view1 = CreateTestView(zIndex: 5);
			var view2 = CreateTestView(zIndex: 5);
			var view3 = CreateTestView(zIndex: 10);

			layout.Add(view0);
			layout.Add(view1);
			layout.Add(view2);
			layout.Add(view3);

			var zordered = layout.OrderByZIndex().ToArray();
			Assert.Equal(view0, zordered[0]);
			Assert.Equal(view1, zordered[1]);
			Assert.Equal(view2, zordered[2]);
			Assert.Equal(view3, zordered[3]);

			// Fake an update
			view3.ZIndex.Returns(5);

			zordered = layout.OrderByZIndex().ToArray();
			Assert.Equal(view0, zordered[0]);
			Assert.Equal(view1, zordered[1]);
			Assert.Equal(view2, zordered[2]);
			Assert.Equal(view3, zordered[3]);
		}

		[Fact]
		public void ZIndexUpdatePreservesAddOrderLotsOfEqualZIndexes()
		{
			// This tests the same thing as ZIndexUpdatePreservesAddOrderForEqualZIndexes,
			// but for more views - since the sorting algorithm can change when the arrays 
			// are larger, we were running into situations where layouts with more controls
			// were _not_ preserving the Add() order when sorting by z-index.

			var layout = new FakeLayout();

			int views = 100;
			int iterations = 10;

			var toAdd = new IView[views];

			for (int n = 0; n < views; n++)
			{
				toAdd[n] = CreateTestView(zIndex: 0);
				layout.Add(toAdd[n]);
			}

			for (int i = 0; i < iterations; i++)
			{
				var zordered = layout.OrderByZIndex().ToArray();

				for (int n = 0; n < zordered.Length; n++)
				{
					Assert.Equal(toAdd[n], zordered[n]);
				}
			}
		}
	}
}
