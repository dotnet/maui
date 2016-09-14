using System.ComponentModel;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class TextCellRenderer : CellRenderer
	{
		static readonly Color DefaultDetailColor = new Color(.32, .4, .57);
		static readonly Color DefaultTextColor = Color.Black;

		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			var textCell = (TextCell)item;

			var tvc = reusableCell as CellTableViewCell;
			if (tvc == null)
				tvc = new CellTableViewCell(UITableViewCellStyle.Subtitle, item.GetType().FullName);
			else
				tvc.Cell.PropertyChanged -= tvc.HandlePropertyChanged;

			tvc.Cell = textCell;
			textCell.PropertyChanged += tvc.HandlePropertyChanged;
			tvc.PropertyChanged = HandlePropertyChanged;

			tvc.TextLabel.Text = textCell.Text;
			tvc.DetailTextLabel.Text = textCell.Detail;
			tvc.TextLabel.TextColor = textCell.TextColor.ToUIColor(DefaultTextColor);
			tvc.DetailTextLabel.TextColor = textCell.DetailColor.ToUIColor(DefaultDetailColor);

			WireUpForceUpdateSizeRequested(item, tvc, tv);

			UpdateIsEnabled(tvc, textCell);

			UpdateBackground(tvc, item);

			return tvc;
		}

		protected virtual void HandlePropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			var tvc = (CellTableViewCell)sender;
			var textCell = (TextCell)tvc.Cell;
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
				tvc.TextLabel.TextColor = textCell.TextColor.ToUIColor(DefaultTextColor);
			else if (args.PropertyName == TextCell.DetailColorProperty.PropertyName)
				tvc.DetailTextLabel.TextColor = textCell.DetailColor.ToUIColor(DefaultTextColor);
			else if (args.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled(tvc, textCell);
		}

		static void UpdateIsEnabled(CellTableViewCell cell, TextCell entryCell)
		{
			cell.UserInteractionEnabled = entryCell.IsEnabled;
			cell.TextLabel.Enabled = entryCell.IsEnabled;
			cell.DetailTextLabel.Enabled = entryCell.IsEnabled;
		}
	}
}