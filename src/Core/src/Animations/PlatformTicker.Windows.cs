using System;
using Microsoft.UI.Xaml.Media;
using ViewManagement = Windows.UI.ViewManagement;

namespace Microsoft.Maui.Animations
{
	/// <inheritdoc/>
	public class PlatformTicker : Ticker, IDisposable
	{
		readonly ViewManagement.UISettings _uiSettings = new();
		bool _disposed;

		/// <summary>
		/// Creates a new Windows <see cref="PlatformTicker"/> that respects the "Show animations in Windows" accessibility setting.
		/// </summary>
		public PlatformTicker()
		{
			SystemEnabled = _uiSettings.AnimationsEnabled;
			_uiSettings.AnimationsEnabledChanged += OnAnimationsEnabledChanged;
		}

		/// <inheritdoc/>
		public override void Start()
		{
			if (_disposed)
			{
				return;
			}

			CompositionTarget.Rendering += RenderingFrameEventHandler;
		}

		/// <inheritdoc/>
		public override void Stop()
		{
			CompositionTarget.Rendering -= RenderingFrameEventHandler;
		}

		/// <inheritdoc/>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				Stop();
				_uiSettings.AnimationsEnabledChanged -= OnAnimationsEnabledChanged;
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		void OnAnimationsEnabledChanged(ViewManagement.UISettings sender, ViewManagement.UISettingsAnimationsEnabledChangedEventArgs args)
		{
			SystemEnabled = sender.AnimationsEnabled;
		}

		void RenderingFrameEventHandler(object? sender, object? args)
		{
			Fire?.Invoke();
		}
	}
}