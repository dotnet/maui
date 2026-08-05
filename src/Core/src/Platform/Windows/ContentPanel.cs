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
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	public partial class ContentPanel : MauiPanel
	{
		readonly Path? _borderPath;
		IBorderStroke? _borderStroke;
		FrameworkElement? _content;

		internal Path? BorderPath => _borderPath;
		internal IBorderStroke? BorderStroke => _borderStroke;
		internal FrameworkElement? Content
		{
			get => _content;
			set
			{
				var children = CachedChildren;

				// Remove the previous content if it exists
				if (_content is not null && children.Contains(_content) && value != _content)
				{
					children.Remove(_content);
				}

				_content = value;

				if (_content is null)
				{
					return;
				}

				if (!children.Contains(_content))
				{
					children.Add(_content);
				}
			}
		}

		internal bool IsInnerPath { get; private set; }

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			var measured = base.MeasureOverride(availableSize);

			// On Windows, when content inside a Border has the same WidthRequest/HeightRequest
			// as the Border itself, AdjustForExplicitSize expands the content's measured size
			// back to its explicit request even after the stroke inset reduces the constraint.
			// This inflates MeasureContent's result by StrokeThickness*2, so the parent
			// allocates an oversized layout slot and the border renders with its right/bottom
			// strokes clipped. Capping here at the Border's explicit dimensions corrects the
			// reported desired size and ensures the parent allocates the right amount.
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
			var actual = base.ArrangeOverride(finalSize);

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

			if (_borderStroke is null)
			{
				return;
			}

			UpdateBorder(_borderStroke.Shape);
		}

		void UpdateBorder(IShape? strokeShape)
		{
			if (strokeShape is null || _borderPath is null)
			{
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
			if (Content is null)
			{
				return;
			}

			if (height <= 0 && width <= 0)
			{
				return;
			}

			var clipGeometry = borderShape;

			if (clipGeometry is null)
			{
				return;
			}

			var visual = ElementCompositionPreview.GetElementVisual(Content);

			// Prevent clip collision: When ContentView is inside Border, let WrapperView handle 
			// clipping to avoid Border overwriting ContentView's clip geometry during SizeChanged.
			if (visual.Clip != null && Content.Parent is WrapperView)
			{
				return;
			}
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

			// Use ActualOffset (not LayoutInformation.GetLayoutSlot) because it reflects the true
			// visual position of Content after WinUI alignment adjustments (e.g. a Stretch=None
			// image wider than its slot is centered, shifting ActualOffset well outside the slot).
			// The formula converts the stroke-inset boundary from ContentPanel space into Content's
			// local space so the clip aligns correctly regardless of alignment-driven offsets.
			geometricClip.Offset = new Vector2(strokeThickness - Content.ActualOffset.X, strokeThickness - Content.ActualOffset.Y);

			visual.Clip = geometricClip;
		}
	}
}