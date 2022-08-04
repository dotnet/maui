using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using MapKit;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Maps.Platform
{
	internal class MauiMKMapView : MKMapView
	{
		IMapHandler? _handler;
		object? _lastTouchedView;

		internal event EventHandler<EventArgs>? LayoutSubviewsFired;

		public MauiMKMapView(IMapHandler handler)
		{
			DidSelectAnnotationView += MkMapViewOnAnnotationViewSelected;
			GetViewForAnnotation = GetViewForAnnotation2;
			_handler = handler;
		}

		public override void LayoutSubviews()
		{
			LayoutSubviewsFired?.Invoke(this, new EventArgs());
			base.LayoutSubviews();
		}

		protected override void Dispose(bool disposing)
		{
			DidSelectAnnotationView -= MkMapViewOnAnnotationViewSelected;
			GetViewForAnnotation = null;
			_handler = null;
			base.Dispose(disposing);
		}

		void MkMapViewOnAnnotationViewSelected(object? sender, MKAnnotationViewEventArgs e)
		{
			var annotation = e.View.Annotation;
			var pin = GetPinForAnnotation(annotation!);

			if (pin != null)
			{
				// SendMarkerClick() returns the value of PinClickedEventArgs.HideInfoWindow
				// Hide the info window by deselecting the annotation
				bool deselect = pin.SendMarkerClick();
				if (deselect)
				{
					DeselectAnnotation(annotation, false);
				}
			}
		}

		IMapPin GetPinForAnnotation(IMKAnnotation annotation)
		{
			IMapPin targetPin = null!;
			IMap map = _handler?.VirtualView!;

			for (int i = 0; i < map.Pins.Count; i++)
			{
				var pin = map.Pins[i];
				if ((IMKAnnotation)pin.MarkerId == annotation)
				{
					targetPin = pin;
					break;
				}
			}

			return targetPin;
		}

		MKAnnotationView GetViewForAnnotation2(MKMapView mapView, IMKAnnotation annotation)
		{
			MKAnnotationView? mapPin;

			// https://bugzilla.xamarin.com/show_bug.cgi?id=26416
			var userLocationAnnotation = Runtime.GetNSObject(annotation.Handle) as MKUserLocation;
			if (userLocationAnnotation != null)
				return null!;

			const string defaultPinId = "defaultPin";
			mapPin = mapView.DequeueReusableAnnotation(defaultPinId);
			if (mapPin == null)
			{
				if (OperatingSystem.IsIOSVersionAtLeast(11))
				{
					mapPin = new MKMarkerAnnotationView(annotation, defaultPinId);
				}
				else
				{
					mapPin = new MKPinAnnotationView(annotation, defaultPinId);

				}

				mapPin.CanShowCallout = true;

				if (OperatingSystem.IsIOSVersionAtLeast(11))
				{
					// Need to set this to get the callout bubble to show up
					// Without this no callout is shown, it's displayed differently
					mapPin.RightCalloutAccessoryView = new UIView();
				}
			}

			mapPin.Annotation = annotation;
			AttachGestureToPin(mapPin, annotation);

			return mapPin;
		}

		void AttachGestureToPin(MKAnnotationView mapPin, IMKAnnotation annotation)
		{
			var recognizers = mapPin.GestureRecognizers;

			if (recognizers != null)
			{
				foreach (var r in recognizers)
				{
					mapPin.RemoveGestureRecognizer(r);
				}
			}

			var recognizer = new UITapGestureRecognizer(g => OnCalloutClicked(annotation))
			{
				ShouldReceiveTouch = (gestureRecognizer, touch) =>
				{
					_lastTouchedView = touch.View;
					return true;
				}
			};

			mapPin.AddGestureRecognizer(recognizer);
		}


		void OnCalloutClicked(IMKAnnotation annotation)
		{
			// lookup pin
			var targetPin = GetPinForAnnotation(annotation);

			// pin not found. Must have been activated outside of forms
			if (targetPin == null)
				return;

			// if the tap happened on the annotation view itself, skip because this is what happens when the callout is showing
			// when the callout is already visible the tap comes in on a different view
			if (_lastTouchedView is MKAnnotationView)
				return;

			targetPin.SendMarkerClick();

			// SendInfoWindowClick() returns the value of PinClickedEventArgs.HideInfoWindow
			// Hide the info window by deselecting the annotation
			bool deselect = targetPin.SendInfoWindowClick();
			if (deselect)
			{
				DeselectAnnotation(annotation, true);
			}
		}

		internal void PinCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddPins(e.NewItems ?? new List<IMapPin>());
					break;
				case NotifyCollectionChangedAction.Remove:
					RemovePins(e.OldItems ?? new List<IMapPin>());
					break;
				case NotifyCollectionChangedAction.Replace:
					RemovePins(e.OldItems ?? new List<IMapPin>());
					AddPins(e.NewItems ?? new List<IMapPin>());
					break;
				case NotifyCollectionChangedAction.Reset:
					if (Annotations?.Length > 0)
						RemoveAnnotations(Annotations);
					AddPins((IList)_handler?.VirtualView.Pins!);
					break;
				case NotifyCollectionChangedAction.Move:
					//do nothing
					break;
			}
		}

		void AddPins(IList pins)
		{
			if (_handler?.MauiContext == null)
				return;

			foreach (IMapPin pin in pins)
			{
				if (pin.ToHandler(_handler.MauiContext).PlatformView is IMKAnnotation annotation)
				{
					pin.MarkerId = annotation;
					AddAnnotation(annotation);
				}
			}
		}

		void RemovePins(IList pins)
		{
			foreach (IMapPin pin in pins)
			{
				RemoveAnnotation((IMKAnnotation)pin.MarkerId);
			}
		}
	}
}
