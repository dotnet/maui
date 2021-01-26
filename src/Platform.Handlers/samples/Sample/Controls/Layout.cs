using System.Collections;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Platform;
using Xamarin.Platform.Handlers;
using Xamarin.Platform.Layouts;

namespace Sample
{
	public abstract class Layout : View, Xamarin.Platform.ILayout, IEnumerable<IView>
	{
		ILayoutManager _layoutManager;
		ILayoutManager LayoutManager => _layoutManager ??= CreateLayoutManager();

		readonly List<IView> _children = new List<IView>();

		public IReadOnlyList<IView> Children { get => _children.AsReadOnly(); }

		public ILayoutHandler LayoutHandler => Handler as ILayoutHandler;

		protected abstract ILayoutManager CreateLayoutManager();

		public IEnumerator<IView> GetEnumerator() => _children.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _children.GetEnumerator();

		public override Size Measure(double widthConstraint, double heightConstraint)
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

		public override void Arrange(Rectangle bounds)
		{
			if (IsArrangeValid)
			{
				return;
			}

			base.Arrange(bounds);

			LayoutManager.Arrange(Frame);
			IsArrangeValid = true;
			Handler?.SetFrame(Frame);
		}

		public override void InvalidateMeasure()
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

			InvalidateMeasure();

			LayoutHandler?.Add(child);
		}

		public void Remove(IView child)
		{
			if (child == null)
				return;

			_children.Remove(child);

			InvalidateMeasure();

			LayoutHandler?.Remove(child);
		}
	}
}
