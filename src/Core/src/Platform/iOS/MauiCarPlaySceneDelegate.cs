#if IOS && !MACCATALYST
using System;
using CarPlay;
using Foundation;
using Microsoft.Maui.LifecycleEvents;
using UIKit;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides a base scene delegate for CarPlay integration in .NET MAUI applications.
	/// </summary>
	[System.Runtime.Versioning.SupportedOSPlatform("ios14.0")]
	[Register("MauiCarPlaySceneDelegate")]
	public class MauiCarPlaySceneDelegate : UIResponder, ICPTemplateApplicationSceneDelegate
	{
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "CPInterfaceController is managed by CarPlay framework lifecycle.")]
		CPInterfaceController? _interfaceController;

		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "CPWindow is managed by CarPlay framework lifecycle.")]
		CPWindow? _carPlayWindow;

		/// <summary>
		/// Gets the CarPlay interface controller for managing the template hierarchy.
		/// </summary>
		public CPInterfaceController? InterfaceController => _interfaceController;

		/// <summary>
		/// Gets the CarPlay window for rendering custom content (navigation apps only).
		/// </summary>
		public CPWindow? CarPlayWindow => _carPlayWindow;

		/// <summary>
		/// Called when the CarPlay scene connects and a drawable <see cref="CPWindow"/> is provided.
		/// This overload is only invoked for navigation apps that declare the
		/// <c>com.apple.developer.carplay-maps</c> entitlement, since only those apps receive a window
		/// they can render custom content into.
		/// </summary>
		/// <param name="templateApplicationScene">The CarPlay scene that connected.</param>
		/// <param name="interfaceController">The interface controller used to present CarPlay templates.</param>
		/// <param name="window">The CarPlay window navigation apps can draw custom content into.</param>
		[Export("templateApplicationScene:didConnectInterfaceController:toWindow:")]
		public virtual void DidConnect(CPTemplateApplicationScene templateApplicationScene, CPInterfaceController interfaceController, CPWindow window)
		{
			_interfaceController = interfaceController;
			_carPlayWindow = window;

			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.CarPlayDidConnect>(
				del => del(templateApplicationScene, interfaceController));
		}

		/// <summary>
		/// Called when the CarPlay scene connects for template-based apps that do not receive a
		/// <see cref="CPWindow"/>, such as audio, communication, EV charging, parking, and quick food
		/// ordering apps.
		/// </summary>
		/// <param name="templateApplicationScene">The CarPlay scene that connected.</param>
		/// <param name="interfaceController">The interface controller used to present CarPlay templates.</param>
		[Export("templateApplicationScene:didConnectInterfaceController:")]
		public virtual void DidConnect(CPTemplateApplicationScene templateApplicationScene, CPInterfaceController interfaceController)
		{
			_interfaceController = interfaceController;

			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.CarPlayDidConnect>(
				del => del(templateApplicationScene, interfaceController));
		}

		/// <summary>
		/// Called when the CarPlay scene disconnects, for example when the user disconnects the device
		/// from CarPlay or the app is moved to the background. Clears the cached
		/// <see cref="InterfaceController"/> and <see cref="CarPlayWindow"/> references.
		/// </summary>
		/// <param name="templateApplicationScene">The CarPlay scene that disconnected.</param>
		/// <param name="interfaceController">The interface controller that was used to present CarPlay templates.</param>
		[Export("templateApplicationScene:didDisconnectInterfaceController:")]
		public virtual void DidDisconnect(CPTemplateApplicationScene templateApplicationScene, CPInterfaceController interfaceController)
		{
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.CarPlayDidDisconnect>(
				del => del(templateApplicationScene, interfaceController));

			_interfaceController = null;
			_carPlayWindow = null;
		}

		/// <summary>
		/// Called when the CarPlay scene disconnects for navigation apps that were connected with a
		/// drawable <see cref="CPWindow"/>. Mirrors <see cref="DidConnect(CPTemplateApplicationScene, CPInterfaceController, CPWindow)"/>
		/// for symmetry and clears the cached <see cref="InterfaceController"/> and <see cref="CarPlayWindow"/> references.
		/// </summary>
		/// <param name="templateApplicationScene">The CarPlay scene that disconnected.</param>
		/// <param name="interfaceController">The interface controller that was used to present CarPlay templates.</param>
		/// <param name="window">The CarPlay window that was provided to the navigation app when it connected.</param>
		[Export("templateApplicationScene:didDisconnectInterfaceController:fromWindow:")]
		public virtual void DidDisconnect(CPTemplateApplicationScene templateApplicationScene, CPInterfaceController interfaceController, CPWindow window)
		{
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.CarPlayDidDisconnect>(
				del => del(templateApplicationScene, interfaceController));

			_interfaceController = null;
			_carPlayWindow = null;
		}

		/// <summary>
		/// Called when the user selects a navigation maneuver from the CarPlay maneuver template. This
		/// is only invoked for navigation apps that display turn-by-turn maneuver guidance.
		/// </summary>
		/// <param name="templateApplicationScene">The CarPlay scene raising the event.</param>
		/// <param name="maneuver">The maneuver the user selected.</param>
		[Export("templateApplicationScene:didSelectManeuver:")]
		public virtual void DidSelectManeuver(CPTemplateApplicationScene templateApplicationScene, CPManeuver maneuver)
		{
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.CarPlayDidSelectManeuver>(
				del => del(templateApplicationScene, maneuver));
		}

		/// <summary>
		/// Called when the user selects a navigation alert banner in CarPlay. This is only invoked for
		/// navigation apps that display alert or warning banners to the driver.
		/// </summary>
		/// <param name="templateApplicationScene">The CarPlay scene raising the event.</param>
		/// <param name="navigationAlert">The navigation alert the user selected.</param>
		[Export("templateApplicationScene:didSelectNavigationAlert:")]
		public virtual void DidSelectNavigationAlert(CPTemplateApplicationScene templateApplicationScene, CPNavigationAlert navigationAlert)
		{
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.CarPlayDidSelectNavigationAlert>(
				del => del(templateApplicationScene, navigationAlert));
		}
	}
}
#endif
