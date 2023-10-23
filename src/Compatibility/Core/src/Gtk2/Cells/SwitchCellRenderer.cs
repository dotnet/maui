using System.ComponentModel;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK.Cells
{
	public class SwitchCellRenderer : CellRenderer
	{
		public override CellBase GetCell(Cell item, Gtk.Container reusableView, Controls.ListView listView)
		{
			var switchCell = base.GetCell(item, reusableView, listView) as SwitchCell;

			switchCell.Toggled -= OnToggled;
			switchCell.Toggled += OnToggled;

			return switchCell;
		}

		protected override Gtk.Container GetCellWidgetInstance(Cell item)
		{
			var switchCell = (Microsoft.Maui.Controls.Compatibility.SwitchCell)item;

			var text = switchCell.Text ?? string.Empty;
			var on = switchCell.On;

			return new SwitchCell(text, on);
		}

		protected override void CellPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.CellPropertyChanged(sender, args);

			var gtkSwitchCell = (SwitchCell)sender;
			var switchCell = (Microsoft.Maui.Controls.Compatibility.SwitchCell)gtkSwitchCell.Cell;

			if (args.PropertyName == Microsoft.Maui.Controls.Compatibility.SwitchCell.TextProperty.PropertyName)
			{
				gtkSwitchCell.Text = switchCell.Text ?? string.Empty;
			}
			else if (args.PropertyName == Microsoft.Maui.Controls.Compatibility.SwitchCell.OnProperty.PropertyName)
			{
				gtkSwitchCell.On = switchCell.On;
			}
		}

		private void OnToggled(object sender, bool active)
		{
			((Microsoft.Maui.Controls.Compatibility.SwitchCell)Cell).On = active;
		}
	}
}
