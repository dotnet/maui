using System;
using AppKit;
using GraphicsTester.Scenarios;
using Foundation;
using Microsoft.Maui.Graphics;

namespace GraphicsTester.Mac
{
	public class TesterTableViewSource : NSTableViewSource
	{
		public event Action<IDrawable> ScenarioSelected;

		public override NSObject GetObjectValue(NSTableView tableView, NSTableColumn tableColumn, nint row)
		{
			return new NSString(ScenarioList.Scenarios[(int)row].ToString());
		}

		public override nint GetRowCount(NSTableView tableView)
		{
			return ScenarioList.Scenarios.Count;
		}
#pragma warning disable CA1416, CA1422
		public override void SelectionDidChange(NSNotification notification)
		{
			if (ScenarioSelected != null)
			{
				var tableView = notification.Object as NSTableView;
				if (tableView != null)
				{
					var row = tableView.SelectedRow;
					if (row >= 0)
					{
						var scenario = ScenarioList.Scenarios[(int)row];
						ScenarioSelected(scenario);
					}
				}
			}
		}
#pragma warning restore CA1416, CA1422
	}
}
