using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

// This is a temporary namespace until we rename everything and move the legacy layouts
namespace Microsoft.Maui.Controls.Layout2
{
	[ContentProperty(nameof(Children))]
	public abstract class Layout : View, Microsoft.Maui.ILayout, IList<IView>, IPaddingElement
	{
		ILayoutManager _layoutManager;
		ILayoutManager LayoutManager => _layoutManager ??= CreateLayoutManager();

		// The actual backing store for the IViews in the ILayout
		readonly List<IView> _children = new();

		// This provides a Children property for XAML 
		public IList<IView> Children => this;

		public ILayoutHandler LayoutHandler => Handler as ILayoutHandler;

		public int Count => _children.Count;

		public bool IsReadOnly => ((ICollection<IView>)_children).IsReadOnly;

		public IView this[int index] { get => _children[index]; set => _children[index] = value; }

		public Thickness Padding
		{
			get => (Thickness)GetValue(PaddingElement.PaddingProperty);
			set => SetValue(PaddingElement.PaddingProperty, value);
		}

		protected abstract ILayoutManager CreateLayoutManager();

		public IEnumerator<IView> GetEnumerator() => _children.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _children.GetEnumerator();

#pragma warning disable CS0672 // Member overrides obsolete member
		public override SizeRequest GetSizeRequest(double widthConstraint, double heightConstraint)
#pragma warning restore CS0672 // Member overrides obsolete member
		{
			var size = (this as IFrameworkElement).Measure(widthConstraint, heightConstraint);
			return new SizeRequest(size);
		}

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			var margin = (this as IView)?.Margin ?? Thickness.Zero;
			// Adjust the constraints to account for the margins
			widthConstraint -= margin.HorizontalThickness;
			heightConstraint -= margin.VerticalThickness;
			var sizeWithoutMargins = LayoutManager.Measure(widthConstraint, heightConstraint);
			DesiredSize = new Size(sizeWithoutMargins.Width + Margin.HorizontalThickness,
				sizeWithoutMargins.Height + Margin.VerticalThickness);
			return DesiredSize;
		}

		protected override Size ArrangeOverride(Rectangle bounds)
		{
			base.ArrangeOverride(bounds);
			Frame = bounds;
			LayoutManager.ArrangeChildren(Frame);
			foreach (var child in Children)
			{
				child.Handler?.NativeArrange(child.Frame);
			}
			return Frame.Size;
		}

		protected override void InvalidateMeasureOverride()
		{
			base.InvalidateMeasureOverride();
			foreach (var child in Children)
			{
				child.InvalidateMeasure();
			}
		}

		public virtual void Add(IView child)
		{
			if (child == null)
				return;
			_children.Add(child);
			if (child is Element element)
				element.Parent = this;
			InvalidateMeasure();
			LayoutHandler?.Add(child);
		}

		public void Clear()
		{
			for (int n = _children.Count - 1; n >= 0; n--)
			{
				Remove(this[n]);
			}
		}

		public bool Contains(IView item)
		{
			return _children.Contains(item);
		}

		public void CopyTo(IView[] array, int arrayIndex)
		{
			_children.CopyTo(array, arrayIndex);
		}

		public int IndexOf(IView item)
		{
			return _children.IndexOf(item);
		}

		public void Insert(int index, IView child)
		{
			if (child == null)
				return;

			_children.Insert(index, child);

			if (child is Element element)
				element.Parent = this;

			InvalidateMeasure();

			LayoutHandler?.Add(child);
		}

		public virtual bool Remove(IView child)
		{
			if (child == null)
				return false;

			var result = _children.Remove(child);

			if (child is Element element)
				element.Parent = null;

			InvalidateMeasure();

			LayoutHandler?.Remove(child);

			return result;
		}

		public void RemoveAt(int index)
		{
			if (index >= Count)
			{
				return;
			}

			var child = _children[index];

			_children.RemoveAt(index);

			if (child is Element element)
				element.Parent = null;

			InvalidateMeasure();

			LayoutHandler?.Remove(child);
		}

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue)
		{
			InvalidateMeasure();
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator()
		{
			return new Thickness(0);
		}
	}
}
