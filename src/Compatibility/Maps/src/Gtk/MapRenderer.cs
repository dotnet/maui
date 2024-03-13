using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Platform;
using Circle = Microsoft.Maui.Controls.Maps.Circle;
using IList = System.Collections.IList;
using Math = System.Math;
using Polygon = Microsoft.Maui.Controls.Maps.Polygon;
using Polyline = Microsoft.Maui.Controls.Maps.Polyline;

namespace Microsoft.Maui.Controls.Compatibility.Maps.Gtk
{
	public class MapView : NotImplementedView { }

	public class MapRenderer : Handlers.Compatibility.ViewRenderer<Map, MapView>
	{
		const string MoveMessageName = "MapMoveToRegion";

		bool _disposed;


		public MapRenderer() : base()
		{
			AutoPackage = false;
		}

		protected new Map Map => Element;


		protected override Size MinimumSize()
		{
			return new Size(40);
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(40), new Size(40));
		}

		protected override MapView CreateNativeControl()
		{
			return new MapView();
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				if (Element != null)
				{
#pragma warning disable CS0618 // Type or member is obsolete
					MessagingCenter.Unsubscribe<Map, MapSpan>(this, MoveMessageName);
#pragma warning restore CS0618 // Type or member is obsolete

					((ObservableCollection<Pin>)Element.Pins).CollectionChanged -= OnPinCollectionChanged;
					foreach (Pin pin in Element.Pins)
					{
						pin.PropertyChanged -= PinOnPropertyChanged;
					}

					((ObservableCollection<MapElement>)Element.MapElements).CollectionChanged -= OnMapElementCollectionChanged;
					foreach (MapElement child in Element.MapElements)
					{
						child.PropertyChanged -= MapElementPropertyChanged;
					}
				}


				base.Dispose(disposing);
			}
		}

		void PinOnPropertyChanged(object sender, PropertyChangedEventArgs e) { }

		void MapElementPropertyChanged(object sender, PropertyChangedEventArgs e) { }

		void OnMapElementCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { }

		void OnPinCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { }

		protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
		{
			base.OnElementChanged(e);

			MapView oldMapView = Control;

			MapView mapView = CreateNativeControl();

			if (e.OldElement != null)
			{
				Map oldMapModel = e.OldElement;

				((ObservableCollection<Pin>)oldMapModel.Pins).CollectionChanged -= OnPinCollectionChanged;
				foreach (Pin pin in oldMapModel.Pins)
				{
					pin.PropertyChanged -= PinOnPropertyChanged;
				}

				((ObservableCollection<MapElement>)oldMapModel.MapElements).CollectionChanged -= OnMapElementCollectionChanged;
				foreach (MapElement child in oldMapModel.MapElements)
				{
					child.PropertyChanged -= MapElementPropertyChanged;
				}

#pragma warning disable CS0618 // Type or member is obsolete
				MessagingCenter.Unsubscribe<Map, MapSpan>(this, MoveMessageName);
#pragma warning restore CS0618 // Type or member is obsolete


				oldMapView?.Dispose();
			}

#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<Map, MapSpan>(this, MoveMessageName, OnMoveToRegionMessage, Map);
#pragma warning restore CS0618 // Type or member is obsolete

			((INotifyCollectionChanged)Map.Pins).CollectionChanged += OnPinCollectionChanged;
			((INotifyCollectionChanged)Map.MapElements).CollectionChanged += OnMapElementCollectionChanged;
		}

		void OnMoveToRegionMessage(Map arg1, MapSpan arg2)
		{
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Map.MapTypeProperty.PropertyName)
			{
				return;
			}


			if (e.PropertyName == Map.IsShowingUserProperty.PropertyName) { }
			else if (e.PropertyName == Map.IsScrollEnabledProperty.PropertyName) { }
			else if (e.PropertyName == Map.IsZoomEnabledProperty.PropertyName) { }
			else if (e.PropertyName == Map.IsTrafficEnabledProperty.PropertyName) { }
		}
	}
}