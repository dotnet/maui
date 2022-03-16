using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;
using Microsoft.Maui.Platform;
using AndroidX.Window.Layout;
using Android.Content.Res;

namespace Microsoft.Maui.Foldable
{
	class FoldableService : IFoldableService, IFoldableContext
	{
		#region IFoldableContext properties
		public bool IsSeparating
		{
			get { return _isSpanned; }
			set
			{
				_isSpanned = value;
				FoldLayoutChanged();
			}
		}

		public Rect FoldingFeatureBounds
		{
			get
			{
				global::Android.Util.Log.Debug("JWM2", $"DualScreenServiceImpl.getFoldingFeatureBounds _hingeDp:{_hingeDp}");
				return _hingeDp;
			}
			set
			{
				global::Android.Util.Log.Debug("JWM2", $"DualScreenServiceImpl.setFoldingFeatureBounds value:{value}");
				_hingeDp = value;
				FoldLayoutChanged();
			}
		}

		internal void OnConfigurationChanged(Activity activity, Configuration configuration)
		{
			// set window size after rotation
			var bounds = wmc.ComputeCurrentWindowMetrics(activity).Bounds;
			var rect = new Rect(bounds.Left, bounds.Top, bounds.Width(), bounds.Height());
			WindowBounds = rect;
			consumer.SetWindowSize(rect);
			Update();

			//TODO: do we need to update the screen here???
			_helper?.Update();
			_onScreenChangedEventManager.HandleEvent(this, EventArgs.Empty, nameof(OnScreenChanged));
		}

		internal void OnStart(Activity activity)
		{
			foldContext = activity.GetWindow().Handler.MauiContext.Services.GetService(
								typeof(IFoldableContext)) as IFoldableContext; // DualScreenServiceImpl instance

			consumer.SetFoldableContext(foldContext); // so that we can update it on each message

			// set window size first time - sets WindowBounds
			var bounds = wmc.ComputeCurrentWindowMetrics(activity).Bounds;
			var rect = new Rect(bounds.Left, bounds.Top, bounds.Width(), bounds.Height());
			consumer.SetWindowSize(rect);

			wit.AddWindowLayoutInfoListener(activity, runOnUiThreadExecutor(), consumer); // `consumer` is the IConsumer implementation declared below
		}

		internal void OnStop(Activity activity)
		{
			wit.RemoveWindowLayoutInfoListener(consumer);
		}

		internal void OnCreate(Activity activity)
		{
			Init(this, activity);
			wit = new AndroidX.Window.Java.Layout.WindowInfoTrackerCallbackAdapter(
			AndroidX.Window.Layout.WindowInfoTracker.Companion.GetOrCreate(
				activity));

			wmc = WindowMetricsCalculator.Companion.OrCreate; // source method `getOrCreate` is munged by NuGet auto-binding
		}

		#region Used by WindowInfoTracker callback
		static Java.Util.Concurrent.IExecutor runOnUiThreadExecutor()
		{
			return new MyExecutor();
		}
		class MyExecutor : Java.Lang.Object, Java.Util.Concurrent.IExecutor
		{
			global::Android.OS.Handler handler = new global::Android.OS.Handler(global::Android.OS.Looper.MainLooper);
			public void Execute(Java.Lang.IRunnable r)
			{
				handler.Post(r);
			}
		}
		#endregion

		public Rect WindowBounds
		{
			get { return _windowBounds; }
			set
			{
				_windowBounds = value;
				global::Android.Util.Log.Info("JWM2", $"=== FoldingFeatureChanged?.Invoke isSeparating:{IsSeparating} window:{WindowBounds}");
				FoldLayoutChanged();
			}
		}
		void FoldLayoutChanged()
		{
			OnLayoutChanged?.Invoke(this, new Microsoft.Maui.Foldable.FoldEventArgs()
			{
				isSeparating = IsSeparating,
				FoldingFeatureBounds = FoldingFeatureBounds,
				WindowBounds = WindowBounds
			});
		}

		bool _isSpanned = false;
		Rect _hingeDp = Rect.Zero;
		Rect _windowBounds = Rect.Zero;
		#endregion
		Activity _mainActivity;
		IFoldableContext _foldableInfo;

