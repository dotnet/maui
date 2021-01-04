using System;
using System.Graphics;
using System.Graphics.CoreGraphics;
using CoreGraphics;
using Foundation;
using GraphicsTester.Scenarios;
using UIKit;

namespace GraphicsTester.iOS
{
    public class TesterView : UIView
    {
        nfloat TableViewWidth = 250;

        readonly UITableView tableView;
        readonly NativeGraphicsView graphicsView;
        readonly TesterTableViewSource tableSource;

        public TesterView(CGRect bounds) : base()
        {
            Frame = bounds;

            GraphicsPlatform.Register(NativeGraphicsService.Instance);

            tableSource = new TesterTableViewSource();

            tableSource.ScenarioSelected += (drawable) =>
            {
                graphicsView.Drawable = drawable;
                graphicsView.InvalidateDrawable();
            };

            tableView = new UITableView
            {
                AllowsSelection = true,
                Source = tableSource
            };

            AddSubview(tableView);

            graphicsView = new NativeGraphicsView
            {
                Drawable = ScenarioList.Scenarios[0]
            };

            AddSubview(graphicsView);

            tableView.SelectRow(NSIndexPath.FromRowSection(0, 0), false, UITableViewScrollPosition.None);
        }

        public override CGRect Frame
        {
            get => base.Frame;
            set
            {
                base.Frame = value;
                LayoutSubviews();
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (tableView == null || graphicsView == null)
                return;

            tableView.Frame = new CGRect(0, 0, TableViewWidth, Bounds.Height);
            graphicsView.Frame = new CGRect(TableViewWidth, 0, Bounds.Width - TableViewWidth, Bounds.Height);
            graphicsView.InvalidateDrawable();
        }
    }
}