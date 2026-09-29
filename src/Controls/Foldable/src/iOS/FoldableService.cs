#if IOS_DUO_BINDINGS
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Foldable
{
	class FoldableService : IFoldableService, IDisposable
	{
		static readonly UIViewReservedRegionKind DivisionRegionKind = UIViewReservedRegionKind.CreateDivision();
		readonly WeakEventManager _hingeAngleChangedEventManager = new WeakEventManager();
		readonly WeakEventManager _onLayoutChangedEventManager = new WeakEventManager();
		readonly WeakEventManager _onScreenChangedEventManager = new WeakEventManager();
		readonly FoldableMonitorRegistry<VisualElement, ViewMonitor> _monitors;
		UIWindow _window;
		ViewMonitor _windowMonitor;
		double _hingeAngle;

		public FoldableService()
		{
			_monitors = new FoldableMonitorRegistry<VisualElement, ViewMonitor>(DisposeMonitor);
			DeviceDisplay.MainDisplayInfoChanged += OnDisplayInfoChanged;
		}

		public event EventHandler OnScreenChanged
		{
			add => _onScreenChangedEventManager.AddEventHandler(value);
			remove => _onScreenChangedEventManager.RemoveEventHandler(value);
		}

		public event EventHandler<FoldEventArgs> OnLayoutChanged
		{
			add => _onLayoutChangedEventManager.AddEventHandler(value);
			remove => _onLayoutChangedEventManager.RemoveEventHandler(value);
		}

		public event EventHandler<FoldableHingeAngleChangedEventArgs> HingeAngleChanged
		{
			add => _hingeAngleChangedEventManager.AddEventHandler(value);
			remove => _hingeAngleChangedEventManager.RemoveEventHandler(value);
		}

		public bool IsSpanned => false;

		public bool IsLandscape => DeviceDisplay.MainDisplayInfo.Orientation.IsLandscape();

		public Size ScaledScreenSize => DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();

		public Task<int> GetHingeAngleAsync() => Task.FromResult((int)Math.Round(_hingeAngle));

		public Rect GetHinge() => Rect.Zero;

		public Rect GetHinge(VisualElement visualElement)
		{
			var platformView = GetPlatformView(visualElement);
			if (platformView?.Window == null || !OperatingSystem.IsIOSVersionAtLeast(27, 1))
				return Rect.Zero;

			var regions = platformView.GetReservedRegions(DivisionRegionKind);
			if (regions == null || regions.Length == 0)
				return Rect.Zero;

			var candidates = new List<FoldableRegion>(regions.Length);
			for (int i = 0; i < regions.Length; i++)
			{
				var region = regions[i];
				var frame = region.Frame;
				candidates.Add(new FoldableRegion(
					new Rect(frame.X, frame.Y, frame.Width, frame.Height),
					region.Active));
			}

			var localRegion = FoldableRegionHelper.GetActiveDivisionRegion(
				candidates,
				new Rect(0, 0, platformView.Bounds.Width, platformView.Bounds.Height));

			if (localRegion == Rect.Zero)
				return Rect.Zero;

			var windowRegion = platformView.ConvertRectToView(
				new CoreGraphics.CGRect(localRegion.X, localRegion.Y, localRegion.Width, localRegion.Height),
				platformView.Window);

			return new Rect(windowRegion.X, windowRegion.Y, windowRegion.Width, windowRegion.Height);
		}

		public bool IsLandscapeFor(VisualElement visualElement)
		{
			var window = GetPlatformView(visualElement)?.Window ?? _window;
			if (window == null)
				return IsLandscape;

			return window.Bounds.Width >= window.Bounds.Height;
		}

		public Size GetScaledScreenSize(VisualElement visualElement)
		{
			var window = GetPlatformView(visualElement)?.Window ?? _window;
			if (window == null)
				return ScaledScreenSize;

			return new Size(window.Bounds.Width, window.Bounds.Height);
		}

		public Point? GetLocationOnScreen(VisualElement visualElement)
		{
			var platformView = GetPlatformView(visualElement);
			if (platformView?.Window == null)
				return null;

			var frame = platformView.ConvertRectToView(platformView.Bounds, platformView.Window);
			return new Point(frame.X, frame.Y);
		}

		public void StartMonitoring(VisualElement visualElement)
		{
			if (GetPlatformView(visualElement) is not UIView platformView ||
				platformView.Window == null ||
				!OperatingSystem.IsIOSVersionAtLeast(27, 1))
			{
				return;
			}

			if (visualElement == null)
			{
				var interactionView = platformView.Window.RootViewController?.View ?? platformView.Window;
				if (ReferenceEquals(_windowMonitor?.View, interactionView))
					return;

				if (_windowMonitor != null)
					DisposeMonitor(_windowMonitor);

				_windowMonitor = CreateMonitor(interactionView);
				return;
			}

			_monitors.GetOrAdd(visualElement, () =>
			{
				var interactionView = platformView.Window.RootViewController?.View ?? platformView.Window;
				return CreateMonitor(interactionView);
			});
		}

		public void StopMonitoring(VisualElement visualElement)
		{
			if (visualElement == null)
			{
				if (_windowMonitor != null)
					DisposeMonitor(_windowMonitor);

				_windowMonitor = null;
				return;
			}

			_monitors.Remove(visualElement);
		}

		public void Dispose()
		{
			DeviceDisplay.MainDisplayInfoChanged -= OnDisplayInfoChanged;
			_monitors.Dispose();

			if (_windowMonitor != null)
				DisposeMonitor(_windowMonitor);

			_windowMonitor = null;
		}

		internal void SetWindow(UIWindow window)
		{
			_window = window;
		}

		UIView GetPlatformView(VisualElement visualElement)
		{
			return visualElement?.Handler?.PlatformView as UIView ??
				_window?.RootViewController?.View;
		}

		ViewMonitor CreateMonitor(UIView interactionView)
		{
			UIHingeInteractionUpdateHandler updateHandler = OnHingeUpdated;
			var interaction = new UIHingeInteraction(updateHandler);
			interactionView.AddInteraction(interaction);
			return new ViewMonitor(interactionView, interaction, updateHandler);
		}

		void OnHingeUpdated(UIHingeInteraction interaction, UIHingeInteractionUpdate update)
		{
			var hinge = update.Hinge;
			var newAngle = hinge == null ? 0d : FoldableRegionHelper.RadiansToDegrees((double)hinge.Angle);
			if (_hingeAngle != newAngle)
			{
				_hingeAngle = newAngle;
				_hingeAngleChangedEventManager.HandleEvent(
					this,
					new FoldableHingeAngleChangedEventArgs(newAngle),
					nameof(HingeAngleChanged));
			}

			RaiseLayoutChanged();
		}

		void OnDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
		{
			_onScreenChangedEventManager.HandleEvent(this, e, nameof(OnScreenChanged));
			RaiseLayoutChanged();
		}

		void RaiseLayoutChanged()
		{
			_onLayoutChangedEventManager.HandleEvent(
				this,
				new FoldEventArgs(),
				nameof(OnLayoutChanged));
		}

		static void DisposeMonitor(ViewMonitor monitor)
		{
			monitor.Interaction.Enabled = false;
			monitor.View.RemoveInteraction(monitor.Interaction);
			monitor.Interaction.Dispose();
		}

		sealed class ViewMonitor
		{
			public ViewMonitor(
				UIView view,
				UIHingeInteraction interaction,
				UIHingeInteractionUpdateHandler updateHandler)
			{
				View = view;
				Interaction = interaction;
				UpdateHandler = updateHandler;
			}

			public UIView View { get; }
			public UIHingeInteraction Interaction { get; }
			public UIHingeInteractionUpdateHandler UpdateHandler { get; }
		}
	}
}
#endif
