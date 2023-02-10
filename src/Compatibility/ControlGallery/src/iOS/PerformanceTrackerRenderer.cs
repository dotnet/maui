using System;
using System.Collections.Generic;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS;
using Microsoft.Maui.Controls.Compatibility.Maps.iOS;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Maps;
using ObjCRuntime;
using UIKit;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: ExportRenderer(typeof(PerformanceTracker), typeof(PerformanceTrackerRenderer))]
[assembly: ExportRenderer(typeof(ActivityIndicator), typeof(PerformanceTrackingActivityIndicator))]
[assembly: ExportRenderer(typeof(BoxView), typeof(PerformanceTrackingBoxView))]
[assembly: ExportRenderer(typeof(Button), typeof(PerformanceTrackingButton))]
[assembly: ExportRenderer(typeof(ImageButton), typeof(PerformanceTrackingImageButton))]
[assembly: ExportRenderer(typeof(CheckBox), typeof(PerformanceTrackingCheckBox))]
[assembly: ExportRenderer(typeof(DatePicker), typeof(PerformanceTrackingDatePicker))]
[assembly: ExportRenderer(typeof(Editor), typeof(PerformanceTrackingEditor))]
[assembly: ExportRenderer(typeof(Entry), typeof(PerformanceTrackingEntry))]
[assembly: ExportRenderer(typeof(Image), typeof(PerformanceTrackingImage))]
[assembly: ExportRenderer(typeof(Label), typeof(PerformanceTrackingLabel))]
[assembly: ExportRenderer(typeof(Picker), typeof(PerformanceTrackingPicker))]
[assembly: ExportRenderer(typeof(ProgressBar), typeof(PerformanceTrackingProgressBar))]
[assembly: ExportRenderer(typeof(SearchBar), typeof(PerformanceTrackingSearchBar))]
[assembly: ExportRenderer(typeof(Slider), typeof(PerformanceTrackingSlider))]
[assembly: ExportRenderer(typeof(Stepper), typeof(PerformanceTrackingStepper))]
[assembly: ExportRenderer(typeof(Switch), typeof(PerformanceTrackingSwitch))]
[assembly: ExportRenderer(typeof(TimePicker), typeof(PerformanceTrackingTimePicker))]
[assembly: ExportRenderer(typeof(WebView), typeof(PerformanceTrackingWebView))]
#pragma warning restore CS0612 // Type or member is obsolete
[assembly: ExportRenderer(typeof(ListView), typeof(PerformanceTrackingListView))]
[assembly: ExportRenderer(typeof(Map), typeof(PerformanceTrackingMap))]
[assembly: ExportRenderer(typeof(TableView), typeof(PerformanceTrackingTableView))]
[assembly: ExportRenderer(typeof(Frame), typeof(PerformanceTrackingFrame))]

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS
{
	public interface IDrawnObservable
	{
		int Drawn
		{
			get; set;
		}
	}

	[System.Obsolete]
	public class PerformanceTrackerRenderer : ViewRenderer
	{
		public const string SubviewAddedMessage = "SubviewAdded";
		public static bool EnableInstrumentation = false;
		List<IDisposable> _observers = new List<IDisposable>();
		bool _SagaStarted = false;

		public PerformanceTrackerRenderer()
		{
			EnableInstrumentation = true;
			MessagingCenter.Instance.Subscribe<IDrawnObservable>(this, SubviewAddedMessage, HandleSubviewAdded);
		}

		PerformanceTracker PerformanceTracker => Element as PerformanceTracker;

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			StartSaga();
		}

		protected override void Dispose(bool disposing)
		{
			DisposeObservers();
			MessagingCenter.Instance.Unsubscribe<IDrawnObservable>(this, SubviewAddedMessage);
			EnableInstrumentation = false;

			base.Dispose(disposing);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == PerformanceTracker.ContentProperty.PropertyName)
			{
				PerformanceTracker.Watcher.ResetTest();
			}
		}

		void DisposeObservers()
		{
			foreach (IDisposable observer in _observers)
			{
				observer.Dispose();
			}
			_observers.Clear();
		}

		void HandleDrawnObserved(NSObservedChange obj)
		{
			PerformanceTracker.Watcher.WaitForComplete();
		}

		void HandleSubviewAdded(IDrawnObservable obj)
		{
			StartSaga();
		}

		void StartSaga()
		{
			if (_SagaStarted)
			{
				PerformanceTracker.Watcher.WaitForComplete();
				return;
			}

			_SagaStarted = true;
			PerformanceTracker.Watcher.BeginTest(init: () => SubscribeToDrawn(this), cleanup: () =>
			{
				DisposeObservers();
				_SagaStarted = false;
			});
		}

		void SubscribeToDrawn(UIView elem)
		{
			if (elem is IDrawnObservable)
			{
				_observers.Add(elem.AddObserver(nameof(IDrawnObservable.Drawn), NSKeyValueObservingOptions.OldNew, HandleDrawnObserved));
			}

			foreach (var child in elem.Subviews)
			{
				SubscribeToDrawn(child);
			}
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingActivityIndicator : ActivityIndicatorRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingActivityIndicator> _watcher;
		int _Drawn;

		public PerformanceTrackingActivityIndicator()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingActivityIndicator>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingBoxView : BoxRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingBoxView> _watcher;
		int _Drawn;

		public PerformanceTrackingBoxView()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingBoxView>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	public class PerformanceTrackingFrame : Handlers.Compatibility.FrameRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingFrame> _watcher;
		int _Drawn;

		public PerformanceTrackingFrame()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingFrame>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingButton : ButtonRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingButton> _watcher;
		int _Drawn;

		public PerformanceTrackingButton()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingButton>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingImageButton : ImageButtonRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingImageButton> _watcher;
		int _Drawn;

		public PerformanceTrackingImageButton()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingImageButton>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingCheckBox : CheckBoxRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingCheckBox> _watcher;
		int _Drawn;

		public PerformanceTrackingCheckBox()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingCheckBox>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingDatePicker : DatePickerRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingDatePicker> _watcher;
		int _Drawn;

		public PerformanceTrackingDatePicker()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingDatePicker>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingEditor : EditorRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingEditor> _watcher;
		int _Drawn;

		public PerformanceTrackingEditor()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingEditor>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingEntry : EntryRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingEntry> _watcher;
		int _Drawn;

		public PerformanceTrackingEntry()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingEntry>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingImage : ImageRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingImage> _watcher;
		int _Drawn;

		public PerformanceTrackingImage()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingImage>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();

			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingLabel : LabelRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingLabel> _watcher;
		int _Drawn;

		public PerformanceTrackingLabel()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingLabel>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher?.Dispose();

			base.Dispose(disposing);
		}
	}

	public class PerformanceTrackingListView : Handlers.Compatibility.ListViewRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingListView> _watcher;
		int _Drawn;

		public PerformanceTrackingListView()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingListView>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();

			base.Dispose(disposing);
		}
	}

	public class PerformanceTrackingMap : MapRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingMap> _watcher;
		int _Drawn;

		public PerformanceTrackingMap()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingMap>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingPicker : PickerRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingPicker> _watcher;
		int _Drawn;

		public PerformanceTrackingPicker()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingPicker>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingProgressBar : ProgressBarRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingProgressBar> _watcher;
		int _Drawn;

		public PerformanceTrackingProgressBar()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingProgressBar>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingSearchBar : SearchBarRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingSearchBar> _watcher;
		int _Drawn;

		public PerformanceTrackingSearchBar()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingSearchBar>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingSlider : SliderRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingSlider> _watcher;
		int _Drawn;

		public PerformanceTrackingSlider()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingSlider>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingStepper : StepperRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingStepper> _watcher;
		int _Drawn;

		public PerformanceTrackingStepper()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingStepper>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingSwitch : SwitchRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingSwitch> _watcher;
		int _Drawn;

		public PerformanceTrackingSwitch()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingSwitch>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	public class PerformanceTrackingTableView : Handlers.Compatibility.TableViewRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingTableView> _watcher;
		int _Drawn;

		public PerformanceTrackingTableView()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingTableView>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingTimePicker : TimePickerRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingTimePicker> _watcher;
		int _Drawn;

		public PerformanceTrackingTimePicker()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingTimePicker>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	[System.Obsolete]
	public class PerformanceTrackingWebView : WkWebViewRenderer, IDrawnObservable
	{
		readonly SubviewWatcher<PerformanceTrackingWebView> _watcher;
		int _Drawn;

		public PerformanceTrackingWebView()
		{
			_watcher = new SubviewWatcher<PerformanceTrackingWebView>(this);
		}

		[Export(nameof(IDrawnObservable.Drawn))]
		public int Drawn
		{
			get { return _Drawn; }
			set
			{
				WillChangeValue(nameof(IDrawnObservable.Drawn));
				_Drawn = value;
				DidChangeValue(nameof(IDrawnObservable.Drawn));
			}
		}

		[Export("getLayerTransformString", ArgumentSemantic.Retain)]
		public NSString GetLayerTransformString
		{
			get => new NSString(Layer.Transform.ToString());
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			Drawn++;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			MessagingCenter.Instance.Send((IDrawnObservable)this, PerformanceTrackerRenderer.SubviewAddedMessage);
			_watcher.SubscribeToDrawn(this);
		}

		protected override void Dispose(bool disposing)
		{
			_watcher.Dispose();
			base.Dispose(disposing);
		}
	}

	public class SubviewWatcher<T> : IDisposable where T : UIView, IDrawnObservable
	{
		readonly T _View;
		List<IDisposable> _observers = new List<IDisposable>();

		public SubviewWatcher(T view)
		{
			_View = view;
		}

		public void Dispose()
		{
			DisposeObservers();
		}

		public void SubscribeToDrawn(UIView elem)
		{
#pragma warning disable CS0612 // Type or member is obsolete
			if (!PerformanceTrackerRenderer.EnableInstrumentation)
				return;
#pragma warning restore CS0612 // Type or member is obsolete

			if (elem != _View && elem is IDrawnObservable)
			{
				_observers.Add(elem.AddObserver(nameof(IDrawnObservable.Drawn), NSKeyValueObservingOptions.OldNew, HandleDrawnObserved));
			}

			foreach (var child in elem.Subviews)
			{
				SubscribeToDrawn(child);
			}
		}

		void DisposeObservers()
		{
			foreach (IDisposable observer in _observers)
			{
				observer.Dispose();
			}
			_observers.Clear();
		}

		void HandleDrawnObserved(NSObservedChange obj)
		{
			_View.Drawn++;
		}
	}
}
