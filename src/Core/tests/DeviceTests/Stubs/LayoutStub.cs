using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class LayoutStub : StubBase, ILayout
	{
		ILayoutManager _layoutManager;

		public ILayoutHandler LayoutHandler => Handler as ILayoutHandler;

		public void Add(IView child)
		{
			Children.Add(child);
		}

		public bool Remove(IView child)
		{
			return Children.Remove(child);
		}

		public int IndexOf(IView item)
		{
			return Children.IndexOf(item);
		}

		public void Insert(int index, IView item)
		{
			Children.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			Children.RemoveAt(index);
		}

		public void Clear()
		{
			Children.Clear();
		}

		public bool Contains(IView item)
		{
			return Children.Contains(item);
		}

		public void CopyTo(IView[] array, int arrayIndex)
		{
			Children.CopyTo(array, arrayIndex);
		}

		public IEnumerator<IView> GetEnumerator()
		{
			return Children.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Children.GetEnumerator();
		}

		public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return LayoutManager.Measure(widthConstraint, heightConstraint);
		}

		public Size CrossPlatformArrange(Rect bounds)
		{
			return LayoutManager.ArrangeChildren(bounds);
		}

		public Thickness Padding { get; set; }
		public int Count => Children.Count;
		public bool IsReadOnly => Children.IsReadOnly;

		ILayoutManager LayoutManager => _layoutManager ??= CreateLayoutManager();

		protected virtual ILayoutManager CreateLayoutManager() => new LayoutManagerStub();

		public bool IgnoreSafeArea => false;

		public bool ClipsToBounds { get; set; }

		public IView this[int index] { get => Children[index]; set => Children[index] = value; }
	}
}