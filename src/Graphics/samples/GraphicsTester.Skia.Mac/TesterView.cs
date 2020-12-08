using System.Graphics;
using System.Graphics.CoreGraphics;
using System.Graphics.Skia.Views;
using AppKit;
using CoreGraphics;

namespace GraphicsTester.Skia {

    public class TesterViewController : NSViewController {
        public override void LoadView ()
        {
            View = new TesterView ();
        }

        public override void ViewDidAppear()
        {
            base.ViewDidAppear();
            var frame = View.Window.Frame;
            frame.Size = new CGSize(1024, 1024);
            View.Window.SetFrame(frame, false);
        }
    }

    public class TesterView : NSView {
        private readonly NSTableView tableView;
        private readonly SkiaGraphicsView graphicsView;
        private readonly TesterTableViewSource tableSource;

        public TesterView () : base ()
        {
            GraphicsPlatform.Register (NativeGraphicsService.Instance);

            tableSource = new TesterTableViewSource ();
            tableSource.ScenarioSelected += (drawable) => {
                graphicsView.Drawable = drawable;
            };

            tableView = new NSTableView ();
            tableView.AddColumn (new NSTableColumn () {
                Width = 300,
            });
            tableView.Source = tableSource;
            //tableView.BackgroundColor = NSColor.White;

            AddSubview (tableView);

            graphicsView = new SkiaGraphicsView ();
            AddSubview (graphicsView);

            Layout ();

            tableView.SelectRow (0, false);
        }

        public override bool IsFlipped => true;

        public override void Layout ()
        {
            var bounds = Bounds;
            tableView.Frame = new CGRect (0, 0, 300, bounds.Height);
            graphicsView.Frame = new CGRect (300, 0, bounds.Width - 300, bounds.Height);
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

