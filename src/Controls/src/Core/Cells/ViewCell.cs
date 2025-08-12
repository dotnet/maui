#nullable disable
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="T:Microsoft.Maui.Controls.Cell"/> containing a developer-defined <see cref="T:Microsoft.Maui.Controls.View"/>.</summary>
	[ContentProperty("View")]
	public class ViewCell : Cell
	{
		View _view;

		/// <summary>Gets or sets the View representing the content of the ViewCell.</summary>
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
					RemoveLogicalChild(_view);
					_view.ComputedConstraint = LayoutConstraint.None;
				}

				_view = value;

				if (_view != null)
				{
					_view.ComputedConstraint = LayoutConstraint.Fixed;
					AddLogicalChild(_view);
				}

				ForceUpdateSize();
				OnPropertyChanged();
			}
		}
	}
}