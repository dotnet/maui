using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

// This is a temporary namespace until we rename everything and move the legacy layouts
namespace Microsoft.Maui.Controls.Layout2
{
	[ContentProperty(nameof(Children))]
	public abstract class Layout : View, Microsoft.Maui.ILayout, IEnumerable<IView>
	{
		ILayoutManager _layoutManager;
		ILayoutManager LayoutManager => _layoutManager ??= CreateLayoutManager();

		readonly List<IView> _children = new List<IView>();

		public IReadOnlyList<IView> Children { get => _children.AsReadOnly(); }

		public ILayoutHandler LayoutHandler => Handler as ILayoutHandler;

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

		public virtual void Remove(IView child)
		{
			if (child == null)
				return;

			_children.Remove(child);

			if (child is Element element)
				element.Parent = null;

			InvalidateMeasure();

			LayoutHandler?.Remove(child);
		}
	}
}
