#nullable disable
using System;
using System.ComponentModel;
using System.Drawing;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class SwitchCellRenderer : CellRenderer
	{
		const string CellName = "Xamarin.SwitchCell";

		UIColor _defaultOnColor;

		[Preserve(Conditional = true)]
		public SwitchCellRenderer()
		{
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			var tvc = reusableCell as CellTableViewCell;
			UISwitch uiSwitch = null;
			if (tvc == null)
				tvc = new CellTableViewCell(UITableViewCellStyle.Value1, CellName);
			else
			{
				uiSwitch = tvc.AccessoryView as UISwitch;
				CellPropertyChanged -= HandlePropertyChanged;
			}

			SetRealCell(item, tvc);

			if (uiSwitch == null)
			{
				uiSwitch = new UISwitch(new RectangleF());
				uiSwitch.ValueChanged += OnSwitchValueChanged;
				tvc.AccessoryView = uiSwitch;
			}

#pragma warning disable CS0618 // Type or member is obsolete
			var boolCell = (SwitchCell)item;
#pragma warning restore CS0618 // Type or member is obsolete

			_defaultOnColor = UISwitch.Appearance.OnTintColor;

			tvc.Cell = item;
			CellPropertyChanged += HandlePropertyChanged;
			tvc.AccessoryView = uiSwitch;
#pragma warning disable CA1416, CA1422 // TODO: 'UITableViewCell.TextLabel' is unsupported on: 'ios' 14.0 and later
			tvc.TextLabel.Text = boolCell.Text;
#pragma warning restore CA1416, CA1422

			uiSwitch.On = boolCell.On;

			UpdateBackground(tvc, item);
			UpdateIsEnabled(tvc, boolCell);
			UpdateFlowDirection(tvc, boolCell);
			UpdateOnColor(tvc, boolCell);

			return tvc;
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var boolCell = (SwitchCell)sender;
#pragma warning restore CS0618 // Type or member is obsolete
			var realCell = (CellTableViewCell)GetRealCell(boolCell);

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			if (e.PropertyName == SwitchCell.OnProperty.PropertyName)
			{
				((UISwitch)realCell.AccessoryView).SetState(boolCell.On, true);
				UpdateOnColor(realCell, boolCell);
			}
#pragma warning disable CS0618 // Type or member is obsolete
			else if (e.PropertyName == SwitchCell.TextProperty.PropertyName)
#pragma warning disable CA1416, CA1422 // TODO: 'UITableViewCell.TextLabel' is unsupported on: 'ios' 14.0 and later
				realCell.TextLabel.Text = boolCell.Text;
#pragma warning restore CA1416, CA1422
			else if (e.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled(realCell, boolCell);
#pragma warning disable CS0618 // Type or member is obsolete
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection(realCell, boolCell);
			else if (e.PropertyName == SwitchCell.OnColorProperty.PropertyName)
				UpdateOnColor(realCell, boolCell);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		}

		static void OnSwitchValueChanged(object sender, EventArgs eventArgs)
		{
			var view = (UIView)sender;
			var sw = (UISwitch)view;

			CellTableViewCell realCell = null;
			while (view.Superview != null && realCell == null)
			{
				view = view.Superview;
				realCell = view as CellTableViewCell;
			}

			if (realCell != null)
#pragma warning disable CS0618 // Type or member is obsolete
				((SwitchCell)realCell.Cell).On = sw.On;
#pragma warning restore CS0618 // Type or member is obsolete
		}

#pragma warning disable CS0618 // Type or member is obsolete
		void UpdateFlowDirection(CellTableViewCell cell, SwitchCell switchCell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			var uiSwitch = cell.AccessoryView as UISwitch;

			uiSwitch.UpdateFlowDirection((IView)switchCell.Parent);
		}

#pragma warning disable CS0618 // Type or member is obsolete
		void UpdateIsEnabled(CellTableViewCell cell, SwitchCell switchCell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			cell.UserInteractionEnabled = switchCell.IsEnabled;
#pragma warning disable CA1416, CA1422 // TODO: 'UITableViewCell.TextLabel', DetailTextLabel is unsupported on: 'ios' 14.0 and later
			cell.TextLabel.Enabled = switchCell.IsEnabled;
			cell.DetailTextLabel.Enabled = switchCell.IsEnabled;
#pragma warning restore CA1416, CA1422
			var uiSwitch = cell.AccessoryView as UISwitch;
			uiSwitch?.Enabled = switchCell.IsEnabled;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		void UpdateOnColor(CellTableViewCell cell, SwitchCell switchCell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			var uiSwitch = cell.AccessoryView as UISwitch;
			if (uiSwitch != null)
			{
				if (switchCell.OnColor == null)
					uiSwitch.OnTintColor = _defaultOnColor;
				else
					uiSwitch.OnTintColor = switchCell.OnColor.ToPlatform();
			}
		}
	}
}