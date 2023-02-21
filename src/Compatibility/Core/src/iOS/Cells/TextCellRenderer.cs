using System;
using System.ComponentModel;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[Obsolete("Use Microsoft.Maui.Controls.Platform.Compatibility.TextCellRenderer instead")]
	public class TextCellRenderer : CellRenderer
	{
		readonly Color DefaultDetailColor = Maui.Platform.ColorExtensions.SecondaryLabelColor.ToColor();
		readonly Color DefaultTextColor = Maui.Platform.ColorExtensions.LabelColor.ToColor();

		[Preserve(Conditional = true)]
		public TextCellRenderer()
		{
		}

#pragma warning disable CA1416, CA1422  // TODO: 'UITableViewCell.TextLabel' is unsupported on: 'ios' 14.0 and later
		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			var textCell = (TextCell)item;

			if (!(reusableCell is CellTableViewCell tvc))
				tvc = new CellTableViewCell(UITableViewCellStyle.Subtitle, item.GetType().FullName);
			else
				tvc.PropertyChanged -= HandleCellPropertyChanged;

			SetRealCell(item, tvc);

			tvc.Cell = textCell;
			tvc.PropertyChanged = HandleCellPropertyChanged;

			tvc.TextLabel.Text = textCell.Text;
			tvc.DetailTextLabel.Text = textCell.Detail;
			tvc.TextLabel.TextColor = textCell.TextColor.ToPlatform(DefaultTextColor);
			tvc.DetailTextLabel.TextColor = textCell.DetailColor.ToPlatform(DefaultDetailColor);

			WireUpForceUpdateSizeRequested(item, tvc, tv);

			UpdateIsEnabled(tvc, textCell);

			UpdateBackground(tvc, item);

			SetAccessibility(tvc, item);
			UpdateAutomationId(tvc, textCell);

			return tvc;
		}

		protected virtual void HandleCellPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			var textCell = (TextCell)sender;
			var tvc = (CellTableViewCell)GetRealCell(textCell);

			if (args.PropertyName == TextCell.TextProperty.PropertyName)
			{
				tvc.TextLabel.Text = ((TextCell)tvc.Cell).Text;
				tvc.TextLabel.SizeToFit();
			}
			else if (args.PropertyName == TextCell.DetailProperty.PropertyName)
			{
				tvc.DetailTextLabel.Text = ((TextCell)tvc.Cell).Detail;
				tvc.DetailTextLabel.SizeToFit();
			}
			else if (args.PropertyName == TextCell.TextColorProperty.PropertyName)
				tvc.TextLabel.TextColor = textCell.TextColor.ToPlatform(DefaultTextColor);
			else if (args.PropertyName == TextCell.DetailColorProperty.PropertyName)
				tvc.DetailTextLabel.TextColor = textCell.DetailColor.ToPlatform(DefaultTextColor);
			else if (args.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled(tvc, textCell);
			else if (args.PropertyName == TextCell.AutomationIdProperty.PropertyName)
				UpdateAutomationId(tvc, textCell);

			HandlePropertyChanged(tvc, args);
		}
		void UpdateAutomationId(CellTableViewCell tvc, TextCell cell)
		{
			tvc.AccessibilityIdentifier = cell.AutomationId;
		}

		protected virtual void HandlePropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			//keeping this method for backwards compatibility 
			//as the the sender for this method is a CellTableViewCell
		}

		static void UpdateIsEnabled(CellTableViewCell cell, TextCell entryCell)
		{
			cell.UserInteractionEnabled = entryCell.IsEnabled;
			cell.TextLabel.Enabled = entryCell.IsEnabled;
			cell.DetailTextLabel.Enabled = entryCell.IsEnabled;
		}
#pragma warning restore CA1416, CA1422
	}
}
