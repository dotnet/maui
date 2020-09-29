using System;
using System.ComponentModel;

namespace Xamarin.Forms
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