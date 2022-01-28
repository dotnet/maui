using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Controls.Compatibility
{
	public struct InitializationOptions
	{
		public InitializationOptions(UI.Xaml.LaunchActivatedEventArgs args)
		{
			this = default(InitializationOptions);
			LaunchActivatedEventArgs = args;
		}
		public UI.Xaml.LaunchActivatedEventArgs LaunchActivatedEventArgs;
		public InitializationFlags Flags;
	}

	public static partial class Forms
	{
		//TODO WINUI3 This is set by main page currently because
		// it's only a single window
		public static UI.Xaml.Window MainWindow { get; set; }

		public static bool IsInitialized { get; private set; }

		public static IMauiContext MauiContext { get; private set; }

		public static void Init(IActivationState state, InitializationOptions? options = null)
		{
			SetupInit(state.Context, state.Context.GetOptionalNativeWindow(), maybeOptions: options);
		}

		static void SetupInit(
			IMauiContext mauiContext,
			UI.Xaml.Window mainWindow,
			IEnumerable<Assembly> rendererAssemblies = null,
			InitializationOptions? maybeOptions = null)
		{
			MauiContext = mauiContext;
			Registrar.RegisterRendererToHandlerShim(RendererToHandlerShim.CreateShim);

			var accent = (WSolidColorBrush)Microsoft.UI.Xaml.Application.Current.Resources["SystemColorControlAccentBrush"];
			KnownColor.SetAccent(accent.ToColor());

			switch (UserInteractionMode)
			{
				case UserInteractionModeEnum.Mouse:
					Device.SetIdiom(TargetIdiom.Desktop);
					break;
				case UserInteractionModeEnum.Touch:
					Device.SetIdiom(TargetIdiom.Tablet);
					break;
				default:
					Device.SetIdiom(TargetIdiom.Unsupported);
					break;
			}

			Device.SetFlowDirection(mauiContext.GetFlowDirection());

			ExpressionSearch.Default = new WindowsExpressionSearch();

			Registrar.ExtraAssemblies = rendererAssemblies?.ToArray();

			var platformServices = new WindowsPlatformServices();

			Device.PlatformServices = platformServices;
			Device.PlatformInvalidator = platformServices;

			if (mainWindow != null)
			{
				MainWindow = mainWindow;
			}

			IsInitialized = true;
		}

		public enum UserInteractionModeEnum { Touch, Mouse };

		public static UserInteractionModeEnum UserInteractionMode
		{
			get
			{
				UserInteractionModeEnum userInteractionMode = UserInteractionModeEnum.Mouse;

				int SM_CONVERTIBLESLATEMODE = 0x2003;
				int SM_TABLETPC = 0x56;

				bool isTouchMode = GetSystemMetrics(SM_CONVERTIBLESLATEMODE) == 0; // Tablet Mode
				bool isTabletPC = GetSystemMetrics(SM_TABLETPC) != 0;// Tablet PC

				if (isTouchMode && isTabletPC)
				{
					userInteractionMode = UserInteractionModeEnum.Touch;
				}

				return userInteractionMode;
			}
		}

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "GetSystemMetrics")]
		private static extern int GetSystemMetrics(int nIndex);
	}
}