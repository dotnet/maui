using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Maui
{
	public interface ICellController
	{
		event EventHandler ForceUpdateSizeRequested;

		void SendAppearing();
		void SendDisappearing();
	}
}
