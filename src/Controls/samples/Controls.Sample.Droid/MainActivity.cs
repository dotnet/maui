using Android.App;
using Android.OS;
using Android.Widget;
using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Android.Views;

namespace Maui.Controls.Sample.Droid
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
	[IntentFilter(
		new[] { Microsoft.Maui.Essentials.Platform.Intent.ActionAppAction },
		Categories = new[] { Android.Content.Intent.CategoryDefault })]
	public class MainActivity : MauiAppCompatActivity
	{
		IAdornerService _adornerService;
		ViewGroupOverlay _parent;
		Microsoft.Maui.ILayout MainLayout => (CurrentWindow.Page.View as Microsoft.Maui.Controls.ScrollView).Content as Microsoft.Maui.ILayout;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			_adornerService = MauiApplication.Current.Services.GetService<IAdornerService>();
			_parent = (ViewGroupOverlay)_adornerService.GetAdornerLayer();

			_adornerService.HandlePoint(point =>
			{
			   _parent.Clear();
			   AddAdornerAtPoint(MainLayout, point);
			});

			Device.StartTimer(System.TimeSpan.FromSeconds(3), () =>
			{
				AddAdornerAtPoint(MainLayout, new Point(2, 2));
				return false;
			});
		}

		void AddAdornerAtPoint(Microsoft.Maui.ILayout layout, Point point)
		{
			var view = _adornerService.GetViewAtPoint(layout, point);
			if (view == null)
				return;

			var rect = view.Frame;

			Android.Views.View nativeView = GetAdorner(
												(int)this.ToPixels(rect.Location.X),
												(int)this.ToPixels(rect.Location.Y),
												(int)this.ToPixels(rect.Size.Width),
												(int)this.ToPixels(rect.Size.Height));
			
			_parent.Add(nativeView);
		}

		Android.Views.View GetAdorner(int x, int y, int width, int height)
		{
			System.Diagnostics.Debug.WriteLine($"Add Adorner {x} {y} {width} {height}");

			Android.Views.View native = new ImageView(this);
			var shape = new Android.Graphics.Drawables.GradientDrawable();
			shape.SetStroke(2, Color.Red.ToNative());
			(native as ImageView).SetImageDrawable(shape);

			//Microsoft.Maui.Controls.Shapes.Rectangle rectShape = new Microsoft.Maui.Controls.Shapes.Rectangle();
			//rectShape.Stroke = new SolidColorBrush(Color.Red);
			//rectShape.StrokeThickness = 2;
			//var native = rectShape.ToNative(this.CurrentWindow.MauiContext);

			native.Measure(Android.Views.View.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
			Android.Views.View.MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
			native.Layout(x, y, width, height);
			return native;
		}
	}
}