using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using WApplication = Microsoft.UI.Xaml.Application;
using WDataTemplate = Microsoft.UI.Xaml.DataTemplate;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract class CellRenderer : ElementHandler<Cell, WDataTemplate>, IRegisterable, ICellRenderer
	{
		public CellRenderer() : base(ElementHandler.ElementMapper)
		{
		}

		protected override WDataTemplate CreatePlatformElement() =>
			GetTemplate(VirtualView);

		public abstract WDataTemplate GetTemplate(Cell cell);
	}

	public class TextCellRenderer : CellRenderer
	{
		public override WDataTemplate GetTemplate(Cell cell)
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

	public class EntryCellRenderer : CellRenderer
	{
		public override WDataTemplate GetTemplate(Cell cell)
		{
			return (WDataTemplate)WApplication.Current.Resources["EntryCell"];
		}
	}

	public class ViewCellRenderer : CellRenderer
	{
		public override WDataTemplate GetTemplate(Cell cell)
		{
			return (WDataTemplate)WApplication.Current.Resources["ViewCell"];
		}
	}

	public class SwitchCellRenderer : CellRenderer
	{
		public override WDataTemplate GetTemplate(Cell cell)
		{
			return (WDataTemplate)WApplication.Current.Resources["SwitchCell"];
		}
	}

	public class ImageCellRenderer : CellRenderer
	{
		public override WDataTemplate GetTemplate(Cell cell)
		{
			//if (cell.Parent is ListView)
			//	return (WDataTemplate)WApplication.Current.Resources["ListImageCell"];
			return (WDataTemplate)WApplication.Current.Resources["ImageCell"];
		}
	}
}