using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Xamarin.Forms.Platform.UWP
{
	public class EntryCellTextBox : TextBox
	{
		protected override void OnKeyUp(KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Enter)
			{
				var cell = DataContext as IEntryCellController;
				if (cell != null)
				{
					cell.SendCompleted();
					e.Handled = true;
				}
			}

			base.OnKeyUp(e);
		}
	}
}