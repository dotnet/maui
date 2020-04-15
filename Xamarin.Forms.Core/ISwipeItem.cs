using System;
using System.Windows.Input;

namespace Xamarin.Forms
{
	public interface ISwipeItem
	{
		bool IsVisible { get; set; }
		ICommand Command { get; set; }
		object CommandParameter { get; set; }

		event EventHandler<EventArgs> Invoked;
		void OnInvoked();
	}
}