		#region Hinge
		object _hingeAngleLock = new object();
		HingeSensor _singleUseHingeSensor;
		TaskCompletionSource<int> _gettingHingeAngle;
		FoldableService _HingeService;
		static HingeSensor DefaultHingeSensor;
		#endregion

		ScreenHelper _helper;
		Size _pixelScreenSize;
		Consumer consumer;
		AndroidX.Window.Java.Layout.WindowInfoTrackerCallbackAdapter wit = null; // for hinge/fold
		IWindowMetricsCalculator wmc = null; // for window dimensions
		IFoldableContext foldContext; // DualScreenServiceImpl instance

		readonly WeakEventManager _onScreenChangedEventManager = new WeakEventManager();

		/// <summary>Event triggered when the app is spanned or unspanned or rotated while spanned</summary>
		public event System.EventHandler<FoldEventArgs> OnLayoutChanged;

		[Controls.Internals.Preserve(Conditional = true)]
		public FoldableService()
		{
			_HingeService = this;
		}

		void Init(IFoldableContext foldableInfo, Activity activity = null)
		{
			consumer ??= new Consumer(activity);
			if (_foldableInfo == null)
			{
				_foldableInfo = foldableInfo;

				if (_foldableInfo != null)
				{
					_hingeDp = _foldableInfo.FoldingFeatureBounds; // convert to DP?
					_isSpanned = _foldableInfo.IsSeparating;
					_windowBounds = _foldableInfo.WindowBounds;
					Update(); // calculate dp from px for hinge coordinates
				}

				if (_HingeService == null)
				{
					_mainActivity = activity;
					return;
				}

				if (activity == _mainActivity && _HingeService._helper != null)
				{
					_HingeService?.Update();
					return;
				}

				_mainActivity = activity;

				if (_mainActivity == null)
					return;

				var screenHelper = _HingeService._helper ?? new ScreenHelper(foldableInfo, _mainActivity);

				_HingeService._helper = screenHelper;
				_HingeService.SetupHingeSensors(_mainActivity);

				_HingeService?.Update();
			}
		}


		public Size ScaledScreenSize
		{
			get;
			private set;
		}

		public event EventHandler OnScreenChanged
		{
			add { _onScreenChangedEventManager.AddEventHandler(value); }
			remove { _onScreenChangedEventManager.RemoveEventHandler(value); }
		}

		void Update()
		{
			var windowBounds = WindowBounds;

			if (windowBounds.Height == 00 && windowBounds.Width == 0)
				return;

			//HACK:FOLDABLE
			_isSpanned = _helper?.IsDualMode ?? false;

			// Hinge
			if (!_isSpanned)
			{
				_hingeDp = Rect.Zero;
			}
			else // IsSpanned
			{
				var hinge = _helper.GetHingeBoundsDip();

				if (hinge == Rect.Zero || !IsSpanned)
				{
					_hingeDp = Rect.Zero;
				}
				else
				{
					//TODO: verify this works with zero-size hinge (foldable devices)
					_hingeDp = new Rect((hinge.Left), (hinge.Top), (hinge.Width), (hinge.Height));
				}
			}

			_helper.FoldingFeatureBounds = FoldingFeatureBounds;
			_helper.IsSpanned = IsSeparating;

			_pixelScreenSize = new Size(windowBounds.Width, windowBounds.Height);

			if (_mainActivity is Activity)
			{
				ScaledScreenSize = new Size(
					_mainActivity.FromPixels(_pixelScreenSize.Width),
					_mainActivity.FromPixels(_pixelScreenSize.Height));
			}

			Update(); //TODO: confirm timing this update

			var args = new FoldEventArgs()
			{
				FoldingFeatureBounds = FoldingFeatureBounds,
				isSeparating = IsSeparating,
				WindowBounds = WindowBounds
			};

			OnLayoutChanged?.Invoke(this, args);
		}

		public bool IsSpanned => _isSpanned;

