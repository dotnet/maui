using Tizen.Applications;
using ElmSharp;
using Embedding.XF;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;
using EButton = ElmSharp.Button;

namespace Embedding.Tizen
{
	class App : CoreUIApplication
	{
		Window _mainWindow;
		Naviframe _navi;
		EButton _showEmbedded;
		EButton _showAlertsActionSheets;
		EButton _showWebView;
		Page _currentPage;

		protected override void OnCreate()
		{
			base.OnCreate();
			_mainWindow = new Window("Embedding.Tizen")
			{
				AvailableRotations = DisplayRotation.Degree_0 | DisplayRotation.Degree_180 | DisplayRotation.Degree_270 | DisplayRotation.Degree_90
			};
			_mainWindow.Show();
			Forms.Init(this);
			Initialize();
		}

		void Initialize()
		{
			var conformant = new Conformant(_mainWindow);
			conformant.Show();

			_navi = new Naviframe(_mainWindow)
			{
				PreserveContentOnPop = true,
				DefaultBackButtonEnabled = true
			};
			_navi.Show();
			conformant.SetContent(_navi);

			var rootPage = CreateMainPage(_mainWindow);
			_navi.Push(rootPage);

			_mainWindow.BackButtonPressed += (sender, e) =>
			{
				bool handled = _currentPage?.SendBackButtonPressed() ?? false;

				if (!handled)
				{
					if (_navi.NavigationStack.Count == 1)
					{
						Exit();
					}
					_currentPage = null;
					_navi.Pop();
				}
			};

			_navi.Popped += (sender, e) =>
			{
				_currentPage = null;
			};
		}

		public EvasObject CreateMainPage(Window parent)
		{
			Box box = new Box(parent);
			box.Show();

			_showEmbedded = new EButton(parent)
			{
				Text = "Show Embedded Page",
				WeightX = 1,
				AlignmentX = -1,
			};
			_showEmbedded.Show();
			_showEmbedded.Clicked += (sender, e) => ShowHello();

			_showAlertsActionSheets = new EButton(parent)
			{
				Text = "Show Alerts and ActionSheets Page",
				WeightX = 1,
				AlignmentX = -1,
			};
			_showAlertsActionSheets.Show();
			_showAlertsActionSheets.Clicked += (sender, e) => ShowAlertsAndActionSheets();

			_showWebView = new EButton(parent)
			{
				Text = "Show WebView Page",
				WeightX = 1,
				AlignmentX = -1,
			};
			_showWebView.Show();
			_showWebView.Clicked += (sender, e) => ShowWebView();

			box.PackEnd(_showEmbedded);
			box.PackEnd(_showAlertsActionSheets);
			box.PackEnd(_showWebView);
			return box;
		}

		public void ShowHello()
		{
			_currentPage = new Hello();
			_navi.Push(_currentPage.CreateEvasObject(_mainWindow));
		}

		public void ShowAlertsAndActionSheets()
		{
			_currentPage = new AlertsAndActionSheets();
			_navi.Push(_currentPage.CreateEvasObject(_mainWindow));
		}

		public void ShowWebView()
		{
			_currentPage = new WebViewExample();
			_navi.Push(_currentPage.CreateEvasObject(_mainWindow));
		}

		static void Main(string[] args)
		{
			Elementary.Initialize();
			Elementary.ThemeOverlay();
			App app = new App();
			app.Run(args);
		}
	}
}
