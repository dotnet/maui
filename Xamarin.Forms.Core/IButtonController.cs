using System;
using System.Windows.Input;

namespace Xamarin.Forms
{
	public interface IButtonController : IViewController
	{
		void SendClicked();
		void SendPressed();
		void SendReleased();
		object CommandParameter { get; set; }
		ICommand Command { get; set; }
		bool IsEnabledCore { set; }
		void PropagateUpClicked();
		void PropagateUpPressed();
		void PropagateUpReleased();
		bool IsPressed { get; }
		void SetIsPressed(bool isPressed);

	}
}