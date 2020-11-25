using System;
using AppKit;
using GraphicsTester.Scenarios;
using Foundation;
using System.Graphics;

namespace GraphicsTester.Mac
{
    public class TesterTableViewSource : NSTableViewSource
    {
        public event Action<EWDrawable> ScenarioSelected;

        public override NSObject GetObjectValue (NSTableView tableView, NSTableColumn tableColumn, nint row)
        {
            return new NSString ((string)ScenarioList.Scenarios [(int)row].ToString());
        }

        public override nint GetRowCount (NSTableView tableView)
        {
            return ScenarioList.Scenarios.Count;
        }

        public override void SelectionDidChange (NSNotification notification)
        {
            if (ScenarioSelected != null)
            {
                if (notification.Object is NSTableView tableView)
                {
                    var row = tableView.SelectedRow;
                    if (row >= 0)
                    {
                        var scenario = ScenarioList.Scenarios [(int)row];
                        ScenarioSelected (scenario);
                    }
                }
            }
        }
    }
}

