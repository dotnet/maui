using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform.Compatibility;
using WApplication = Microsoft.UI.Xaml.Application;
using WDataTemplate = Microsoft.UI.Xaml.DataTemplate;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[Obsolete("Use Microsoft.Maui.Controls.Platform.Compatibility.TextCellRenderer instead")]
	public class TextCellRenderer : ICellRenderer
	{
		public virtual WDataTemplate GetTemplate(Cell cell)
		{
			if (cell.RealParent is ListView)
			{
				if (cell.GetIsGroupHeader<ItemsView<Cell>, Cell>())
					return (WDataTemplate)WApplication.Current.Resources["ListViewHeaderTextCell"];

				//return (WDataTemplate) WApplication.Current.Resources["ListViewTextCell"];
			}

			return (WDataTemplate)WApplication.Current.Resources["TextCell"];
		}
	}

	[Obsolete("Use Microsoft.Maui.Controls.Platform.Compatibility.EntryCellRendererCompleted instead")]
	public class EntryCellRendererCompleted : ICommand
	{
		public bool CanExecute(object parameter)
		{
			return true;
		}

#pragma warning disable 0067 // Revisit: Can't remove; required by interface
		public event EventHandler CanExecuteChanged;
#pragma warning restore

		public void Execute(object parameter)
		{
			var entryCell = (IEntryCellController)parameter;
			entryCell.SendCompleted();
		}
	}

	[Obsolete("Use Microsoft.Maui.Controls.Platform.Compatibility.EntryCellRenderer instead")]
	public class EntryCellRenderer : ICellRenderer
	{
		public virtual WDataTemplate GetTemplate(Cell cell)
		{
			return (WDataTemplate)WApplication.Current.Resources["EntryCell"];
		}
	}

	[Obsolete("Use Microsoft.Maui.Controls.Platform.Compatibility.ViewCellRenderer instead")]
	public class ViewCellRenderer : ICellRenderer
	{
		public virtual WDataTemplate GetTemplate(Cell cell)
		{
			return (WDataTemplate)WApplication.Current.Resources["ViewCell"];
		}
	}

	[Obsolete("Use Microsoft.Maui.Controls.Platform.Compatibility.SwitchCellRenderer instead")]
	public class SwitchCellRenderer : ICellRenderer
	{
		public virtual WDataTemplate GetTemplate(Cell cell)
		{
			return (WDataTemplate)WApplication.Current.Resources["SwitchCell"];
		}
	}

	[Obsolete("Use Microsoft.Maui.Controls.Platform.Compatibility.ImageCellRenderer instead")]
	public class ImageCellRenderer : ICellRenderer
	{
		public virtual WDataTemplate GetTemplate(Cell cell)
		{
			//if (cell.Parent is ListView)
			//	return (WDataTemplate)WApplication.Current.Resources["ListImageCell"];
			return (WDataTemplate)WApplication.Current.Resources["ImageCell"];
		}
	}
}