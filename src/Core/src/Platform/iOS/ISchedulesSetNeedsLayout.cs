namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Marks a <see cref="UIKit.UIView"/> that can schedule <see cref="UIKit.UIView.SetNeedsLayout"/> when attached to window.
	/// </summary>
	internal interface ISchedulesSetNeedsLayout
	{
		/// <summary>
		/// Schedules <see cref="UIKit.UIView.SetNeedsLayout"/> when attached to window.
		/// </summary>
		void ScheduleSetNeedsLayoutPropagation();
	}
}