#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Platform
{
	internal sealed partial class MauiContentButton : MauiButton
	{
		readonly ConditionalWeakTable<Control, StrongBox<bool>> _contentTabStops = new();
		readonly Dictionary<string, object> _nativeResourceDefaults = new();
		FrameworkElement? _presentedContent;
		bool _nativeDefaultsCaptured;

		static readonly string[] BackgroundResourceKeys =
		{
			"ButtonBackground",
			"ButtonBackgroundPointerOver",
			"ButtonBackgroundPressed",
			"ButtonBackgroundDisabled",
		};

		static readonly string[] StrokeColorResourceKeys =
		{
			"ButtonBorderBrush",
			"ButtonBorderBrushPointerOver",
			"ButtonBorderBrushPressed",
			"ButtonBorderBrushDisabled",
		};

		static readonly string[] StrokeThicknessResourceKeys =
		{
			"ButtonBorderThemeThickness",
		};

		static readonly string[] CornerRadiusResourceKeys =
		{
			"ControlCornerRadius",
		};

		internal event EventHandler<KeyRoutedEventArgs>? ActivationKeyDown;
		internal event EventHandler<KeyRoutedEventArgs>? ActivationKeyUp;

		internal MauiContentButton()
		{
			Padding = new WThickness(0);
			HorizontalContentAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
			VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;

			ContentPanel = new ContentPanel
			{
				HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
				VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch,
				IsHitTestVisible = false,
				Margin = new WThickness(0)
			};

			AutomationProperties.SetAccessibilityView(ContentPanel, AccessibilityView.Raw);
			Content = ContentPanel;
		}

		internal ContentPanel ContentPanel { get; }

		internal void CaptureNativeDefaults()
		{
			if (_nativeDefaultsCaptured)
				return;

			_nativeDefaultsCaptured = true;
			CaptureNativeDefaults(BackgroundResourceKeys);
			CaptureNativeDefaults(StrokeColorResourceKeys);
			CaptureNativeDefaults(StrokeThicknessResourceKeys);
			CaptureNativeDefaults(CornerRadiusResourceKeys);
		}

		internal void UpdateBackground(IContentButton button)
		{
			ControlExtensions.BackgroundImageRequests.Begin(this);
			CaptureNativeDefaults();

			var brush = button.Background?.ToPlatform();
			if (brush is null)
				RestoreNativeDefaults(BackgroundResourceKeys);
			else
			{
				RemoveNativeDefaultBeforeBackgroundOverride();
				ButtonExtensions.UpdateBackgroundBrush(this, brush);
			}
		}

		internal void UpdateStrokeColor(IContentButton button)
		{
			CaptureNativeDefaults();

			if (button.StrokeColor is null)
				RestoreNativeDefaults(StrokeColorResourceKeys);
			else
				ButtonExtensions.UpdateStrokeColor(this, button);
		}

		internal void UpdateStrokeThickness(IContentButton button)
		{
			CaptureNativeDefaults();

			if (button.StrokeThickness < 0)
				RestoreNativeDefaults(StrokeThicknessResourceKeys);
			else
				ButtonExtensions.UpdateStrokeThickness(this, button);
		}

		internal void UpdateCornerRadius(IContentButton button)
		{
			CaptureNativeDefaults();

			if (button.CornerRadius < 0)
				RestoreNativeDefaults(CornerRadiusResourceKeys);
			else
				ButtonExtensions.UpdateCornerRadius(this, button);
		}

		internal void SetPresentedContent(FrameworkElement? content)
		{
			ClearPresentedContent();

			_presentedContent = content;
			ContentPanel.Content = content;

			if (content is null)
				return;

			// ContentPanel is not hit-test visible, which excludes this subtree from input.
			// Leave the content's own IsHitTestVisible to its InputTransparent mapping.
			content.Loaded += OnPresentedContentLoaded;
			DisableTabStops(content);
		}

		internal void ClearPresentedContent()
		{
			if (_presentedContent is not null)
				_presentedContent.Loaded -= OnPresentedContentLoaded;

			foreach (var pair in _contentTabStops)
			{
				pair.Key.IsTabStop = pair.Value.Value;
			}

			_contentTabStops.Clear();
			_presentedContent = null;
			ContentPanel.Content = null;
		}

		internal void UpdateContentLayout(double strokeThickness)
		{
			var strokeInset = Math.Max(0, strokeThickness);

			// The cross-platform layout accounts for an explicit stroke. Cancel the
			// space reserved by the native Button template so the inset is applied once.
			ContentPanel.Margin = new WThickness(-strokeInset);
			Padding = new WThickness(0);
			InvalidateMeasure();
		}

		protected override void OnKeyDown(KeyRoutedEventArgs e)
		{
			if (e.Key is VirtualKey.Space or VirtualKey.Enter)
			{
				ActivationKeyDown?.Invoke(this, e);
			}

			base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyRoutedEventArgs e)
		{
			if (e.Key is VirtualKey.Space or VirtualKey.Enter)
			{
				ActivationKeyUp?.Invoke(this, e);
			}

			base.OnKeyUp(e);
		}

		void OnPresentedContentLoaded(object sender, RoutedEventArgs e)
		{
			if (_presentedContent is not null)
			{
				DisableTabStops(_presentedContent);
			}
		}

		void DisableTabStops(DependencyObject element)
		{
			if (element is Control control && !_contentTabStops.TryGetValue(control, out _))
			{
				_contentTabStops.Add(control, new StrongBox<bool>(control.IsTabStop));
				control.IsTabStop = false;
			}

			for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
			{
				DisableTabStops(VisualTreeHelper.GetChild(element, i));
			}
		}

		void CaptureNativeDefaults(string[] keys)
		{
			// Lookups can resolve theme resources that this button never set. Only capture keys
			// defined locally (for example by a handler customization) so theme values stay dynamic.
			var localKeys = Resources.Keys;
			foreach (var key in keys)
			{
				if (localKeys.Contains(key) && Resources.TryGetValue(key, out var value) && value is not null)
					_nativeResourceDefaults[key] = value;
			}
		}

		void RestoreNativeDefaults(string[] keys)
		{
			foreach (var key in keys)
			{
				if (_nativeResourceDefaults.TryGetValue(key, out var value))
					Resources[key] = value;
				else
					Resources.Remove(key);
			}

			this.RefreshThemeResources();
		}

		// ButtonExtensions may adjust the current brush's alpha for transitions; never mutate the captured default.
		void RemoveNativeDefaultBeforeBackgroundOverride()
		{
			const string backgroundKey = "ButtonBackground";

			if (_nativeResourceDefaults.TryGetValue(backgroundKey, out var nativeDefault) &&
				Resources.TryGetValue(backgroundKey, out var current) &&
				ReferenceEquals(current, nativeDefault))
			{
				Resources.Remove(backgroundKey);
			}
		}
	}
}
