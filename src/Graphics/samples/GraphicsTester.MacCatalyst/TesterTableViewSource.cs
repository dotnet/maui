using System;
using Microsoft.Maui.Graphics;
using Foundation;
using GraphicsTester.Scenarios;
using UIKit;

namespace GraphicsTester.iOS
{
	public class TesterTableViewSource : UITableViewSource
	{
		public event Action<IDrawable> ScenarioSelected;

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell("cell");

			if (cell == null)
			{
				cell = new UITableViewCell(UITableViewCellStyle.Default, "cell");
			}

#pragma warning disable CA1416, CA1422 // Validate platform compatibility
			cell.TextLabel.Text = ScenarioList.Scenarios[indexPath.Row].ToString();
#pragma warning restore CA1416, CA1422 // Validate platform compatibility

			return cell;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return ScenarioList.Scenarios.Count;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			if (ScenarioSelected != null)
			{
				if (tableView != null)
				{
					var row = indexPath.Row;

					if (row >= 0)
					{
						var scenario = ScenarioList.Scenarios[row];
						ScenarioSelected(scenario);
					}
				}
			}
		}
	}
}
