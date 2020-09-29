using System;
using System.ComponentModel;
using Android.Views;
using Xamarin.Forms.Internals;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	public sealed class VisualElementRenderer : IDisposable, IEffectControlProvider, ITabStop
	{
		bool _disposed;

		IVisualElementRenderer _renderer;
		readonly GestureManager _gestureManager;
		readonly AutomationPropertiesProvider _automationPropertiesProvider;
		readonly EffectControlProvider _effectControlProvider;

		public VisualElementRenderer(IVisualElementRenderer renderer)
		{
			_renderer = renderer;
			_renderer.ElementPropertyChanged += OnElementPropertyChanged;
			_renderer.ElementChanged += OnElementChanged;
			_gestureManager = new GestureManager(_renderer);
			_automationPropertiesProvider = new AutomationPropertiesProvider(_renderer);

			_effectControlProvider = new EffectControlProvider(_renderer?.View);
		}

		VisualElement Element => _renderer?.Element;

		AView Control => _renderer?.View;

		AView ITabStop.TabStop => Control;

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			_effectControlProvider.RegisterEffect(effect);
		}

		void UpdateFlowDirection()
		{
			if (_disposed)
				return;

			Control.UpdateFlowDirection(Element);
		}

		public bool OnTouchEvent(MotionEvent e)
		{
			return _gestureManager.OnTouchEvent(e);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				EffectUtilities.UnregisterEffectControlProvider(this, Element);

				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;
				}

				if (_renderer != null)
				{
					_renderer.ElementChanged -= OnElementChanged;
					_renderer.ElementPropertyChanged -= OnElementPropertyChanged;
					_renderer = null;
				}

				_gestureManager?.Dispose();
				_automationPropertiesProvider?.Dispose();
			}
		}

		void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			Performance.Start(out string reference);
			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (e.NewElement != null)
			{
				e.NewElement.PropertyChanged += OnElementPropertyChanged;
				UpdateFlowDirection();
				UpdateIsEnabled();
			}

			EffectUtilities.RegisterEffectControlProvider(this, e.OldElement, e.NewElement);
			Performance.Stop(reference);
		}

		void UpdateIsEnabled()
		{
			if (Element == null || _disposed)
			{
				return;
			}

			Control.Enabled = Element.IsEnabled;
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
			{
				UpdateFlowDirection();
			}
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
			{
				UpdateIsEnabled();
			}
		}
	}
}