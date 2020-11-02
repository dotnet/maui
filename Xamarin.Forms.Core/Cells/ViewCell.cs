using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xamarin.Forms
{
	[ContentProperty("View")]
	public class ViewCell : Cell
	{
		ReadOnlyCollection<Element> _logicalChildren;

		View _view;

		public View View
		{
			get { return _view; }
			set
			{
				if (_view == value)
					return;

				OnPropertyChanging();

				if (_view != null)
				{
					OnChildRemoved(_view, 0);
					_view.ComputedConstraint = LayoutConstraint.None;
				}

				_view = value;

				if (_view != null)
				{
					_view.ComputedConstraint = LayoutConstraint.Fixed;
					_logicalChildren = new ReadOnlyCollection<Element>(new List<Element>(new[] { View }));
					OnChildAdded(_view);
				}
				else
				{
					_logicalChildren = null;
				}
				ForceUpdateSize();
				OnPropertyChanged();
			}
		}

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal => _logicalChildren ?? base.LogicalChildrenInternal;
	}
}