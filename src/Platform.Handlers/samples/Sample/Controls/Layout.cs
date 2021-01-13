using System.Collections;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Platform;
using Xamarin.Platform.Layouts;

namespace Sample
{
	public abstract class Layout : View, Xamarin.Platform.ILayout, IEnumerable<IView>
	{
		ILayoutManager _layoutManager;
		ILayoutManager LayoutManager => _layoutManager ??= CreateLayoutManager();

		readonly List<IView> _children = new List<IView>();

		public IReadOnlyList<IView> Children { get => _children.AsReadOnly(); }

		public void Add(IView view)
		{
			if (view == null)
				return;

			_children.Add(view);

			InvalidateMeasure();
		}

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
	}
}
