using System;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	public interface IButtonController : IViewController
	{
		void SendClicked();
		void SendPressed();
		void SendReleased();
	}
}