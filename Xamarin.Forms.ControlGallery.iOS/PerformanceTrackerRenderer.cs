using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(PerformanceTracker), typeof(PerformanceTrackerRenderer))]
[assembly: ExportRenderer(typeof(ActivityIndicator), typeof(PerformanceTrackingActivityIndicator))]
[assembly: ExportRenderer(typeof(BoxView), typeof(PerformanceTrackingBoxView))]
[assembly: ExportRenderer(typeof(Button), typeof(PerformanceTrackingButton))]
[assembly: ExportRenderer(typeof(DatePicker), typeof(PerformanceTrackingDatePicker))]
[assembly: ExportRenderer(typeof(Editor), typeof(PerformanceTrackingEditor))]
[assembly: ExportRenderer(typeof(Entry), typeof(PerformanceTrackingEntry))]
[assembly: ExportRenderer(typeof(Image), typeof(PerformanceTrackingImage))]
[assembly: ExportRenderer(typeof(Label), typeof(PerformanceTrackingLabel))]
[assembly: ExportRenderer(typeof(ListView), typeof(PerformanceTrackingListView))]
[assembly: ExportRenderer(typeof(Map), typeof(PerformanceTrackingMap))]
[assembly: ExportRenderer(typeof(Picker), typeof(PerformanceTrackingPicker))]
[assembly: ExportRenderer(typeof(ProgressBar), typeof(PerformanceTrackingProgressBar))]
[assembly: ExportRenderer(typeof(SearchBar), typeof(PerformanceTrackingSearchBar))]
[assembly: ExportRenderer(typeof(Slider), typeof(PerformanceTrackingSlider))]
[assembly: ExportRenderer(typeof(Stepper), typeof(PerformanceTrackingStepper))]
[assembly: ExportRenderer(typeof(Switch), typeof(PerformanceTrackingSwitch))]
[assembly: ExportRenderer(typeof(TableView), typeof(PerformanceTrackingTableView))]
[assembly: ExportRenderer(typeof(TimePicker), typeof(PerformanceTrackingTimePicker))]
[assembly: ExportRenderer(typeof(WebView), typeof(PerformanceTrackingWebView))]

namespace Xamarin.Forms.ControlGallery.iOS
{
	public interface IDrawnObservable
	{
		int Drawn
		{
			get; set;
		}
	}

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

	public class PerformanceTrackingListView : ListViewRenderer, IDrawnObservable
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

	public class PerformanceTrackingTableView : TableViewRenderer, IDrawnObservable
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

	public class PerformanceTrackingWebView : WebViewRenderer, IDrawnObservable
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
			if (!PerformanceTrackerRenderer.EnableInstrumentation)
				return;

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