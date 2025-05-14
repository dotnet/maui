#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.Cell"/> containing a developer-defined <see cref="Microsoft.Maui.Controls.View"/>.</summary>
	[Obsolete("The controls which use ViewCell (ListView and TableView) are obsolete. Please use CollectionView instead.")]
	[ContentProperty("View")]
#if WINDOWS || IOS || MACCATALYST || ANDROID || TIZEN
#pragma warning disable CS0618 // Type or member is obsolete
	[ElementHandler<Handlers.Compatibility.ViewCellRenderer>]
#pragma warning restore CS0618 // Type or member is obsolete
#endif
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