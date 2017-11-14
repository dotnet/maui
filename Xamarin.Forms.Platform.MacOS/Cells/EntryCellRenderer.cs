using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Xamarin.Forms.Platform.MacOS
{
	public class EntryCellRenderer : CellRenderer
	{
		static readonly Color s_defaultTextColor = Color.Black;

		public override NSView GetCell(Cell item, NSView reusableView, NSTableView tv)
		{
			NSTextField nsEntry = null;
			var tvc = reusableView as CellNSView;
			if (tvc == null)
				tvc = new CellNSView(NSTableViewCellStyle.Value2);
			else
			{
				tvc.Cell.PropertyChanged -= OnCellPropertyChanged;

				nsEntry = tvc.AccessoryView.Subviews[0] as NSTextField;
				if (nsEntry != null)
				{
					nsEntry.RemoveFromSuperview();
					nsEntry.Changed -= OnTextFieldTextChanged;
				}
			}

			SetRealCell(item, tvc);

			if (nsEntry == null)
				tvc.AccessoryView.AddSubview(nsEntry = new NSTextField());

			var entryCell = (EntryCell)item;

			tvc.Cell = item;
			tvc.Cell.PropertyChanged += OnCellPropertyChanged;
			nsEntry.Changed += OnTextFieldTextChanged;

			WireUpForceUpdateSizeRequested(item, tvc, tv);

			UpdateBackground(tvc, entryCell);
			UpdateLabel(tvc, entryCell);
			UpdateText(tvc, entryCell);
			UpdatePlaceholder(tvc, entryCell);
			UpdateLabelColor(tvc, entryCell);
			UpdateHorizontalTextAlignment(tvc, entryCell);
			UpdateIsEnabled(tvc, entryCell);

			return tvc;
		}

		internal override void UpdateBackgroundChild(Cell cell, NSColor backgroundColor)
		{
			var realCell = (CellNSView)GetRealCell(cell);

			var nsTextField = realCell.AccessoryView.Subviews[0] as NSTextField;
			if (nsTextField != null)
				nsTextField.BackgroundColor = backgroundColor;

			base.UpdateBackgroundChild(cell, backgroundColor);
		}

		static void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var entryCell = (EntryCell)sender;
			var realCell = (CellNSView)GetRealCell(entryCell);

			if (e.PropertyName == EntryCell.LabelProperty.PropertyName)
				UpdateLabel(realCell, entryCell);
			else if (e.PropertyName == EntryCell.TextProperty.PropertyName)
				UpdateText(realCell, entryCell);
			else if (e.PropertyName == EntryCell.PlaceholderProperty.PropertyName)
				UpdatePlaceholder(realCell, entryCell);
			else if (e.PropertyName == EntryCell.LabelColorProperty.PropertyName)
				UpdateLabelColor(realCell, entryCell);
			else if (e.PropertyName == EntryCell.HorizontalTextAlignmentProperty.PropertyName)
				UpdateHorizontalTextAlignment(realCell, entryCell);
			else if (e.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled(realCell, entryCell);
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateHorizontalTextAlignment(realCell, entryCell);
		}

		static void OnTextFieldTextChanged(object sender, EventArgs eventArgs)
		{
			var notification = (NSNotification)sender;
			var view = (NSView)notification.Object;
			var field = (NSTextField)view;

			CellNSView realCell = null;
			while (view.Superview != null && realCell == null)
			{
				view = view.Superview;
				realCell = view as CellNSView;
			}

			if (realCell != null)
				((EntryCell)realCell.Cell).Text = field.StringValue;
		}

		static void UpdateHorizontalTextAlignment(CellNSView cell, EntryCell entryCell)
		{
			IVisualElementController viewController = entryCell.Parent as VisualElement;

			var nsTextField = cell.AccessoryView.Subviews[0] as NSTextField;
			if (nsTextField != null)
				nsTextField.Alignment = entryCell.HorizontalTextAlignment.ToNativeTextAlignment(viewController?.EffectiveFlowDirection ?? default(EffectiveFlowDirection));
		}

		static void UpdateIsEnabled(CellNSView cell, EntryCell entryCell)
		{
			cell.TextLabel.Enabled = entryCell.IsEnabled;
			var nsTextField = cell.AccessoryView.Subviews[0] as NSTextField;
			if (nsTextField != null)
				nsTextField.Enabled = entryCell.IsEnabled;
		}

		static void UpdateLabel(CellNSView cell, EntryCell entryCell)
		{
			cell.TextLabel.StringValue = entryCell.Label ?? "";
		}

		static void UpdateLabelColor(CellNSView cell, EntryCell entryCell)
		{
			cell.TextLabel.TextColor = entryCell.LabelColor.ToNSColor(s_defaultTextColor);
		}

		static void UpdatePlaceholder(CellNSView cell, EntryCell entryCell)
		{
			var nsTextField = cell.AccessoryView.Subviews[0] as NSTextField;
			if (nsTextField != null)
				nsTextField.PlaceholderString = entryCell.Placeholder ?? "";
		}

		static void UpdateText(CellNSView cell, EntryCell entryCell)
		{
			var nsTextField = cell.AccessoryView.Subviews[0] as NSTextField;
			if (nsTextField != null && nsTextField.StringValue == entryCell.Text)
				return;

			if (nsTextField != null)
				nsTextField.StringValue = entryCell.Text ?? "";
		}
	}
}