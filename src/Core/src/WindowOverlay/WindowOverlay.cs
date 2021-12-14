using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;

#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIView;
#elif __ANDROID__
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui
{
	public partial class WindowOverlay : IWindowOverlay, IDrawable
	{
		readonly HashSet<IWindowOverlayElement> _windowElements = new();
		bool _isVisible = true;
		private bool _disableUITouchEventPassthrough;

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowOverlay"/> class.
		/// </summary>
		/// <param name="window">The parent Window.</param>
		public WindowOverlay(IWindow window)
		{
			Window = window;
		}

		/// <inheritdoc/>
		public IWindow Window { get; }

		/// <inheritdoc/>
		public IReadOnlyCollection<IWindowOverlayElement> WindowElements => _windowElements;

		/// <inheritdoc/>
		public NativeView? GraphicsView => _graphicsView;

		/// <inheritdoc/>
		public bool IsNativeViewInitialized { get; private set; }

		/// <inheritdoc/>
		public bool DisableUITouchEventPassthrough
		{
			get => _disableUITouchEventPassthrough;
			set
			{
				_disableUITouchEventPassthrough = value;
				OnDisableUITouchEventPassthroughSet();
			}
		}

		/// <inheritdoc/>
		public bool EnableDrawableTouchHandling { get; set; }

		/// <inheritdoc/>
		public bool IsVisible
		{
			get => _isVisible;
			set
			{
				_isVisible = value;
				if (IsNativeViewInitialized)
					Invalidate();
			}
		}

		/// <inheritdoc/>
		public float Density { get; internal set; } = 1;

		/// <inheritdoc/>
		public event EventHandler<WindowOverlayTappedEventArgs>? Tapped;

		/// <inheritdoc/>
		public void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			if (!IsVisible)
				return;
			foreach (var drawable in _windowElements)
				drawable.Draw(canvas, dirtyRect);
		}

		public virtual bool Deinitialize()
		{
			DeinitializeNativeDependencies();
			return true;
		}

		/// <inheritdoc/>
		public virtual bool AddWindowElement(IWindowOverlayElement drawable)
		{
			if (drawable == null)
				throw new ArgumentNullException(nameof(drawable));

			var result = _windowElements.Add(drawable);
			Invalidate();
			return result;
		}

		/// <inheritdoc/>
		public virtual bool RemoveWindowElement(IWindowOverlayElement drawable)
		{
			if (drawable == null)
				throw new ArgumentNullException(nameof(drawable));

			var result = _windowElements.Remove(drawable);
			Invalidate();
			return result;
		}

		/// <inheritdoc/>
		public virtual void RemoveWindowElements()
		{
			_windowElements.Clear();
			Invalidate();
		}

		/// <inheritdoc/>
		public virtual void HandleUIChange()
		{
		}

		/// <summary>
		/// Handles <see cref="Tapped"/> event.
		/// </summary>
		/// <param name="point">Point where user has touched.</param>
		void OnTappedInternal(Point point)
		{
			var elements = new List<IVisualTreeElement>();
			var windowElements = new List<IWindowOverlayElement>();

			if (EnableDrawableTouchHandling)
			{
				windowElements.AddRange(_windowElements.Where(n => n.Contains(point)));
			}

			if (DisableUITouchEventPassthrough)
			{
				var visualWindow = Window as IVisualTreeElement;
				if (visualWindow != null)
					elements.AddRange(visualWindow.GetVisualTreeElements(point));
			}

			Tapped?.Invoke(this, new WindowOverlayTappedEventArgs(point, elements, windowElements));
		}

		partial void OnDisableUITouchEventPassthroughSet();
	}
}