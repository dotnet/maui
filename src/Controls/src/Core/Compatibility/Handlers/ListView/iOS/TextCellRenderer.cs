#nullable disable
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

#pragma warning disable CS0618 // Type or member is obsolete
		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
#pragma warning restore CS0618 // Type or member is obsolete
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var textCell = (TextCell)item;
#pragma warning restore CS0618 // Type or member is obsolete

			if (!(reusableCell is CellTableViewCell tvc))
				tvc = new CellTableViewCell(UITableViewCellStyle.Subtitle, item.GetType().FullName);
			else
				CellPropertyChanged -= HandleCellPropertyChanged;

			SetRealCell(item, tvc);

			tvc.Cell = textCell;
			CellPropertyChanged += HandleCellPropertyChanged;

#pragma warning disable CA1416, CA1422 // TODO: 'UITableViewCell.TextLabel', DetailTextLabel is unsupported on: 'ios' 14.0 and later
			tvc.TextLabel.Text = textCell.Text;
			tvc.DetailTextLabel.Text = textCell.Detail;
			tvc.TextLabel.TextColor = (textCell.TextColor ?? DefaultTextColor).ToPlatform();
			tvc.DetailTextLabel.TextColor = (textCell.DetailColor ?? DefaultDetailColor).ToPlatform();

			UpdateIsEnabled(tvc, textCell);
#pragma warning restore CA1416, CA1422

			UpdateBackground(tvc, item);

			SetAccessibility(tvc, item);
			UpdateAutomationId(tvc, textCell);

			return tvc;
		}

		protected virtual void HandleCellPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var textCell = (TextCell)sender;
#pragma warning restore CS0618 // Type or member is obsolete
			var tvc = (CellTableViewCell)GetRealCell(textCell);

#pragma warning disable CA1416, CA1422 // TODO: 'UITableViewCell.TextLabel', DetailTextLabel is unsupported on: 'ios' 14.0 and later
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			if (args.PropertyName == TextCell.TextProperty.PropertyName)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				tvc.TextLabel.Text = ((TextCell)tvc.Cell).Text;
#pragma warning restore CS0618 // Type or member is obsolete
				tvc.TextLabel.SizeToFit();
			}
#pragma warning disable CS0618 // Type or member is obsolete
			else if (args.PropertyName == TextCell.DetailProperty.PropertyName)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				tvc.DetailTextLabel.Text = ((TextCell)tvc.Cell).Detail;
#pragma warning restore CS0618 // Type or member is obsolete
				tvc.DetailTextLabel.SizeToFit();
			}
#pragma warning disable CS0618 // Type or member is obsolete
			else if (args.PropertyName == TextCell.TextColorProperty.PropertyName)
				tvc.TextLabel.TextColor = textCell.TextColor?.ToPlatform() ?? DefaultTextColor.ToPlatform();
#pragma warning disable CS0618 // Type or member is obsolete
			else if (args.PropertyName == TextCell.DetailColorProperty.PropertyName)
				tvc.DetailTextLabel.TextColor = textCell.DetailColor?.ToPlatform() ?? DefaultTextColor.ToPlatform();
#pragma warning disable CS0618 // Type or member is obsolete
			else if (args.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled(tvc, textCell);
			else if (args.PropertyName == TextCell.AutomationIdProperty.PropertyName)
				UpdateAutomationId(tvc, textCell);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CA1416, CA1422

			HandlePropertyChanged(tvc, args);
		}
#pragma warning disable CS0618 // Type or member is obsolete
		void UpdateAutomationId(CellTableViewCell tvc, TextCell cell)
#pragma warning restore CS0618 // Type or member is obsolete
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
#pragma warning disable CS0618 // Type or member is obsolete
		static void UpdateIsEnabled(CellTableViewCell cell, TextCell entryCell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			cell.UserInteractionEnabled = entryCell.IsEnabled;
			cell.TextLabel.Enabled = entryCell.IsEnabled;
			cell.DetailTextLabel.Enabled = entryCell.IsEnabled;
		}
	}
}
