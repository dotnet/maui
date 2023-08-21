//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using AppKit;
using Foundation;
using GraphicsTester.Scenarios;
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
