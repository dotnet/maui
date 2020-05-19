using System;
using System.Windows.Input;

namespace System.Maui
{
	public interface IButtonController : IViewController
	{
		void SendClicked();
		void SendPressed();
		void SendReleased();
	}
}