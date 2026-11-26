#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using iOSPage = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page;

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Configures the native <see cref="UISheetPresentationController"/> of a modal page presented as a <see cref="BottomSheet"/>
	/// and keeps <see cref="BottomSheet.SelectedDetentProperty"/> in sync with it.
	/// </summary>
	internal sealed class BottomSheetPresentation : IDisposable
	{
		// Points within which two detents are the same resting position.
		const double Tolerance = 1;

		// iOS 15 has only the system detents: a detent up to this fraction of the available height uses the medium one.
		const double MediumThreshold = 0.65;

		const string CustomIdentifierPrefix = "Microsoft.Maui.Controls.BottomSheetDetent.";

		static readonly NSString SelectedDetentIdentifierKey = new NSString("selectedDetentIdentifier");

		readonly WeakReference<UIViewController> _controller;
		Page? _page;
		List<BottomSheetStop> _stops = new();
		string[] _identifiers = Array.Empty<string>();
		double _available;
		double _measuredWidth = -1;
		double _contentHeight = -1;
		bool _cornerRadiusSet;
		bool _updatingFromPlatform;
		bool _contentRemeasurePending;

		public BottomSheetPresentation(UIViewController controller, Page page)
		{
			_controller = new WeakReference<UIViewController>(controller);
			_page = page;

			controller.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;

			page.PropertyChanged += OnPagePropertyChanged;
			page.MeasureInvalidated += OnPageMeasureInvalidated;

			Apply(animated: false);
		}

		UIViewController? Controller => _controller.TryGetTarget(out var controller) ? controller : null;

		[System.Runtime.Versioning.SupportedOSPlatformGuard("ios16.0")]
		[System.Runtime.Versioning.SupportedOSPlatformGuard("maccatalyst16.0")]
		static bool SupportsCustomDetents => OperatingSystem.IsIOSVersionAtLeast(16) || OperatingSystem.IsMacCatalystVersionAtLeast(16);

		bool HasFitContent
		{
			get
			{
				if (_page is null)
					return false;

				foreach (var detent in BottomSheet.GetDetents(_page))
				{
					if (detent.Kind == BottomSheetDetentKind.FitContent)
						return true;
				}

				return false;
			}
		}

		/// <summary>Called by the presenting controller after its layout, when the sheet's size may have changed.</summary>
		public void OnLayout()
		{
			var controller = Controller;
			if (_page is null || controller?.View is not UIView view || controller.SheetPresentationController is not UISheetPresentationController sheet)
				return;

			bool widthChanged = HasFitContent && Math.Abs(view.Bounds.Width - _measuredWidth) > Tolerance && view.Bounds.Width > 0;
			bool availableChanged = Math.Abs(EstimateAvailableHeight(controller) - _available) > Tolerance;

			if (widthChanged || availableChanged)
			{
				// Re-entering UIKit's layout from its own layout pass is not allowed, so update after it.
				controller.BeginInvokeOnMainThread(() => Apply(animated: false));
			}
		}

		/// <summary>Called when the user lets go of the sheet at another detent.</summary>
		public void OnPlatformSelectedDetentChanged(UISheetPresentationController sheet)
		{
			if (_page is null)
				return;

			var identifier = (sheet.ValueForKey(SelectedDetentIdentifierKey) as NSString)?.ToString();
			int index = Array.IndexOf(_identifiers, identifier);
			if (index < 0)
				return;

			var current = BottomSheet.GetSelectedDetent(_page);
			var reported = _stops[index].ReportedDetent(current);
			if (reported.Equals(current))
				return;

			_updatingFromPlatform = true;
			try
			{
				_page.SetValueFromRenderer(BottomSheet.SelectedDetentProperty, reported);
			}
			finally
			{
				_updatingFromPlatform = false;
			}
		}

		public void Dispose()
		{
			if (_page is not null)
			{
				_page.PropertyChanged -= OnPagePropertyChanged;
				_page.MeasureInvalidated -= OnPageMeasureInvalidated;
				_page = null;
			}
		}

		void OnPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BottomSheet.SelectedDetentProperty.PropertyName)
			{
				if (!_updatingFromPlatform)
					ApplySelection(animated: true);
			}
			else if (e.IsOneOf(BottomSheet.DetentsProperty, BottomSheet.HasDragHandleProperty, BottomSheet.IsDismissibleProperty)
				|| e.IsOneOf(BottomSheet.CornerRadiusProperty, iOSPage.BottomSheetPrefersEdgeAttachedInCompactHeightProperty, iOSPage.BottomSheetPrefersScrollingExpandsWhenScrolledToEdgeProperty))
			{
				Apply(animated: true);
			}
		}

		void OnPageMeasureInvalidated(object? sender, EventArgs e)
		{
			if (!HasFitContent)
			{
				// Measured again if FitContent comes back: the content it measured may have changed meanwhile.
				_contentHeight = -1;
				return;
			}

			if (_contentRemeasurePending || Controller is not UIViewController controller)
				return;

			// Content changes come in bursts; measure once they are done.
			_contentRemeasurePending = true;
			controller.BeginInvokeOnMainThread(() =>
			{
				_contentRemeasurePending = false;
				if (_page is not null && MeasureContent(force: true))
					Apply(animated: true);
			});
		}

		void Apply(bool animated)
		{
			var controller = Controller;
			if (_page is null || controller?.SheetPresentationController is not UISheetPresentationController sheet)
				return;

			var page = _page;
			void Update()
			{
				controller.ModalInPresentation = !BottomSheet.GetIsDismissible(page);
				// Mac Catalyst shows the page in a window sheet, while the grabber would stay behind on the presenting window.
				sheet.PrefersGrabberVisible = !OperatingSystem.IsMacCatalyst() && BottomSheet.GetHasDragHandle(page);
				sheet.PrefersEdgeAttachedInCompactHeight = iOSPage.GetBottomSheetPrefersEdgeAttachedInCompactHeight(page);
				sheet.PrefersScrollingExpandsWhenScrolledToEdge = iOSPage.GetBottomSheetPrefersScrollingExpandsWhenScrolledToEdge(page);

				var cornerRadius = BottomSheet.GetCornerRadius(page);
				if (cornerRadius >= 0)
				{
					sheet.PreferredCornerRadius = (nfloat)cornerRadius;
					_cornerRadiusSet = true;
				}
				else if (_cornerRadiusSet)
				{
					sheet.PreferredCornerRadius = UISheetPresentationControllerDetent.AutomaticDimension;
					_cornerRadiusSet = false;
				}

				UpdateDetents(controller, sheet, page);
				SetSelectedIdentifier(sheet, page);
			}

			if (animated && controller.PresentingViewController is not null)
				sheet.AnimateChanges(Update);
			else
				Update();
		}

		void ApplySelection(bool animated)
		{
			var controller = Controller;
			if (_page is null || controller?.SheetPresentationController is not UISheetPresentationController sheet)
				return;

			var page = _page;
			if (animated && controller.PresentingViewController is not null)
				sheet.AnimateChanges(() => SetSelectedIdentifier(sheet, page));
			else
				SetSelectedIdentifier(sheet, page);
		}

		void SetSelectedIdentifier(UISheetPresentationController sheet, Page page)
		{
			if (_identifiers.Length == 0)
				return;

			int index = BottomSheetDetentResolver.IndexOf(_stops, BottomSheet.GetSelectedDetent(page));
			if (index < 0)
			{
				// A detent the sheet doesn't have leaves a presented sheet where it is.
				if (Controller?.PresentingViewController is not null)
					return;

				index = 0;
			}

			// The binding only exposes the system identifiers, so the selection goes through the Objective-C property.
			sheet.SetValueForKey(new NSString(_identifiers[index]), SelectedDetentIdentifierKey);
		}

		void UpdateDetents(UIViewController controller, UISheetPresentationController sheet, Page page)
		{
			_available = EstimateAvailableHeight(controller);
			MeasureContent(force: false);

			var detents = BottomSheet.GetDetents(page);
			Func<double>? measure = _contentHeight >= 0 ? () => _contentHeight : null;

			List<BottomSheetStop> stops;
			if (SupportsCustomDetents)
			{
				stops = BottomSheetDetentResolver.Resolve(detents, _available, measure, Tolerance);
			}
			else
			{
				stops = ResolveSystemStops(detents, _available, measure);
			}

			var identifiers = new string[stops.Count];
			var nativeDetents = new UISheetPresentationControllerDetent[stops.Count];
			for (int i = 0; i < stops.Count; i++)
			{
				(nativeDetents[i], identifiers[i]) = CreateNativeDetent(stops[i], i);
			}

			bool sameIdentifiers = identifiers.Length == _identifiers.Length;
			for (int i = 0; sameIdentifiers && i < identifiers.Length; i++)
				sameIdentifiers = identifiers[i] == _identifiers[i];

			_stops = stops;
			_identifiers = identifiers;

			if (!sameIdentifiers || sheet.Detents.Length != nativeDetents.Length)
				sheet.Detents = nativeDetents;
			else if (SupportsCustomDetents)
				sheet.InvalidateDetents();
		}

		(UISheetPresentationControllerDetent Detent, string Identifier) CreateNativeDetent(BottomSheetStop stop, int index)
		{
			// Without custom detents (iOS 15), each position is the system medium or large detent: the large one is the full height.
			bool isLarge = SupportsCustomDetents
				? stop.Detents.Exists(d => d.Kind == BottomSheetDetentKind.Large)
				: stop.Height >= _available - Tolerance;

			if (isLarge)
				return (UISheetPresentationControllerDetent.CreateLargeDetent(), UISheetPresentationControllerDetentIdentifier.Large.GetConstant()!.ToString());

			if (stop.Detents.Exists(d => d.Kind == BottomSheetDetentKind.Medium) || !SupportsCustomDetents)
				return (UISheetPresentationControllerDetent.CreateMediumDetent(), UISheetPresentationControllerDetentIdentifier.Medium.GetConstant()!.ToString());

			var detent = stop.Detents[0];
			var identifier = CustomIdentifierPrefix + index.ToString(System.Globalization.CultureInfo.InvariantCulture) + "." + detent;
			var weakThis = new WeakReference<BottomSheetPresentation>(this);

			var native = UISheetPresentationControllerDetent.Create(identifier, context =>
			{
				// Resolvers run inside UIKit's layout: they only read the cached content height, never measure.
				double max = context.MaximumDetentValue;
				double contentHeight = weakThis.TryGetTarget(out var presentation) ? presentation._contentHeight : -1;
				Func<double>? measure = contentHeight >= 0 ? () => contentHeight : null;
				return (nfloat)BottomSheetDetentResolver.ResolveHeight(detent, max, measure);
			});

			return (native, identifier);
		}

		// iOS 15: every detent maps to the system medium or large detent.
		static List<BottomSheetStop> ResolveSystemStops(IReadOnlyList<BottomSheetDetent> detents, double available, Func<double>? measure)
		{
			BottomSheetStop? medium = null;
			BottomSheetStop? large = null;

			foreach (var detent in detents)
			{
				bool isMedium = detent.Kind switch
				{
					BottomSheetDetentKind.Medium => true,
					BottomSheetDetentKind.Large or BottomSheetDetentKind.FitContent => false,
					_ => available > 0 && BottomSheetDetentResolver.ResolveHeight(detent, available, measure) <= available * MediumThreshold,
				};

				ref var stop = ref isMedium ? ref medium : ref large;
				if (stop is null)
					stop = new BottomSheetStop(isMedium ? available * BottomSheetDetentResolver.MediumFraction : available, detent);
				else if (!stop.Detents.Contains(detent))
					stop.Detents.Add(detent);
			}

			var stops = new List<BottomSheetStop>(2);
			if (medium is not null)
				stops.Add(medium);
			if (large is not null)
				stops.Add(large);
			return stops;
		}

		bool MeasureContent(bool force)
		{
			if (_page is null || !HasFitContent || Controller is not UIViewController controller)
				return false;

			double width = controller.View?.Bounds.Width ?? 0;
			if (width <= 0)
				width = controller.PresentingViewController?.View?.Bounds.Width ?? controller.View?.Window?.Bounds.Width ?? UIScreen.MainScreen.Bounds.Width;

			if (!force && Math.Abs(width - _measuredWidth) <= Tolerance && _contentHeight >= 0)
				return false;

			// UIKit adds the bottom safe area to a custom detent, so the safe area padding the content applies itself is left out.
			double height = BottomSheetDetentResolver.MeasureContentHeight(_page, width) - GetContentBottomSafeArea(_page);
			_measuredWidth = width;

			if (double.IsNaN(height) || double.IsInfinity(height))
				height = double.PositiveInfinity;

			height = Math.Max(0, height);
			bool changed = Math.Abs(height - _contentHeight) > Tolerance || (_contentHeight < 0);
			_contentHeight = height;
			return changed;
		}

		static double GetContentBottomSafeArea(Page page)
		{
			if (page is not ContentPage contentPage ||
				((IContentView)contentPage).PresentedContent?.Handler is not IPlatformViewHandler { PlatformView: MauiView contentView } ||
				!contentView.AppliesSafeAreaAdjustments)
			{
				return 0;
			}

			return contentView.GetSafeAreaComponentForEdge(3);
		}

		// The largest height a sheet can have, which UIKit reports to the detent resolvers as the maximum detent value.
		static double EstimateAvailableHeight(UIViewController controller)
		{
			var container = controller.SheetPresentationController?.ContainerView;
			var window = container?.Window ?? controller.PresentingViewController?.View?.Window ?? controller.View?.Window;
			CGRect bounds = container?.Bounds ?? window?.Bounds ?? UIScreen.MainScreen.Bounds;
			UIEdgeInsets insets = container?.SafeAreaInsets ?? window?.SafeAreaInsets ?? UIEdgeInsets.Zero;

			// A large sheet leaves a gap under the top safe area and stops at the bottom safe area.
			return Math.Max(0, bounds.Height - insets.Top - insets.Bottom - 10);
		}
	}
}
