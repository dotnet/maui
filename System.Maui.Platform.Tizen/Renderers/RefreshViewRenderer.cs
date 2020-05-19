using ElmSharp;
using System;
using System.Reflection;
using System.Threading.Tasks;
using TWebView = Tizen.WebView.WebView;

namespace System.Maui.Platform.Tizen
{
	class RefreshIcon : ContentView
	{
		public static readonly int IconSize = 48;
		static readonly Color DefaultColor = Color.FromHex("#6200EE");
		static readonly string IconPath = "System.Maui.Platform.Tizen.Resource.refresh_48dp.png";

		bool _isPlaying;
		Image _icon;

		public RefreshIcon()
		{
			HeightRequest = IconSize;
			WidthRequest = IconSize;
			var layout = new AbsoluteLayout()
			{
				HeightRequest = IconSize,
				WidthRequest = IconSize,
			};

			layout.Children.Add(new BoxView
			{
				Color = Color.White,
				CornerRadius = new CornerRadius(IconSize),
			}, new Rectangle(0.5, 0.5, IconSize, IconSize), AbsoluteLayoutFlags.PositionProportional);

			_icon = new Image
			{
				Source = ImageSource.FromResource(IconPath, typeof(ShellItemRenderer).Assembly),
			};

			layout.Children.Add(_icon, new Rectangle(0.5, 0.5, IconSize - 8, IconSize - 8), AbsoluteLayoutFlags.PositionProportional);
			Content = layout;

			IconColor = DefaultColor;
		}

		public Color IconColor
		{
			get
			{
				return PlatformConfiguration.TizenSpecific.Image.GetBlendColor(_icon);
			}
			set
			{
				PlatformConfiguration.TizenSpecific.Image.SetBlendColor(_icon, value == Color.Default ? DefaultColor : value);
			}
		}

		public double IconRotation
		{
			get
			{
				return _icon.Rotation;
			}
			set
			{
				_icon.Rotation = value;
			}
		}

		public void Start()
		{
			Stop();
			_isPlaying = true;
			TurnInternal();
		}

		public void Stop()
		{
			_isPlaying = false;
			_icon.AbortAnimation("RotateTo");
		}

		async void TurnInternal()
		{
			await _icon.RelRotateTo(360, 1000);
			if (_isPlaying)
				TurnInternal();
		}
	}

	class RefreshLayout : StackLayout
	{
		static readonly int MaximumDistance = 100;

		public RefreshLayout()
		{
			HeightRequest = 200;
			HorizontalOptions = LayoutOptions.FillAndExpand;

			RefreshIcon = new RefreshIcon
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				TranslationY = -RefreshIcon.IconSize, 
				Opacity = 0.5,
			};
			Children.Add(RefreshIcon);
		}

		RefreshIcon RefreshIcon { get; set; }

		public Color RefreshIconColor
		{
			get => RefreshIcon.IconColor;
			set => RefreshIcon.IconColor = value;
		}

		public void SetDistance(double distance)
		{
			var calculated = -RefreshIcon.IconSize + distance;
			if (calculated > MaximumDistance)
				calculated = MaximumDistance;
			RefreshIcon.TranslationY = calculated;
			RefreshIcon.IconRotation = 180 * (calculated / (float)MaximumDistance);
			RefreshIcon.Opacity = 0.5 + (calculated / (float)MaximumDistance);
		}

		public void Start()
		{
			_ = RefreshIcon.TranslateTo(0, MaximumDistance / 2.0, length:200);
			RefreshIcon.Start();
		}

		public bool ShouldRefresh()
		{
			return RefreshIcon.TranslationY > (MaximumDistance - 30);
		}

		public async Task StopAsync()
		{
			_ = RefreshIcon.FadeTo(0);
			await RefreshIcon.ScaleTo(0.2);
			RefreshIcon.Stop();
		}

