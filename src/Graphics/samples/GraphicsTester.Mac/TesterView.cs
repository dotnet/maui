using System;
using System.IO;
using System.Text;
using AppKit;
using CoreGraphics;
using System.Graphics;
using Foundation;
using SkiaSharp;
using WebKit;
using SkiaSharp.Views.Mac;

namespace GraphicsTester.Mac
{
   public class TesterView : NSView
   {
      private readonly NSImageView _imageView;
      private readonly NSTabView _tabView;
      private readonly NSTableView _tableView;
      private readonly NSScrollView _tableViewScrollView;
      private readonly WebView _webView;
      private readonly NSTextView _textView;
      private readonly MMGraphicsView _graphicsView;
      private readonly MMGraphicsView _sketchView;
      private readonly SKCanvasView _skiaCanvasView;
      private readonly SKGLView _skiaGlView;

      private EWDrawable _drawable;

      public TesterView(CGRect rect) : base(rect)
      {
         GraphicsPlatform.Register(MMGraphicsService.Instance);
         ServiceContainer.Register<IFontService>(MMFontService.Instance);
         ServiceContainer.Register<IPdfImageService>(MMPdfImageService.Instance);
         ServiceContainer.Register<IPdfRenderService>(MMPdfRenderService.Instance);
         ServiceContainer.Register<IPdfExportService>(MMPdfExportService.Instance);
         
         var tableSource = new TesterTableViewSource();
         tableSource.ScenarioSelected += (drawable) =>
         {
            _graphicsView.Drawable = drawable;
            _graphicsView.InvalidateDrawable();

            _sketchView.Drawable = drawable;
            _sketchView.InvalidateDrawable();

            CreateSvg(drawable);
            CreateImage(drawable);

            _drawable = drawable;
            _skiaCanvasView.NeedsDisplay = true;
            _skiaGlView.NeedsDisplay = true;
         };

         _tableView = new NSTableView(new CGRect(0, 0, 300, rect.Height));
         _tableView.AddColumn(new NSTableColumn()
         {
            Width = 300,
         });
         _tableView.Source = tableSource;
         _tableView.BackgroundColor = NSColor.White;
         _tableView.HeaderView = null;

         _tableViewScrollView = new NSScrollView(new CGRect(0, 0, 300, rect.Height)) {DocumentView = _tableView};

         // ReSharper disable once VirtualMemberCallInConstructor
         AddSubview(_tableViewScrollView);

         _graphicsView = new MMGraphicsView {BackgroundColor = StandardColors.White};

         _sketchView = new MMGraphicsView {Renderer = new MMSketchRenderer(), BackgroundColor = StandardColors.White};

         _imageView = new NSImageView {AutoresizingMask = NSViewResizingMask.WidthSizable};

         _webView = new WebView();
         _textView = new NSTextView
         {
            VerticallyResizable = true,
            HorizontallyResizable = false,
            AutoresizingMask = NSViewResizingMask.WidthSizable
         };

         _skiaCanvasView = new SKCanvasView {IgnorePixelScaling = true};
         _skiaCanvasView.PaintSurface += SkiaCanvasViewOnPaintSurface;

         _skiaGlView = new SKGLView();
         _skiaCanvasView.IgnorePixelScaling = true;
         _skiaGlView.PaintSurface += SkiaGlViewOnPaintSurface;

         var scrollView = new NSScrollView {HasVerticalScroller = true, DocumentView = _textView};

         _tabView = new NSTabView();
         // ReSharper disable once VirtualMemberCallInConstructor
         AddSubview(_tabView);

         var item1 = new NSTabViewItem {View = _graphicsView, Label = "Native"};
         _tabView.Add(item1);

         var item2 = new NSTabViewItem {View = _webView, Label = "SVG"};
         _tabView.Add(item2);

         var item3 = new NSTabViewItem {View = scrollView, Label = "SVG Source"};
         _tabView.Add(item3);

         var item4 = new NSTabViewItem {View = _sketchView, Label = "Sketch"};
         _tabView.Add(item4);

         var item5 = new NSTabViewItem {View = _skiaCanvasView, Label = "Skia Canvas"};
         _tabView.Add(item5);

         var item6 = new NSTabViewItem {View = _skiaGlView, Label = "Skia GL"};
         _tabView.Add(item6);

         var item7 = new NSTabViewItem {View = _imageView, Label = "NSImage"};
         _tabView.Add(item7);

         _tableView.SelectRow(0, false);
      }

      private void SkiaGlViewOnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
      {
         try
         {
            e.Surface.Canvas.Clear(SKColors.White);
            var canvas = new SkiaCanvas {Canvas = e.Surface.Canvas};
            canvas.PixelShift(.5f, .5f);
            var bounds = _skiaGlView.Bounds;
            _drawable.Draw(canvas, bounds.AsEWRectangle());
         }
         catch (Exception exception)
         {
            Logger.Debug(exception);
         }
      }

      private void SkiaCanvasViewOnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
      {
         try
         {
            e.Surface.Canvas.Clear(SKColors.White);
            var canvas = new SkiaCanvas {Canvas = e.Surface.Canvas};
            canvas.PixelShift(.5f, .5f);
            var bounds = _skiaCanvasView.Bounds;
            _drawable.Draw(canvas, bounds.AsEWRectangle());
         }
         catch (Exception exception)
         {
            Logger.Debug(exception);
         }
      }

      void CreateSvg(EWDrawable drawable)
      {
         var width = (float)Math.Max(_webView.Bounds.Width, _graphicsView.Bounds.Width);
         var height = (float)Math.Max(_webView.Bounds.Height, _graphicsView.Bounds.Height);

         using (var canvas = new SVGCanvas(new EWRectangle(0, 0, width, height), true))
         {
            drawable.Draw(canvas, new EWRectangle(float.MinValue / 2, float.MinValue / 2, float.MaxValue, float.MaxValue));
            using (var stream = new MemoryStream())
            {
               canvas.Write(stream);
               var bytes = stream.ToArray();
               _webView.MainFrame.LoadData(NSData.FromArray(bytes), "image/svg+xml", "", new NSUrl("http://localhost"));

               var source = Encoding.UTF8.GetString(bytes);
               _textView.Value = source;
            }
         }
      }

      private void CreateImage(EWDrawable drawable)
      {
         var width = (float)Math.Max(_imageView.Bounds.Width, _imageView.Bounds.Width);
         var height = (float)Math.Max(_imageView.Bounds.Height, _imageView.Bounds.Height);

         if (width > 0 && height > 0)
         {
            var scale = (float)_imageView.ConvertRectToBacking(new CGRect(0, 0, 1, 1)).Width;

            using (var context = GraphicsPlatform.CurrentService.CreateBitmapExportContext((int)(width * scale), (int)(height * scale), scale))
            {
               var canvas = context.Canvas;
               canvas.Scale(scale, scale);
               canvas.FillColor = StandardColors.White;
               canvas.FillRectangle(0, 0, width, height);
               drawable.Draw(canvas, new EWRectangle(0, 0, width, height));
               _imageView.Image = context.Image.AsNSImage();
            }
         }
      }

      public override bool IsFlipped => true;

      public override void Layout()
      {
         var bounds = Bounds;
         _tableViewScrollView.Frame = new CGRect(0, 24, 300, bounds.Height - 24);
         _tabView.Frame = new CGRect(300, 24, bounds.Width - 300, bounds.Height - 24);
      }

      public override CGRect Frame
      {
         get => base.Frame;
         set
         {
            base.Frame = value;
            Layout();
         }
      }
   }
}