		public Task<int> GetHingeAngleAsync()
		{
			Task<int> returnValue = null;
			lock (_hingeAngleLock)
			{
				if (_gettingHingeAngle == null)
				{
					_gettingHingeAngle = new TaskCompletionSource<int>();
					StartListeningForHingeChanges();
				}

				returnValue = _gettingHingeAngle.Task;
			}

			return returnValue;
		}

		public Rect GetHinge() => _hingeDp;

		/// <summary>
		/// I question whether we should be basing anything on landscape-ness, and 
		/// instead should be using the orientation of the hinge
		/// </summary>
		public bool IsLandscape
		{
			get
			{
				if (_mainActivity == null)
					return false;
				else
				{
					using (global::Android.Util.DisplayMetrics display = (_mainActivity as Activity).Resources.DisplayMetrics)
					{
						return (display.WidthPixels >= display.HeightPixels);
					}
				}
			}

		}

		public Point? GetLocationOnScreen(VisualElement visualElement)
		{
			var androidView = visualElement.Handler?.PlatformView as AView;

			if (!androidView.IsAlive())
				return null;

			int[] location = new int[2];
			androidView.GetLocationOnScreen(location);
			return new Point(androidView.Context.FromPixels(location[0]), androidView.Context.FromPixels(location[1]));
		}

		public object WatchForChangesOnLayout(VisualElement visualElement, Action action)
		{
			if (action == null || _mainActivity == null)
				return null;

			var androidView = visualElement.Handler?.PlatformView as AView;

			if (androidView == null || !androidView.IsAlive())
				return null;

			var listener = new DualScreenGlobalLayoutListener(action, androidView, _mainActivity);

			var table = new System.Runtime.CompilerServices.ConditionalWeakTable<AView, DualScreenGlobalLayoutListener>();
			androidView.ViewTreeObserver.AddOnGlobalLayoutListener(listener);
			table.Add(androidView, listener);
			return table;
		}

		public void StopWatchingForChangesOnLayout(VisualElement visualElement, object handle)
		{
			if (!(handle is System.Runtime.CompilerServices.ConditionalWeakTable<AView, DualScreenGlobalLayoutListener> table))
				return;

			DualScreenGlobalLayoutListener ggl = null;

			var androidView = visualElement.Handler?.PlatformView as AView;

			if (androidView == null || !(table.TryGetValue(androidView, out ggl)))
			{
				foreach (var pair in table)
					ggl = pair.Value;
			}

			if (ggl == null)
				return;

			try
			{
				ggl.Invalidate();
			}
			catch
			{
				// just in case something along the call path here is disposed of
			}

			if (androidView == null || !androidView.IsAlive())
				return;

			try
			{
				androidView.ViewTreeObserver.RemoveOnGlobalLayoutListener(ggl);
			}
			catch
			{
				// just in case something along the call path here is disposed of
			}
		}

		class DualScreenGlobalLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
		{
			WeakReference<Action> _callback;
			WeakReference<AView> _view;
			WeakReference<Activity> _activity;

			public DualScreenGlobalLayoutListener(Action callback, AView view, Activity activity)
			{
				_callback = new WeakReference<Action>(callback);
				_view = new WeakReference<AView>(view);
				_activity = new WeakReference<Activity>(activity);
			}

			public void OnGlobalLayout()
			{
				if (_view == null || _callback == null)
					return;

				Action invokeMe = null;
				AView view = null;

				if (!_view.TryGetTarget(out view) || !view.IsAlive())
				{
					Invalidate(view);
				}
				else if (_callback.TryGetTarget(out invokeMe))
				{
					invokeMe();
				}
				else
				{
					Invalidate(view);
				}
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
					Invalidate(null);

				base.Dispose(disposing);
			}

			internal void Invalidate()
			{
				AView view = null;
				_view.TryGetTarget(out view);
				Invalidate(view);
			}

