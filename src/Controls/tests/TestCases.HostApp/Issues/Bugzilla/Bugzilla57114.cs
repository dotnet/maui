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
			return view;
		}

		protected override void ConnectHandler(Android.Views.View platformView)
		{
			base.ConnectHandler(platformView);
			if (PlatformView is Android.Views.View view)
			{
				view.Touch += OnTouch;
			}
		}

		protected override void DisconnectHandler(Android.Views.View platformView)
		{
			base.DisconnectHandler(platformView);
			if (PlatformView is Android.Views.View view)
			{
				view.Touch -= OnTouch;
			}
		}

		private void OnTouch(object sender, Android.Views.View.TouchEventArgs e)
		{
#pragma warning disable CS0618 // Type or member is obsolete
            MessagingCenter.Instance.Send(this as object, Bugzilla57114._57114NativeGestureFiredMessage);
#pragma warning restore CS0618 // Type or member is obsolete
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
        };

        public _57114ViewHandler() : base(Mapper)
        {
        }

        protected override Microsoft.UI.Xaml.Controls.Grid CreatePlatformView()
        {
			Microsoft.UI.Xaml.Controls.Grid nativeView = new Microsoft.UI.Xaml.Controls.Grid() { Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.AliceBlue) };
			nativeView.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock() { Text = "_57114View" });
            return nativeView;
        }

		protected override void ConnectHandler(Microsoft.UI.Xaml.Controls.Grid platformView)
		{
			base.ConnectHandler(platformView);
			if (PlatformView is Microsoft.UI.Xaml.Controls.Grid view)
			{
				view.Tapped += OnTapped;
			}
		}

		protected override void DisconnectHandler(Microsoft.UI.Xaml.Controls.Grid platformView)
		{
			base.DisconnectHandler(platformView);
			if (PlatformView is Microsoft.UI.Xaml.Controls.Grid view)
			{
				view.Tapped -= OnTapped;
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