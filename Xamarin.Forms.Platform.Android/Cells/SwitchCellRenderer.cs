using System.ComponentModel;
using Android.Content;
using Android.Views;
using AView = Android.Views.View;
using ASwitch = Android.Widget.Switch;

namespace Xamarin.Forms.Platform.Android
{
	public class SwitchCellRenderer : CellRenderer
	{
		SwitchCellView _view;

		protected override AView GetCellCore(Cell item, AView convertView, ViewGroup parent, Context context)
		{
			var cell = (SwitchCell)Cell;

			if ((_view = convertView as SwitchCellView) == null)
				_view = new SwitchCellView(context, item);

			_view.Cell = cell;

			UpdateText();
			UpdateChecked();
			UpdateHeight();
			UpdateIsEnabled(_view, cell);
			UpdateFlowDirection();

			return _view;
		}

		protected override void OnCellPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName == SwitchCell.TextProperty.PropertyName)
				UpdateText();
			else if (args.PropertyName == SwitchCell.OnProperty.PropertyName)
				UpdateChecked();
			else if (args.PropertyName == "RenderHeight")
				UpdateHeight();
			else if (args.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled(_view, (SwitchCell)sender);
			else if (args.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
		}

		void UpdateChecked()
		{
			((ASwitch)_view.AccessoryView).Checked = ((SwitchCell)Cell).On;
		}

		void UpdateIsEnabled(SwitchCellView cell, SwitchCell switchCell)
		{
			cell.Enabled = switchCell.IsEnabled;
			var aSwitch = cell.AccessoryView as ASwitch;
			if (aSwitch != null)
				aSwitch.Enabled = switchCell.IsEnabled;
		}

		void UpdateFlowDirection()
		{
			_view.UpdateFlowDirection(ParentView);
		}

		void UpdateHeight()
		{
			_view.SetRenderHeight(Cell.RenderHeight);
		}

		void UpdateText()
		{
			_view.MainText = ((SwitchCell)Cell).Text;
		}
	}
}