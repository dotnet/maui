#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Gtk;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform
{

	// ported from https://github.com/xamarin/Xamarin.Forms/blob/5.0.0/Xamarin.Forms.Platform.GTK/VisualElementTracker.cs
	class GesturePlatformManager : IDisposable
	{

		readonly IPlatformViewHandler _handler;
		readonly NotifyCollectionChangedEventHandler _collectionChangedHandler;
		Widget? _container;
		Widget? _control;
		VisualElement? _element;

		bool _isDisposed;

		public GesturePlatformManager(IViewHandler handler)
		{
			_handler = (IPlatformViewHandler)handler;
			_collectionChangedHandler = ModelGestureRecognizersOnCollectionChanged;

			if (_handler.VirtualView == null)
				throw new ArgumentNullException(nameof(handler.VirtualView));

			if (_handler.PlatformView == null)
				throw new ArgumentNullException(nameof(handler.PlatformView));

			Control = _handler.PlatformView;

			if (_handler.ContainerView != null)
				Container = _handler.ContainerView;
			else
				Container = _handler.PlatformView;
		}

		public Widget? Container
		{
			get { return _container; }
			set
			{
				if (_container == value)
					return;

				if (_container != null)
				{
					_container.ButtonPressEvent -= OnContainerButtonPressEvent;
				}

				_container = value;

				UpdatingGestureRecognizers();
			}
		}

		public Widget? Control
		{
			get { return _control; }
			set
			{
				if (_control == value)
					return;

				if (_control != null)
				{
					_control.ButtonPressEvent -= OnControlButtonPressEvent;
				}

				_control = value;

				if (PreventGestureBubbling)
				{
					UpdatingGestureRecognizers();
				}
			}
		}

		public VisualElement? Element
		{
			get { return _element; }
			set
			{
				if (_element == value)
					return;

				if (_element != null)
				{
					var view = _element as View;

					if (view != null)
					{
						var oldRecognizers = (ObservableCollection<IGestureRecognizer>)view.GestureRecognizers;
						oldRecognizers.CollectionChanged -= _collectionChangedHandler;
					}
				}

				_element = value;

				if (_element != null)
				{
					var view = _element as View;

					if (view != null)
					{
						var newRecognizers = (ObservableCollection<IGestureRecognizer>)view.GestureRecognizers;
						newRecognizers.CollectionChanged += _collectionChangedHandler;
					}
				}
			}
		}

		private bool PreventGestureBubbling
		{
			get
			{
				return Element switch
				{
					Button => true,
					CheckBox => true,
					DatePicker => true,
					Stepper => true,
					Slider => true,
					Switch => true,
					TimePicker => true,
					ImageButton => true,
					RadioButton => true,
					_ => false,
				};
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (!disposing)
				return;

			if (_container != null)
			{
				_container.ButtonPressEvent -= OnContainerButtonPressEvent;
			}

			if (_element != null)
			{
				var view = _element as View;

				if (view != null)
				{
					var oldRecognizers = (ObservableCollection<IGestureRecognizer>)view.GestureRecognizers;
					oldRecognizers.CollectionChanged -= _collectionChangedHandler;
				}
			}

			if (_control != null)
			{
				_control.ButtonPressEvent -= OnControlButtonPressEvent;
			}

			_container = null;
			_control = null;
			_element = null;
		}

		private void ModelGestureRecognizersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			UpdatingGestureRecognizers();
		}

		private void UpdatingGestureRecognizers()
		{
			var view = Element as View;
			var gestures = view?.GestureRecognizers;

			if (_container == null || gestures == null)
				return;

			_container.ButtonPressEvent -= OnContainerButtonPressEvent;

			if (gestures.GetGesturesFor<TapGestureRecognizer>().Any() || gestures.GetGesturesFor<PointerGestureRecognizer>().Any())
			{
				_container.ButtonPressEvent += OnContainerButtonPressEvent;
			}
			else
			{
				if (_control != null && PreventGestureBubbling)
				{
					_control.ButtonPressEvent += OnControlButtonPressEvent;
				}
			}
		}

		private void OnContainerButtonPressEvent(object sender, ButtonPressEventArgs args)
		{
			var button = args.Event.Button;

			if (button != 1 && button != 3)
			{
				return;
			}

			var view = Element as View;

			if (view == null)
				return;

			int numClicks = 0;

			switch (args.Event.Type)
			{
				case Gdk.EventType.ThreeButtonPress:
					numClicks = 3;

					break;
				case Gdk.EventType.TwoButtonPress:
					numClicks = 2;

					break;
				case Gdk.EventType.ButtonPress:
					numClicks = 1;

					break;
				default:
					return;
			}

			// Taps or Clicks
			if (button == (uint)ButtonsMask.Primary)
			{
				IEnumerable<TapGestureRecognizer> tapGestures = view.GestureRecognizers
				   .GetGesturesFor<TapGestureRecognizer>(recognizer => recognizer.NumberOfTapsRequired == numClicks);

				foreach (TapGestureRecognizer recognizer in tapGestures)
					recognizer.SendTapped(view);

			}

			IEnumerable<PointerGestureRecognizer> clickGestures = view.GestureRecognizers
			   .GetGesturesFor<PointerGestureRecognizer>();

			foreach (PointerGestureRecognizer recognizer in clickGestures)
				recognizer.SendPointerPressed(view, element => new Point(args.Event.X, args.Event.Y), new PlatformPointerEventArgs());

		}

		private void OnControlButtonPressEvent(object sender, ButtonPressEventArgs args)
		{
			args.RetVal = true;
		}

	}

}