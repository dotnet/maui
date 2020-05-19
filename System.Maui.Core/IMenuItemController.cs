using System;
using System.ComponentModel;

namespace System.Maui
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IMenuItemController
	{
		bool IsEnabled { get; set; }
		void Activate();

		[Obsolete("This property is obsolete as of 3.5.0. Please use MenuItem.IsEnabledProperty.PropertyName instead.")]
		string IsEnabledPropertyName { get; }
	}
}
