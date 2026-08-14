using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Maui.Graphics;
#if MAUI_GRAPHICS_WIN2D
using Microsoft.Maui.Graphics.Win2D;
#else
using Microsoft.Maui.Graphics.Platform;
#endif
using Microsoft.Maui.Primitives;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	public partial class ContentPanel : MauiPanel
	{
		readonly Path? _borderPath;
		ContentPanelClipHost? _contentClipHost;
		IBorderStroke? _borderStroke;
		FrameworkElement? _content;

		internal Path? BorderPath => _borderPath;
		internal IBorderStroke? BorderStroke => _borderStroke;
		internal ContentPanelClipHost? ContentClipHost => _contentClipHost;
		internal FrameworkElement? Content
		{
			get => _content;
			set
			{
				if (value == _content)
				{
					return;
				}

				if (_contentClipHost is not null)
				{
					_contentClipHost.CachedChildren.Clear();
					ClearContentClip();
				}
				else if (_content is not null && CachedChildren.Contains(_content))
				{
					CachedChildren.Remove(_content);
				}

				_content = value;

				if (_content is null)
				{
					return;
				}

				var children = _contentClipHost?.CachedChildren ?? CachedChildren;
				if (!children.Contains(_content))
				{
					children.Add(_content);
				}
			}

		}

		internal bool IsInnerPath { get; private set; }

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			global::Windows.Foundation.Size measured;

			if (_contentClipHost is not null)
			{
				_contentClipHost.InvalidateMeasure();
				_contentClipHost.Measure(availableSize);
				measured = _contentClipHost.DesiredSize;
			}
			else
			{
				measured = base.MeasureOverride(availableSize);
			}

			return ConstrainMeasureToExplicitBorderSize(measured);
		}

		internal global::Windows.Foundation.Size ConstrainMeasureToExplicitBorderSize(global::Windows.Foundation.Size measured)
		{
			// AdjustForExplicitSize can expand content back to its explicit request after the
			// stroke inset reduced its constraint. Cap both the host and panel desired sizes so
			// WinUI also arranges the host within the Border's explicit dimensions.
			if (_borderStroke is not null && Content is not null &&
			 CrossPlatformLayout is IBorderView borderView)
			{
				var explicitWidth = borderView.Width;
				var explicitHeight = borderView.Height;

				if (Dimension.IsExplicitSet(explicitWidth))
				{
					measured.Width = Math.Min(measured.Width, explicitWidth);
				}

				if (Dimension.IsExplicitSet(explicitHeight))
				{
					measured.Height = Math.Min(measured.Height, explicitHeight);
				}
			}

			return measured;
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			global::Windows.Foundation.Size actual;

			if (_contentClipHost is not null)
			{
				_contentClipHost.InvalidateArrange();
				_contentClipHost.Arrange(new global::Windows.Foundation.Rect(0, 0, finalSize.Width, finalSize.Height));
				actual = finalSize;
			}
			else
			{
				actual = base.ArrangeOverride(finalSize);
			}

			_borderPath?.Arrange(new global::Windows.Foundation.Rect(0, 0, finalSize.Width, finalSize.Height));

			var size = new global::Windows.Foundation.Size(Math.Max(0, actual.Width), Math.Max(0, actual.Height));

			// We need to update the clip since the content's position might have changed
			UpdateClip(_borderStroke?.Shape, size.Width, size.Height);

			return actual;
		}

		protected override AutomationPeer OnCreateAutomationPeer()
		{
			if (CrossPlatformLayout is IBorderView)
			{
				return new MauiBorderAutomationPeer(this);
			}
			else if (CrossPlatformLayout is IContentView)
			{
				// Custom automation peer prevents duplicate announcements when AutomationProperties.Name is set
				return new ContentPanelAutomationPeer(this);
			}

			return base.OnCreateAutomationPeer();
		}

		public ContentPanel()
		{
			_borderPath = new Path();
			EnsureBorderPath(containsCheck: false);

			SizeChanged += ContentPanelSizeChanged;

			RegisterPropertyChangedCallback(BackgroundProperty, OnBackgroundPropertyChanged);
			EnsureHitTestBackground();
		}

		void ContentPanelSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (_borderPath is null)
			{
				return;
			}

			var width = e.NewSize.Width;
			var height = e.NewSize.Height;

			if (width <= 0 || height <= 0)
			{
				ClearContentClip();
				return;
			}

			_borderPath.UpdatePath(_borderStroke?.Shape, width, height);
			UpdateClip(_borderStroke?.Shape, width, height);
		}

		internal void EnsureBorderPath(bool containsCheck = true)
		{
			if (containsCheck)
			{
				var children = CachedChildren;

				if (!children.Contains(_borderPath))
				{
					children.Add(_borderPath);
				}
			}
			else
			{
				CachedChildren.Add(_borderPath);
			}
		}

		internal void EnableContentClip()
		{
			if (_contentClipHost is not null)
			{
				_contentClipHost.CrossPlatformLayout = CrossPlatformLayout;
				return;
			}

			_contentClipHost = new ContentPanelClipHost(this)
			{
				CrossPlatformLayout = CrossPlatformLayout,
			};
			CachedChildren.Add(_contentClipHost);

			if (_content is not null && CachedChildren.Contains(_content))
			{
				CachedChildren.Remove(_content);
				_contentClipHost.CachedChildren.Add(_content);
			}
		}

		internal void UpdateCrossPlatformLayout(ICrossPlatformLayout? crossPlatformLayout)
		{
			CrossPlatformLayout = crossPlatformLayout;

			if (_contentClipHost is not null)
			{
				_contentClipHost.CrossPlatformLayout = crossPlatformLayout;
			}
		}

		static void OnBackgroundPropertyChanged(DependencyObject dependencyObject, DependencyProperty dependencyProperty)
		{
			if (dependencyObject is ContentPanel contentPanel)
			{
				contentPanel.EnsureHitTestBackground();
			}
		}

		void EnsureHitTestBackground()
		{
			if (Background == null)
			{
				Background = new SolidColorBrush(UI.Colors.Transparent);
			}
		}

		public void UpdateBackground(Paint? background)
		{
			if (_borderPath is null)
			{
				return;
			}

			_borderPath.UpdateBackground(background);
		}

		[Obsolete("Use Microsoft.Maui.Platform.UpdateBorderStroke instead")]
		public void UpdateBorderShape(IShape borderShape)
		{
			UpdateBorder(borderShape);
		}

		internal void UpdateBorderStroke(IBorderStroke borderStroke)
		{
			if (borderStroke is null)
			{
				return;
			}

			_borderStroke = borderStroke;
			UpdateBorder(borderStroke.Shape);
		}

		void UpdateBorder(IShape? strokeShape)
		{
			if (_borderPath is null)
			{
				return;
			}

			if (strokeShape is null)
			{
				_borderPath.Data = null;
				ClearContentClip();
				return;
			}

			_borderPath.UpdateBorderShape(strokeShape, ActualWidth, ActualHeight);

			var width = ActualWidth;
			var height = ActualHeight;

			if (width <= 0 || height <= 0)
			{
				return;
			}

			UpdateClip(strokeShape, width, height);
		}

		void UpdateClip(IShape? borderShape, double width, double height)
		{
			if (Content is null || _contentClipHost is null)
			{
				return;
			}

			if (width <= 0 || height <= 0)
			{
				ClearContentClip();
				return;
			}

			var clipGeometry = borderShape;

			if (clipGeometry is null)
			{
				ClearContentClip();
				return;
			}

			var visual = ElementCompositionPreview.GetElementVisual(_contentClipHost);
			var compositor = visual.Compositor;

			PathF? clipPath;
			float strokeThickness = (float)(_borderPath?.StrokeThickness ?? 0);
			// The path size should consider the space taken by the border (top and bottom, left and right)
			var pathSize = new Rect(0, 0, width - strokeThickness * 2, height - strokeThickness * 2);

			if (clipGeometry is IRoundRectangle roundedRectangle)
			{
				clipPath = roundedRectangle.InnerPathForBounds(pathSize, strokeThickness / 2);
				IsInnerPath = true;
			}
			else
			{
				clipPath = clipGeometry.PathForBounds(pathSize);
				IsInnerPath = false;
			}

			var device = CanvasDevice.GetSharedDevice();
			var geometry = clipPath.AsPath(device);
			var path = new CompositionPath(geometry);
			var pathGeometry = compositor.CreatePathGeometry(path);
			var geometricClip = compositor.CreateGeometricClip(pathGeometry);

			geometricClip.Offset = new Vector2(strokeThickness, strokeThickness);

			visual.Clip = geometricClip;
		}

		void ClearContentClip()
		{
			if (_contentClipHost is not null)
			{
				ElementCompositionPreview.GetElementVisual(_contentClipHost).Clip = null;
			}
		}
	}

	sealed partial class ContentPanelClipHost : MauiPanel
	{
		readonly ContentPanel _owner;

		internal ContentPanelClipHost(ContentPanel owner)
		{
			_owner = owner;
		}

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize) =>
			_owner.ConstrainMeasureToExplicitBorderSize(base.MeasureOverride(availableSize));
	}
}