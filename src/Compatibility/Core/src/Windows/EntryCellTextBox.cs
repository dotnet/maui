using Windows.System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
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