		public async Task ResetRefreshIconAsync()
		{
			new Animation((r) =>
			{
				RefreshIcon.IconRotation = 180 * (RefreshIcon.TranslationY / (float)MaximumDistance);
			}).Commit(RefreshIcon, "reset", length: 250);
			_ = RefreshIcon.FadeTo(0.5, length: 250);
			await RefreshIcon.TranslateTo(0, -RefreshIcon.IconSize, length: 250);
		}
	}

	enum RefreshState
	{
		Idle,
		Drag,
		Loading,
	}

	public class RefreshViewRenderer : LayoutRenderer
	{
		GestureLayer _gestureLayer;

		RefreshLayout _refreshLayout;
		IVisualElementRenderer _refreshLayoutRenderer;

		public RefreshViewRenderer()
		{
			RegisterPropertyHandler(RefreshView.RefreshColorProperty, UpdateRefreshColor);
			RegisterPropertyHandler(RefreshView.IsRefreshingProperty, UpdateIsRefreshing);
		}

		RefreshView RefreshView => Element as RefreshView;
		RefreshState RefreshState { get; set; }


		protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
		{
			base.OnElementChanged(e);
			Initialize();
		}

		void Initialize()
		{
			_gestureLayer?.Unrealize();
			_gestureLayer = new GestureLayer(NativeView);
			_gestureLayer.Attach(NativeView);

			_gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Move, OnMoved);
			_gestureLayer.SetMomentumCallback(GestureLayer.GestureState.End, OnEnd);
			_gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Abort, OnEnd);
		}

		void UpdateRefreshLayout()
		{
			_refreshLayout = new RefreshLayout();
			_refreshLayout.RefreshIconColor = RefreshView.RefreshColor;
			_refreshLayoutRenderer = Platform.GetOrCreateRenderer(_refreshLayout);
			(_refreshLayoutRenderer as LayoutRenderer).RegisterOnLayoutUpdated();

			Control.Children.Add(_refreshLayoutRenderer.NativeView);
			var measured = _refreshLayout.Measure(Element.Width, Element.Height);
			var parentBound = NativeView.Geometry;
			var bound = new Rect
			{
				X = parentBound.X,
				Y = parentBound.Y,
				Width = parentBound.Width,
				Height = System.Maui.Maui.ConvertToScaledPixel(measured.Request.Height)
			};

			_refreshLayoutRenderer.NativeView.Geometry = bound;
			RefreshState = RefreshState.Drag;
		}

		bool IsEdgeScrolling()
		{
			if (RefreshView.Content is ScrollView scrollview)
			{
				if (scrollview.ScrollY == 0)
				{
					return true;
				}
			}
			else if (Platform.GetRenderer(RefreshView.Content) is CarouselViewRenderer carouselViewRenderer)
			{
				var collectionView = carouselViewRenderer.NativeView;

				var scroller = collectionView.GetType().GetProperty("Scroller", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(collectionView);

				if (scroller != null)
				{
					if ((scroller as Scroller)?.CurrentRegion.Y == 0)
					{
						return true;
					}
				}
			}
			else if (Platform.GetRenderer(RefreshView.Content) is StructuredItemsViewRenderer itemsViewRenderer)
			{
				var collectionView = itemsViewRenderer.NativeView;

				var scroller = collectionView.GetType().GetProperty("Scroller", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(collectionView);

				if (scroller != null)
				{
					if ((scroller as Scroller)?.CurrentRegion.Y == 0)
					{
						return true;
					}
				}
			}
			else if (Platform.GetRenderer(RefreshView.Content) is ListViewRenderer listViewRenderer)
			{
				if (GetScrollYOnGenList(listViewRenderer.Control.RealHandle) == 0)
				{
					return true;
				}
			}
			else if (Platform.GetRenderer(RefreshView.Content) is WebViewRenderer webviewRenderer)
			{
				if (GetScrollYOnWebView(webviewRenderer.Control.WebView) == 0)
				{
					return true;
				}
			}

			return false;
		}

		int GetScrollYOnGenList(IntPtr handle)
		{
			var interop = typeof(EvasObject).Assembly.GetType("Interop");
			var elementary = interop?.GetNestedType("Elementary", BindingFlags.NonPublic | BindingFlags.Static) ?? null;

			if (elementary != null)
			{
				object[] parameters = new object[] { handle, -1, -1, -1, -1 };
				elementary.GetMethod("elm_scroller_region_get", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, parameters);
				return (int)parameters[2];
			}
			return -1;
		}

		int GetScrollYOnWebView(TWebView webview)
		{
			var property = webview.GetType().GetProperty("ScrollPosition");
			if (property != null)
			{
				var point = (ElmSharp.Point)property.GetValue(webview);
				return point.Y;
			}
			return -1;
		}

		void OnMoved(GestureLayer.MomentumData moment)
		{
			if (RefreshState == RefreshState.Idle)
			{
				if (IsEdgeScrolling())
				{
					UpdateRefreshLayout();
				}
			}

			if (RefreshState == RefreshState.Drag)
			{
				var dy = moment.Y2 - moment.Y1;
				_refreshLayout?.SetDistance(System.Maui.Maui.ConvertToScaledDP(dy));
			}
		}

		void OnEnd(GestureLayer.MomentumData moment)
		{
			if (RefreshState == RefreshState.Drag && _refreshLayout != null && _refreshLayoutRenderer != null)
			{
				if (_refreshLayout.ShouldRefresh())
				{
					_refreshLayout.Start();
					RefreshState = RefreshState.Loading;
					RefreshView.SetValueFromRenderer(RefreshView.IsRefreshingProperty, true);
				}
				else
				{
					_ = ResetRefreshAsync();
				}
			}
		}

		async Task ResetRefreshAsync()
		{
			var refreshLayout = _refreshLayout;
			var refreshIconRenderer = _refreshLayoutRenderer;
			_refreshLayout = null;
			_refreshLayoutRenderer = null;
			await refreshLayout.ResetRefreshIconAsync();
			refreshIconRenderer?.Dispose();
			RefreshState = RefreshState.Idle;
		}

		void UpdateRefreshColor()
		{
			if (_refreshLayout != null)
			{
				_refreshLayout.RefreshIconColor = RefreshView.RefreshColor;
			}
		}

		async void UpdateIsRefreshing(bool init)
		{
			if (init)
				return;

			if (!RefreshView.IsRefreshing && RefreshState == RefreshState.Loading)
			{
				var refreshLayout = _refreshLayout;
				var refreshIconRenderer = _refreshLayoutRenderer;
				_refreshLayout = null;
				_refreshLayoutRenderer = null;
				await refreshLayout?.StopAsync();
				refreshIconRenderer?.Dispose();

				RefreshState = RefreshState.Idle;
			}
		}
	}
}
