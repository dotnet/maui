using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Layouts;

// This is a temporary namespace until we rename everything and move the legacy layouts
namespace Microsoft.Maui.Controls.Layout2
{
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
			if (IsMeasureValid)
			{
				return DesiredSize;
			}

			var sizeWithoutMargins = LayoutManager.Measure(widthConstraint, heightConstraint);
			DesiredSize = new Size(sizeWithoutMargins.Width + Margin.HorizontalThickness,
				sizeWithoutMargins.Height + Margin.VerticalThickness);

			IsMeasureValid = true;
			return DesiredSize;
		}

		protected override void ArrangeOverride(Rectangle bounds)
		{
			if (!IsMeasureValid)
			{
				return;
			}

			if (IsArrangeValid)
			{
				return;
			}

			Arrange(bounds);

			LayoutManager.ArrangeChildren(Frame);
			IsArrangeValid = true;
			Handler?.SetFrame(Frame);
		}

		protected override void InvalidateMeasureOverride()
		{
			base.InvalidateMeasure();

			foreach (var child in Children)
			{
				child.InvalidateArrange();
			}
		}

		public void Add(IView child)
		{
			if (child == null)
				return;

			_children.Add(child);

			// TODO MAUI
			if (child is Element ve)
				ve.Parent = this;

			InvalidateMeasure();

			LayoutHandler?.Add(child);
		}

		public void Remove(IView child)
		{
			if (child == null)
				return;

			_children.Remove(child);

			// TODO MAUI
			if (child is Element ve)
				ve.Parent = null;

			InvalidateMeasure();

			LayoutHandler?.Remove(child);
		}
	}
}
