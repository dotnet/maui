using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Platform.WPF.Helpers
{
	static class UiHelper
	{
		public static void ExecuteInUiThread(Action action)
		{
			if (System.Windows.Application.Current.Dispatcher.CheckAccess())
			{
				action?.Invoke();
			}
			else
			{
				System.Windows.Application.Current.Dispatcher.Invoke(action);
			}
		}
	}
}
