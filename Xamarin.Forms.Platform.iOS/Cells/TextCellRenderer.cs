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

			if (!(reusableCell is CellTableViewCell tvc))
				tvc = new CellTableViewCell(UITableViewCellStyle.Subtitle, item.GetType().FullName);
			else
				tvc.PropertyChanged -= HandleCellPropertyChanged;

			SetRealCell(item, tvc);

			tvc.Cell = textCell;
			tvc.PropertyChanged = HandleCellPropertyChanged;

			tvc.TextLabel.Text = textCell.Text;
			tvc.DetailTextLabel.Text = textCell.Detail;
			tvc.TextLabel.TextColor = textCell.TextColor.ToUIColor(DefaultTextColor);
			tvc.DetailTextLabel.TextColor = textCell.DetailColor.ToUIColor(DefaultDetailColor);

			WireUpForceUpdateSizeRequested(item, tvc, tv);

			UpdateIsEnabled(tvc, textCell);

			UpdateBackground(tvc, item);

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
				tvc.TextLabel.TextColor = textCell.TextColor.ToUIColor(DefaultTextColor);
			else if (args.PropertyName == TextCell.DetailColorProperty.PropertyName)
				tvc.DetailTextLabel.TextColor = textCell.DetailColor.ToUIColor(DefaultTextColor);
			else if (args.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled(tvc, textCell);

			HandlePropertyChanged(tvc, args);
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
	}
}
