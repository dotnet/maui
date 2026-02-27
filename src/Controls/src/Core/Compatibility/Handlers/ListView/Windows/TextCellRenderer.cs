#nullable disable
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using WApplication = Microsoft.UI.Xaml.Application;
using WDataTemplate = Microsoft.UI.Xaml.DataTemplate;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
#pragma warning disable CS0618 // Type or member is obsolete
	public abstract class CellRenderer : ElementHandler<Cell, WDataTemplate>, IRegisterable, ICellRenderer
#pragma warning restore CS0618 // Type or member is obsolete
	{
		public CellRenderer() : base(ElementHandler.ElementMapper)
		{
		}

		protected override WDataTemplate CreatePlatformElement() =>
			GetTemplate(VirtualView);

#pragma warning disable CS0618 // Type or member is obsolete
		public abstract WDataTemplate GetTemplate(Cell cell);
#pragma warning restore CS0618 // Type or member is obsolete
	}

	public class TextCellRenderer : CellRenderer
	{
#pragma warning disable CS0618 // Type or member is obsolete
		public override WDataTemplate GetTemplate(Cell cell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (cell.RealParent is ListView)
			{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
				if (cell.GetIsGroupHeader<ItemsView<Cell>, Cell>())
					return (WDataTemplate)WApplication.Current.Resources["ListViewHeaderTextCell"];
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

				//return (WDataTemplate) WApplication.Current.Resources["ListViewTextCell"];
			}
#pragma warning restore CS0618 // Type or member is obsolete

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
#pragma warning disable CS0618 // Type or member is obsolete
		public override WDataTemplate GetTemplate(Cell cell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return (WDataTemplate)WApplication.Current.Resources["EntryCell"];
		}
	}

	public class ViewCellRenderer : CellRenderer
	{
#pragma warning disable CS0618 // Type or member is obsolete
		public override WDataTemplate GetTemplate(Cell cell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return (WDataTemplate)WApplication.Current.Resources["ViewCell"];
		}
	}

	public class SwitchCellRenderer : CellRenderer
	{
#pragma warning disable CS0618 // Type or member is obsolete
		public override WDataTemplate GetTemplate(Cell cell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return (WDataTemplate)WApplication.Current.Resources["SwitchCell"];
		}
	}

	public class ImageCellRenderer : CellRenderer
	{
#pragma warning disable CS0618 // Type or member is obsolete
		public override WDataTemplate GetTemplate(Cell cell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			//if (cell.Parent is ListView)
			//	return (WDataTemplate)WApplication.Current.Resources["ListImageCell"];
			return (WDataTemplate)WApplication.Current.Resources["ImageCell"];
		}
	}
}