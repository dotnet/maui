using System.ComponentModel;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class TextCellRenderer : CellRenderer
	{
		readonly Color DefaultDetailColor = Microsoft.Maui.Platform.ColorExtensions.SecondaryLabelColor.ToColor();
		readonly Color DefaultTextColor = Microsoft.Maui.Platform.ColorExtensions.LabelColor.ToColor();

		[Preserve(Conditional = true)]
		public TextCellRenderer()
		{
		}

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

#pragma warning disable CA1416, CA1422 // TODO: 'UITableViewCell.TextLabel', DetailTextLabel is unsupported on: 'ios' 14.0 and later
			tvc.TextLabel.Text = textCell.Text;
			tvc.DetailTextLabel.Text = textCell.Detail;
			tvc.TextLabel.TextColor = (textCell.TextColor ?? DefaultTextColor).ToPlatform();
			tvc.DetailTextLabel.TextColor = (textCell.DetailColor ?? DefaultDetailColor).ToPlatform();

			WireUpForceUpdateSizeRequested(item, tvc, tv);

			UpdateIsEnabled(tvc, textCell);
#pragma warning restore CA1416, CA1422

			UpdateBackground(tvc, item);

			SetAccessibility(tvc, item);
			UpdateAutomationId(tvc, textCell);

			return tvc;
		}

		protected virtual void HandleCellPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			var textCell = (TextCell)sender;
			var tvc = (CellTableViewCell)GetRealCell(textCell);

#pragma warning disable CA1416, CA1422 // TODO: 'UITableViewCell.TextLabel', DetailTextLabel is unsupported on: 'ios' 14.0 and later
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
				tvc.TextLabel.TextColor = textCell.TextColor?.ToPlatform() ?? DefaultTextColor.ToPlatform();
			else if (args.PropertyName == TextCell.DetailColorProperty.PropertyName)
				tvc.DetailTextLabel.TextColor = textCell.DetailColor?.ToPlatform() ?? DefaultTextColor.ToPlatform();
			else if (args.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled(tvc, textCell);
			else if (args.PropertyName == TextCell.AutomationIdProperty.PropertyName)
				UpdateAutomationId(tvc, textCell);
#pragma warning restore CA1416, CA1422

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

		[System.Runtime.Versioning.UnsupportedOSPlatform("ios14.0")]
		[System.Runtime.Versioning.UnsupportedOSPlatform("tvos14.0")]
		static void UpdateIsEnabled(CellTableViewCell cell, TextCell entryCell)
		{
			cell.UserInteractionEnabled = entryCell.IsEnabled;
			cell.TextLabel.Enabled = entryCell.IsEnabled;
			cell.DetailTextLabel.Enabled = entryCell.IsEnabled;
		}
	}
}
