using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Util;
using Android.Views;
using Microsoft.Device.Display;
using Xamarin.Forms;
using Xamarin.Forms.DualScreen;
using Xamarin.Forms.Platform.Android;

[assembly: Dependency(typeof(DualScreenService.DualScreenServiceImpl))]

namespace Xamarin.Forms.DualScreen
{
	public class DualScreenService
	{
		[Internals.Preserve(Conditional = true)]
		public DualScreenService()
		{

		}

		public static void Init(Activity activity)
		{
			DependencyService.Register<DualScreenServiceImpl>();
			DualScreenServiceImpl.Init(activity);
		}

		internal class DualScreenServiceImpl : IDualScreenService, Platform.Android.DualScreen.IDualScreenService
		{
			ScreenHelper _helper;
			bool _isDuo = false;
			bool IsDuo => (_helper == null || _HingeService == null || _mainActivity == null || _singleUseHingeSensor == null) ? false : _isDuo;
			HingeSensor _singleUseHingeSensor;
			static Activity _mainActivity;
			static DualScreenServiceImpl _HingeService;
			bool _isLandscape;
			Size _pixelScreenSize;
			object _hingeAngleLock = new object();
			TaskCompletionSource<int> _gettingHingeAngle;

			internal static Activity MainActivity => _mainActivity;

			static HingeSensor DefaultHingeSensor;
			public event EventHandler OnScreenChanged;

			[Internals.Preserve(Conditional = true)]
			public DualScreenServiceImpl()
			{
				_HingeService = this;
				if (_mainActivity != null)
					Init(_mainActivity);
			}

			public static void Init(Activity activity)
			{
				if (_HingeService == null)
				{
					_mainActivity = activity;
					return;
				}

				if (activity == _mainActivity && _HingeService._helper != null)
					return;

				if (_mainActivity is IDeviceInfoProvider oldDeviceInfoProvider)
					oldDeviceInfoProvider.ConfigurationChanged -= _HingeService.ConfigurationChanged;

				_mainActivity = activity;

				if (_mainActivity == null)
					return;

				bool isDuo = _HingeService._isDuo = ScreenHelper.IsDualScreenDevice(_mainActivity);
				if (!isDuo)
					return;

				var screenHelper = _HingeService._helper ?? new ScreenHelper();
				isDuo = screenHelper.Initialize(_mainActivity);
				_HingeService._isDuo = isDuo;

				if (!isDuo)
				{
					_HingeService._helper = null;
					_HingeService.SetupHingeSensors(null);
					return;
				}

				_HingeService._helper = screenHelper;
				_HingeService.SetupHingeSensors(_mainActivity);
				if (_mainActivity is IDeviceInfoProvider newDeviceInfoProvider)
				{
					newDeviceInfoProvider.ConfigurationChanged += _HingeService.ConfigurationChanged;
				}
			}

			public Size ScaledScreenSize
			{
				get;
				private set;
			}

			public bool IsSpanned
				=> IsDuo && (_helper?.IsDualMode ?? false);

			public Task<int> GetHingeAngleAsync()
			{
				if (!IsDuo)
					return Task.FromResult(0);

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

			public Rectangle GetHinge()
			{
				if (!IsDuo)
					return Rectangle.Zero;
								
				var hinge = _helper.GetHingeBoundsDip();

				if (hinge == null)
					return Rectangle.Zero;
				
				var hingeDp = new Rectangle((hinge.Left), (hinge.Top), (hinge.Width()), (hinge.Height()));
				
				return hingeDp;
			}

			public bool IsLandscape
			{
				get
				{
					if (!IsDuo)
					{
						if (_mainActivity == null)
							return false;

						var orientation = _mainActivity.Resources.Configuration.Orientation;
						return orientation == global::Android.Content.Res.Orientation.Landscape;
					}

					var rotation = ScreenHelper.GetRotation(_helper.Activity);
					return (rotation == SurfaceOrientation.Rotation270 || rotation == SurfaceOrientation.Rotation90);
				}
			}

			public Point? GetLocationOnScreen(VisualElement visualElement)
			{
				var view = Platform.Android.Platform.GetRenderer(visualElement);

				if (view?.View == null)
					return null;

				int[] location = new int[2];
				view.View.GetLocationOnScreen(location);
				return new Point(view.View.Context.FromPixels(location[0]), view.View.Context.FromPixels(location[1]));
			}

			public object WatchForChangesOnLayout(VisualElement visualElement, Action action)
			{
				if (action == null)
					return null;

				var view = Platform.Android.Platform.GetRenderer(visualElement);
				var androidView = view?.View;

				if (androidView == null || !androidView.IsAlive())
					return null;

				ViewTreeObserver.IOnGlobalLayoutListener listener = null;
				listener = new GenericGlobalLayoutListener(() =>
				{
					if (!androidView.IsAlive())
					{
						action = null;
						androidView = null;
						try
						{
							_mainActivity?.Window?.DecorView?.RootView?.ViewTreeObserver?.RemoveOnGlobalLayoutListener(listener);
						}
						catch
						{
							// just in case something along the call path here is disposed of
						}

						return;
					}

					action?.Invoke();
				});

				androidView.ViewTreeObserver.AddOnGlobalLayoutListener(listener);
				return listener;
			}

			public void StopWatchingForChangesOnLayout(VisualElement visualElement, object handle)
			{
				if (handle == null)
					return;

				var view = Platform.Android.Platform.GetRenderer(visualElement);
				var androidView = view?.View;

				if (androidView == null || !androidView.IsAlive())
					return;

				if (handle is ViewTreeObserver.IOnGlobalLayoutListener vto)
				{
					try
					{
						view.View.ViewTreeObserver.RemoveOnGlobalLayoutListener(vto);
					}
					catch
					{
						// just in case something along the call path here is disposed of
					}
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

				lock(hingeAngleLock)
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

			void ConfigurationChanged(object sender, EventArgs e)
			{
				if (IsDuo)
					_helper?.Update();

				bool screenChanged = false;
				if (_isLandscape != IsLandscape)
				{
					_isLandscape = IsLandscape;
					screenChanged = true;
				}

				if (_mainActivity != null)
				{
					using (DisplayMetrics display = _mainActivity.Resources.DisplayMetrics)
					{
						var scalingFactor = display.Density;
						_pixelScreenSize = new Size(display.WidthPixels, display.HeightPixels);
						var newSize = new Size(_pixelScreenSize.Width / scalingFactor, _pixelScreenSize.Height / scalingFactor);

						if (newSize != ScaledScreenSize)
						{
							ScaledScreenSize = newSize;
							screenChanged = true;
						}
					}
				}

				if (screenChanged)
					OnScreenChanged?.Invoke(this, e);
			}


			void StartListeningForHingeChanges()
			{
				if (!IsDuo)
					return;

				_singleUseHingeSensor.OnSensorChanged += OnSensorChanged;
				_singleUseHingeSensor.StartListening();
			}

			void StopListeningForHingeChanges()
			{
				if (!IsDuo)
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
	}
}