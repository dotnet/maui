using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18172, "Shadows not drawing/updating correctly in Windows & cover entire screen", PlatformAffected.UWP)]
public class Issue18172 : TestContentPage
{
	protected override void Init()
	{
		Title = "Issue 18172";

		BackgroundColor = Colors.LightPink;

		ScreenSizeMonitor.Instance.setContentPage(this);

		VerticalStackLayout verticalStackLayout = new VerticalStackLayout();
		Content = verticalStackLayout;

		AbsoluteLayout absoluteLayout = new AbsoluteLayout();
		verticalStackLayout.Add(absoluteLayout);
		absoluteLayout.Add(TopObjectWithShadow.Instance);
	}

	class TopObjectWithShadow : AbsoluteLayout
	{
		public static TopObjectWithShadow Instance { get { return lazy.Value; } }
		static readonly Lazy<TopObjectWithShadow> lazy = new Lazy<TopObjectWithShadow>(() => new TopObjectWithShadow());

		public TopSubComponent topMenuFrameComponent;

		private TopObjectWithShadow()
		{
			IgnoreSafeArea = true;

			topMenuFrameComponent = new TopSubComponent();
			Add(topMenuFrameComponent);

			topMenuFrameComponent.Shadow = new Shadow()
			{
				Offset = new Point(0, 5),
				Radius = 5
			};

			ScreenSizeMonitor.Instance.ScreenSizeChanged += OnScreenSizeChanged;
		}

		void OnScreenSizeChanged()
		{
			if (topMenuFrameComponent != null)
			{
				WidthRequest = ScreenSizeMonitor.Instance.screenWidth;
				HeightRequest = ScreenSizeMonitor.Instance.screenHeight;

				topMenuFrameComponent.resizeFunction();
			}
		}
	}

	class TopSubComponent : AbsoluteLayout
	{
		readonly Border _frameBorder;
		readonly double _frameBaseHeight = 68;

		public TopSubComponent()
		{
			_frameBorder = new Border
			{
				AutomationId = "BorderControl"
			};

			Add(_frameBorder);
			IgnoreSafeArea = true;
			_frameBorder.HeightRequest = _frameBaseHeight;
			_frameBorder.BackgroundColor = Colors.White;
			_frameBorder.Margin = new Thickness(0, 20, 0, 0);
			_frameBorder.StrokeShape = new RoundRectangle() { CornerRadius = new CornerRadius(30, 30, 30, 30) };
			_frameBorder.StrokeThickness = 0;
		}
		public void resizeFunction()
		{
			WidthRequest = ScreenSizeMonitor.Instance.screenWidth;
			HeightRequest = ScreenSizeMonitor.Instance.screenHeight;

			_frameBorder.WidthRequest = ScreenSizeMonitor.Instance.screenWidth;
			_frameBorder.HeightRequest = _frameBaseHeight;
		}
	}

	class ScreenSizeMonitor
	{
		public ContentPage pageToMonitor;

		private static readonly Lazy<ScreenSizeMonitor> lazy = new Lazy<ScreenSizeMonitor>(() => new ScreenSizeMonitor());
		public static ScreenSizeMonitor Instance { get { return lazy.Value; } }

		public double screenWidth = 0;
		public double screenHeight = 0;
		public event Action ScreenSizeChanged = null;

		public void setContentPage(ContentPage contentPageToMonitor)
		{
			pageToMonitor = contentPageToMonitor;
			StartScreenMonitor();
		}

		void StartScreenMonitor()
		{
			UpdateFunction();

			pageToMonitor.SizeChanged += delegate
			{
				UpdateFunction();
			};
		}

		void UpdateFunction()
		{
			if (pageToMonitor.Width > 0 && pageToMonitor.Height > 0)
			{
				screenWidth = pageToMonitor.Width;
				screenHeight = pageToMonitor.Height;

				invokeScreenSizeChanged();
			}
		}
		public void invokeScreenSizeChanged()
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				ScreenSizeChanged?.Invoke();
			});
		}
	}
}
