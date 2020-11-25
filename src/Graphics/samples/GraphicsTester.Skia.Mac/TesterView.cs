using System.Graphics;
using System.Graphics.CoreGraphics;
using AppKit;
using CoreGraphics;

namespace GraphicsTester.Skia {

    public class TesterViewController : NSViewController {
        public override void LoadView ()
        {
            View = new TesterView ();
        }

    }

    public class TesterView : NSView {
        private readonly NSTableView tableView;
        private readonly NativeGraphicsView graphicsView;
        private readonly TesterTableViewSource tableSource;

        public TesterView () : base ()
        {
            GraphicsPlatform.Register (NativeGraphicsService.Instance);

            tableSource = new TesterTableViewSource ();
            tableSource.ScenarioSelected += (drawable) => {
                graphicsView.Drawable = drawable;
                graphicsView.InvalidateDrawable ();
            };

            tableView = new NSTableView ();
            tableView.AddColumn (new NSTableColumn () {
                Width = 300,
            });
            tableView.Source = tableSource;
            //tableView.BackgroundColor = NSColor.White;

            AddSubview (tableView);

            graphicsView = new NativeGraphicsView ();
            AddSubview (graphicsView);

            Layout ();

            tableView.SelectRow (0, false);
        }

        public override bool IsFlipped => true;

        public override void Layout ()
        {
            var bounds = Bounds;
            tableView.Frame = new CGRect (0, 24, 300, bounds.Height - 24);
            graphicsView.Frame = new CGRect (300, 24, bounds.Width - 300, bounds.Height - 24);
        }

        public override CGRect Frame {
            get => base.Frame;
            set {
                base.Frame = value;
                Layout ();
            }
        }
    }
}

