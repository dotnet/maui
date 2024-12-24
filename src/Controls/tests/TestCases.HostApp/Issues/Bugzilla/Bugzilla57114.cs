using Microsoft.Maui.Handlers;
namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 57114, "Forms gestures are not supported on UIViews that have native gestures", PlatformAffected.iOS)]
	public class Bugzilla57114 : TestContentPage
	{
		public static string _57114NativeGestureFiredMessage = "_57114NativeGestureFiredMessage";

		Label _results;
		bool _nativeGestureFired;
		bool _formsGestureFired;

		const string Testing = "Testing...";
		const string Success = "Success";
		const string ViewAutomationId = "_57114View";

		protected override void Init()
		{
			var instructions = new Label
			{
				Text = $"Tap the Aqua View below. If the label below changes from '{Testing}' to '{Success}', the test has passed."
			};

			_results = new Label { Text = Testing };

			var view = new _57114View
			{
				AutomationId = ViewAutomationId,
				HeightRequest = 200,
				WidthRequest = 200,
				BackgroundColor = Colors.Aqua,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill
			};

			var tap = new TapGestureRecognizer
			{
				Command = new Command(() =>
				{
					_formsGestureFired = true;
					UpdateResults();
				})
			};

#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<object>(this, _57114NativeGestureFiredMessage, NativeGestureFired);
#pragma warning restore CS0618 // Type or member is obsolete

			view.GestureRecognizers.Add(tap);

			var layout = new StackLayout()
			{
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				Children =
				{
					instructions, _results, view
				}
			};

			Content = layout;
		}

		void NativeGestureFired(object obj)
		{
			_nativeGestureFired = true;
			UpdateResults();
		}

		void UpdateResults()
		{
			if (_nativeGestureFired && _formsGestureFired)
			{
				_results.Text = Success;
			}
			else
			{
				_results.Text = Testing;
			}
		}
	}

	public class _57114View : View
	{

	}
#if ANDROID
	public class _57114ViewHandler : ViewHandler<_57114View, Android.Views.View>
	{
		public _57114ViewHandler() : base(ViewHandler.ViewMapper, ViewHandler.ViewCommandMapper)
		{
		}

		protected override Android.Views.View CreatePlatformView()
		{
			Android.Views.View view = new Android.Views.View(Context);
			//view.Touch += OnTouch;
			return view;
		}

		private void OnTouch(object sender, Android.Views.View.TouchEventArgs e)
		{
			MessagingCenter.Send(this as object, Bugzilla57114._57114NativeGestureFiredMessage);
			e.Handled = false;
		}
	}
#elif IOS || MACCATALYST

	public class _57114ViewHandler : ViewHandler<_57114View, UIKit.UIView>
    {
        public static IPropertyMapper<_57114View, _57114ViewHandler> Mapper = new PropertyMapper<_57114View, _57114ViewHandler>(ViewHandler.ViewMapper)
        {
            
        };

        public _57114ViewHandler() : base(Mapper)
        {
        }

        protected override UIKit.UIView CreatePlatformView()
        {
            var view = new UIKit.UIView();
            var rec = new CustomGestureRecognizer();
            view.AddGestureRecognizer(rec);
            return view;
        }
    }

    public class CustomGestureRecognizer : UIKit.UIGestureRecognizer
    {
        public override void TouchesBegan(Foundation.NSSet touches, UIKit.UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
#pragma warning disable CS0618 // Type or member is obsolete
            MessagingCenter.Instance.Send(this as object, Bugzilla57114._57114NativeGestureFiredMessage);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }

#elif WINDOWS
	public class _57114ViewHandler : ViewHandler<_57114View, Microsoft.UI.Xaml.Controls.Grid>
    {
        public static IPropertyMapper<_57114View, _57114ViewHandler> Mapper = new PropertyMapper<_57114View, _57114ViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(_57114View.BackgroundColor)] = MapBackgroundColor
        };

        public _57114ViewHandler() : base(Mapper)
        {
        }

        protected override Microsoft.UI.Xaml.Controls.Grid CreatePlatformView()
        {
			Microsoft.UI.Xaml.Controls.Grid nativeView = new Microsoft.UI.Xaml.Controls.Grid();
			nativeView.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock() { Text = "_57114View" });
            nativeView.Tapped += OnTapped;
            return nativeView;
        }

        public static void MapBackgroundColor(_57114ViewHandler handler, _57114View view)
        {
            if (handler.PlatformView != null)
            {
                handler.PlatformView.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(global::Windows.UI.Color.FromArgb(
                    (byte)(view.BackgroundColor.Alpha * 255),
                    (byte)(view.BackgroundColor.Red * 255),
                    (byte)(view.BackgroundColor.Green * 255),
                    (byte)(view.BackgroundColor.Blue * 255)));
            }
        }

        private void OnTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs tappedRoutedEventArgs)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            MessagingCenter.Send(this as object, Bugzilla57114._57114NativeGestureFiredMessage);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
#endif

}