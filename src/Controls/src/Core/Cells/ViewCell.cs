#nullable disable
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/ViewCell.xml" path="Type[@FullName='Microsoft.Maui.Controls.ViewCell']/Docs/*" />
	[ContentProperty("View")]
	public class ViewCell : Cell
	{
		View _view;

		/// <include file="../../../docs/Microsoft.Maui.Controls/ViewCell.xml" path="//Member[@MemberName='View']/Docs/*" />
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