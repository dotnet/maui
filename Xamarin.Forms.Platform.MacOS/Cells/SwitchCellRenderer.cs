using System;
using System.ComponentModel;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public class SwitchCellRenderer : CellRenderer
	{
		public override NSView GetCell(Cell item, NSView reusableView, NSTableView tv)
		{
			var tvc = reusableView as CellNSView;
			NSButton nsSwitch = null;
			if (tvc == null)
				tvc = new CellNSView(NSTableViewCellStyle.Value1);
			else
			{
				nsSwitch = tvc.AccessoryView.Subviews[0] as NSButton;
				if (nsSwitch != null)
				{
					nsSwitch.RemoveFromSuperview();
					nsSwitch.Activated -= OnSwitchValueChanged;
				}
				tvc.Cell.PropertyChanged -= OnCellPropertyChanged;
			}

			SetRealCell(item, tvc);

			if (nsSwitch == null)
			{
				nsSwitch = new NSButton { AllowsMixedState = false, Title = string.Empty };
				nsSwitch.SetButtonType(NSButtonType.Switch);
			}

			var boolCell = (SwitchCell)item;

			tvc.Cell = item;
			tvc.Cell.PropertyChanged += OnCellPropertyChanged;
			tvc.AccessoryView.AddSubview(nsSwitch);
			tvc.TextLabel.StringValue = boolCell.Text ?? "";

			nsSwitch.State = boolCell.On ? NSCellStateValue.On : NSCellStateValue.Off;
			nsSwitch.Activated += OnSwitchValueChanged;
			WireUpForceUpdateSizeRequested(item, tvc, tv);

			UpdateBackground(tvc, item);
			UpdateIsEnabled(tvc, boolCell);

			return tvc;
		}

		static void UpdateIsEnabled(CellNSView cell, SwitchCell switchCell)
		{
			cell.TextLabel.Enabled = switchCell.IsEnabled;
			var uiSwitch = cell.AccessoryView.Subviews[0] as NSButton;
			if (uiSwitch != null)
				uiSwitch.Enabled = switchCell.IsEnabled;
		}

		void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var boolCell = (SwitchCell)sender;
			var realCell = (CellNSView)GetRealCell(boolCell);

			if (e.PropertyName == SwitchCell.OnProperty.PropertyName)
				((NSButton)realCell.AccessoryView.Subviews[0]).State = boolCell.On ? NSCellStateValue.On : NSCellStateValue.Off;
			else if (e.PropertyName == SwitchCell.TextProperty.PropertyName)
				realCell.TextLabel.StringValue = boolCell.Text ?? "";
			else if (e.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled(realCell, boolCell);
		}

		void OnSwitchValueChanged(object sender, EventArgs eventArgs)
		{
			var view = (NSView)sender;
			var sw = (NSButton)view;

			CellNSView realCell = null;
			while (view.Superview != null && realCell == null)
			{
				view = view.Superview;
				realCell = view as CellNSView;
			}

			if (realCell != null)
				((SwitchCell)realCell.Cell).On = sw.State == NSCellStateValue.On;
		}
	}
}