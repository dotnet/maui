using CoreAnimation;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using UIKit;



namespace Maui.Controls.Sample.iOS
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate<Startup>
	{
		IAdornerService _adornerService;
		Microsoft.Maui.ILayout MainLayout => (CurrentWindow.Page.View as ScrollView).Content as Microsoft.Maui.ILayout;
		CALayer _parent;

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			var launching = base.FinishedLaunching(application, launchOptions);

			_adornerService = Current.Services.GetService<IAdornerService>();
			_parent = _adornerService.GetAdornerLayer();

			_adornerService?.HandlePoint(point =>
			{
				_adornerService?.ClearAdorners();
				AddAdornerAtPoint(MainLayout, point);
			});

			Device.StartTimer(System.TimeSpan.FromSeconds(3), () =>
			{
				AddAdornerAtPoint(MainLayout, new Point(2, 2));
				return false;
			});

			return launching;
		}

		void AddAdornerAtPoint(Microsoft.Maui.ILayout layout, Point point)
		{
			var view = _adornerService?.GetViewAtPoint(layout, point);
			if (view == null)
				return;

			var frame = view.Frame;

			var statusBarHeight = UIApplication.SharedApplication.StatusBarFrame.Size.Height;

			var cgRect = new CoreGraphics.CGRect(frame.Location.X, frame.Location.Y + statusBarHeight, frame.Size.Width, frame.Size.Height);

			CALayer nativeLayer = new CAShapeLayer
			{
				Path = UIBezierPath.FromRect(cgRect).CGPath,
				FillColor = UIColor.Clear.CGColor,
				StrokeColor = UIColor.Red.CGColor,
				LineWidth = 2.0f
			};

			_parent.AddSublayer(nativeLayer);
		}
	}
}