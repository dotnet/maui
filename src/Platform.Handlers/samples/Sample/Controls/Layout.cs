using System.Collections;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Platform.Layouts;

namespace Xamarin.Platform
{
	public abstract class Layout : View, ILayout, IEnumerable<IView>
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

			DesiredSize = LayoutManager.Measure(widthConstraint, heightConstraint);
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

			LayoutManager.Arrange(bounds);
			IsArrangeValid = true;
			Handler?.SetFrame(bounds);
		}

	}
}
