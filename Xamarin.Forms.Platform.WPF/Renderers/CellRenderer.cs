using System;
using System.Windows.Controls;
using System.Windows.Input;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WPF
{
	public class TextCellRenderer : ICellRenderer
	{
		public virtual System.Windows.DataTemplate GetTemplate(Cell cell)
		{
			/*if (cell.RealParent is ListView)
			{
				if (cell.GetIsGroupHeader<ItemsView<Cell>, Cell>())
					return (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["ListViewHeaderTextCell"];

				return (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["ListViewTextCell"];
			}*/

			return (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["TextCell"];
		}
	}

	public class EntryCellRendererCompleted : ICommand
	{
		public bool CanExecute(object parameter)
		{
			return true;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			var entryCell = (IEntryCellController)parameter;
			entryCell.SendCompleted();
		}

		protected virtual void OnCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public class EntryCellPhoneTextBox : TextBox
	{
		public event EventHandler KeyboardReturnPressed;

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				EventHandler handler = KeyboardReturnPressed;
				if (handler != null)
					handler(this, EventArgs.Empty);
			}
			base.OnKeyUp(e);
		}
	}

	public class EntryCellRenderer : ICellRenderer
	{
		public virtual System.Windows.DataTemplate GetTemplate(Cell cell)
		{
			return (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["EntryCell"];
		}
	}

	public class ViewCellRenderer : ICellRenderer
	{
		public virtual System.Windows.DataTemplate GetTemplate(Cell cell)
		{
			return (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["ViewCell"];
		}
	}

	public class SwitchCellRenderer : ICellRenderer
	{
		public virtual System.Windows.DataTemplate GetTemplate(Cell cell)
		{
			return (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["SwitchCell"];
		}
	}

	public class ImageCellRenderer : ICellRenderer
	{
		public virtual System.Windows.DataTemplate GetTemplate(Cell cell)
		{
			return (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["ImageCell"];
		}
	}
}