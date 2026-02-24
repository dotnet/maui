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

		[Export("templateApplicationScene:didConnectInterfaceController:toWindow:")]
		public virtual void DidConnect(CPTemplateApplicationScene templateApplicationScene, CPInterfaceController interfaceController, CPWindow window)
		{
			_interfaceController = interfaceController;
			_carPlayWindow = window;

			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.CarPlayDidConnect>(
				del => del(templateApplicationScene, interfaceController));
		}

		[Export("templateApplicationScene:didConnectInterfaceController:")]
		public virtual void DidConnect(CPTemplateApplicationScene templateApplicationScene, CPInterfaceController interfaceController)
		{
			_interfaceController = interfaceController;

			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.CarPlayDidConnect>(
				del => del(templateApplicationScene, interfaceController));
		}

		[Export("templateApplicationScene:didDisconnectInterfaceController:")]
		public virtual void DidDisconnect(CPTemplateApplicationScene templateApplicationScene, CPInterfaceController interfaceController)
		{
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.CarPlayDidDisconnect>(
				del => del(templateApplicationScene, interfaceController));

			_interfaceController = null;
		}

		[Export("templateApplicationScene:didSelectManeuver:")]
		public virtual void DidSelectManeuver(CPTemplateApplicationScene templateApplicationScene, CPManeuver maneuver)
		{
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.CarPlayDidSelectManeuver>(
				del => del(templateApplicationScene, maneuver));
		}

		[Export("templateApplicationScene:didSelectNavigationAlert:")]
		public virtual void DidSelectNavigationAlert(CPTemplateApplicationScene templateApplicationScene, CPNavigationAlert navigationAlert)
		{
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.CarPlayDidSelectNavigationAlert>(
				del => del(templateApplicationScene, navigationAlert));
		}
	}
}
#endif
