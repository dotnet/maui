using System;
using Tizen.Applications;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;

namespace Microsoft.Maui.Platform
{
	public class CoreAppContext
	{
		static CoreAppContext? _instance = null;

		public static bool IsInitialized { get; private set; }

		public static CoreAppContext GetInstance(CoreApplication application, Window? window = null)
		{
			if (IsInitialized)
				return _instance!;

			_instance = (window == null) ? new CoreAppContext(application) : new CoreAppContext(application, window);
			return _instance;
		}

		public CoreApplication CurrentApplication { get; private set; }

		public string ResourceDir => CurrentApplication.DirectoryInfo.Resource;

		public Window MainWindow { get; set; }

		Layer? OverlayLayer { get; set; }

		protected CoreAppContext(CoreApplication application) : this(application, CreateDefaultWindow())
		{
		}

		protected CoreAppContext(CoreApplication application, Window window)
		{
			_ = application ?? throw new ArgumentNullException(nameof(application));
			_ = window ?? throw new ArgumentNullException(nameof(window));

			CurrentApplication = application;
			MainWindow = window;
			window.Initialize();
			window.SetWindowCloseRequestHandler(() => application.Exit());
			IsInitialized = true;
		}

		public void SetOverlay(View? content)
		{
			if (content == null)
				return;

			if (OverlayLayer == null)
			{
				OverlayLayer = new Layer();
				MainWindow.AddLayer(OverlayLayer);
				OverlayLayer.RaiseToTop();
			}
			content.WidthSpecification = LayoutParamPolicies.MatchParent;
			content.HeightSpecification = LayoutParamPolicies.MatchParent;
			OverlayLayer.Add(content);
		}

		static Window CreateDefaultWindow()
		{
			return Window.Instance;
		}
	}
}
