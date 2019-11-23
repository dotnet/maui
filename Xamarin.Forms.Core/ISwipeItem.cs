using System;
using System.Windows.Input;

namespace Xamarin.Forms
{
	public interface ISwipeItem
	{
		ICommand Command { get; set; }
		object CommandParameter { get; set; }

		event EventHandler<EventArgs> Invoked;
		void OnInvoked();
	}
}