			// I don't want our code to dispose of this class I'd rather just let the natural
			// process manage the life cycle so we don't dispose of this too early
			void Invalidate(AView androidView)
			{
				if (androidView.IsAlive())
				{
					try
					{
						androidView.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
					}
					catch
					{
						// just in case something along the call path here is disposed of
					}
				}

				try
				{
					// If the androidView itself becomes disposed of the listener will survive the life of the view
					// and it will get moved to the root views tree observer
					if (this.IsAlive() && _activity.TryGetTarget(out Activity activity))
					{
						activity.Window?.DecorView?.RootView?.ViewTreeObserver?.RemoveOnGlobalLayoutListener(this);
					}
				}
				catch
				{
					// just in case something along the call path here is disposed of
				}

				_callback = null;
				_view = null;
			}
		}

		static EventHandler<HingeSensor.HingeSensorChangedEventArgs> _hingeAngleChanged;
		static int subscriberCount;
		static object hingeAngleLock = new object();

		public static event EventHandler<HingeSensor.HingeSensorChangedEventArgs> HingeAngleChanged
		{
			add
			{
				if (DefaultHingeSensor == null)
					return;

				ProcessHingeAngleSubscriberCount(Interlocked.Increment(ref subscriberCount));
				_hingeAngleChanged += value;
			}
			remove
			{
				if (DefaultHingeSensor == null)
					return;

				ProcessHingeAngleSubscriberCount(Interlocked.Decrement(ref subscriberCount));
				_hingeAngleChanged -= value;
			}
		}

		static void ProcessHingeAngleSubscriberCount(int subscriberCount)
		{
			var sensor = DefaultHingeSensor;
			if (sensor == null)
				return;

			lock (hingeAngleLock)
			{
				if (subscriberCount == 1)
				{
					sensor.StartListening();
				}
				else if (subscriberCount == 0)
				{
					sensor.StopListening();
				}
			}
		}

		void DefaultHingeSensorOnSensorChanged(object sender, HingeSensor.HingeSensorChangedEventArgs e)
		{
			_hingeAngleChanged?.Invoke(this, e);
		}

		void SetupHingeSensors(global::Android.Content.Context context)
		{
			if (context == null)
			{
				if (DefaultHingeSensor != null)
					DefaultHingeSensor.OnSensorChanged -= DefaultHingeSensorOnSensorChanged;

				_singleUseHingeSensor = null;
				DefaultHingeSensor = null;
			}
			else
			{
				_singleUseHingeSensor = new HingeSensor(context);
				DefaultHingeSensor = new HingeSensor(context);
				DefaultHingeSensor.OnSensorChanged += DefaultHingeSensorOnSensorChanged;
			}
		}


		void StartListeningForHingeChanges()
		{
			if (_singleUseHingeSensor is null)
				return;

			_singleUseHingeSensor.OnSensorChanged += OnSensorChanged;
			_singleUseHingeSensor.StartListening();
		}

		void StopListeningForHingeChanges()
		{
			if (_singleUseHingeSensor is null)
				return;

			_singleUseHingeSensor.OnSensorChanged -= OnSensorChanged;
			_singleUseHingeSensor.StopListening();
		}

		void OnSensorChanged(object sender, HingeSensor.HingeSensorChangedEventArgs e)
		{
			SetHingeAngle(e.HingeAngle);
		}

		void SetHingeAngle(int hingeAngle)
		{
			TaskCompletionSource<int> toSet = null;
			lock (_hingeAngleLock)
			{
				StopListeningForHingeChanges();
				toSet = _gettingHingeAngle;
				_gettingHingeAngle = null;
			}

			if (toSet != null)
				toSet.SetResult(hingeAngle);
		}
	}


	//HACK:FOLDABLE added this from Microsoft.Maui namespace, because otherwise it
	// wasn't getting picked up as valid extension methods otherwise...
	static class JavaObjectExtensions
	{
		public static bool IsDisposed(this Java.Lang.Object obj)
		{
			return obj.Handle == IntPtr.Zero;
		}

		public static bool IsDisposed(this global::Android.Runtime.IJavaObject obj)
		{
			return obj.Handle == IntPtr.Zero;
		}

		public static bool IsAlive(this Java.Lang.Object obj)
		{
			if (obj == null)
				return false;

			return !obj.IsDisposed();
		}

		public static bool IsAlive(this global::Android.Runtime.IJavaObject obj)
		{
			if (obj == null)
				return false;

			return !obj.IsDisposed();
		}
	}
}