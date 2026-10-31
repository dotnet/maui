#nullable enable
using System;
using System.Collections.Generic;
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
		readonly Dictionary<Control, bool> _contentTabStops = new();
		FrameworkElement? _presentedContent;
		bool _presentedContentHitTestVisible;

		internal event EventHandler<KeyRoutedEventArgs>? ActivationKeyDown;
		internal event EventHandler<KeyRoutedEventArgs>? ActivationKeyUp;

		internal MauiContentButton()
		{
			Padding = new WThickness(0);
			HorizontalContentAlignment = HorizontalAlignment.Stretch;
			VerticalContentAlignment = VerticalAlignment.Stretch;

			ContentPanel = new ContentPanel
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				IsHitTestVisible = false,
				Margin = new WThickness(0)
			};

			AutomationProperties.SetAccessibilityView(ContentPanel, AccessibilityView.Raw);
			Content = ContentPanel;
		}

		internal ContentPanel ContentPanel { get; }

		internal void SetPresentedContent(FrameworkElement? content)
		{
			ClearPresentedContent();

			_presentedContent = content;
			ContentPanel.Content = content;

			if (content is null)
				return;

			_presentedContentHitTestVisible = content.IsHitTestVisible;
			content.IsHitTestVisible = false;
			content.Loaded += OnPresentedContentLoaded;
			DisableTabStops(content);
		}

		internal void ClearPresentedContent()
		{
			if (_presentedContent is not null)
			{
				_presentedContent.Loaded -= OnPresentedContentLoaded;
				_presentedContent.IsHitTestVisible = _presentedContentHitTestVisible;
			}

			foreach (var pair in _contentTabStops)
			{
				pair.Key.IsTabStop = pair.Value;
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
			if (element is Control control && !_contentTabStops.ContainsKey(control))
			{
				_contentTabStops.Add(control, control.IsTabStop);
				control.IsTabStop = false;
			}

			for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
			{
				DisableTabStops(VisualTreeHelper.GetChild(element, i));
			}
		}
	}
}
