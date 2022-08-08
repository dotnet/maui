using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Platform;
using Rect = Microsoft.Maui.Graphics.Rect;
using ERect = ElmSharp.Rect;
using EvasObject = ElmSharp.EvasObject;
using GestureLayer = ElmSharp.GestureLayer;
using Scroller = ElmSharp.Scroller;
using TWebView = Tizen.WebView.WebView;
using XStackLayout = Microsoft.Maui.Controls.StackLayout;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[Obsolete]
	class RefreshIcon : AbsoluteLayout
	{
		public const int IconSize = ThemeConstants.RefreshView.Resources.IconSize;
		static readonly Color DefaultColor = ThemeConstants.RefreshView.ColorClass.DefaultColor;
		const string IconPath = ThemeConstants.RefreshView.Resources.IconPath;

		bool _isPlaying;
		Image _icon;

		public RefreshIcon()
		{
			HeightRequest = IconSize;
			WidthRequest = IconSize;

			Children.Add(new BoxView
			{
				Color = Color.FromRgb(200, 200, 200),
				CornerRadius = new CornerRadius(IconSize),
			}, new Rect(0.5, 0.5, IconSize, IconSize), AbsoluteLayoutFlags.PositionProportional);

			_icon = new Image
			{
				Source = ImageSource.FromResource(IconPath, typeof(ShellItemRenderer).Assembly),
			};

			Children.Add(_icon, new Rect(0.5, 0.5, IconSize - 8, IconSize - 8), AbsoluteLayoutFlags.PositionProportional);

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
				PlatformConfiguration.TizenSpecific.Image.SetBlendColor(_icon, value == null ? DefaultColor : value);
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

	[Obsolete]
	class RefreshLayout : XStackLayout
	{
		static readonly int MaximumDistance = 100;

		public RefreshLayout()
		{
			HeightRequest = 200;
			HorizontalOptions = LayoutOptions.Fill;

			RefreshIcon = new RefreshIcon
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				TranslationY = -RefreshIcon.IconSize,
				Opacity = 0.5,
			};
			Children.Add(RefreshIcon);
			Children.Add(new BoxView
			{
				HeightRequest = 200
			});
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
			_ = RefreshIcon.TranslateTo(0, MaximumDistance / 2.0, length: 200);
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

	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class RefreshViewRenderer : LayoutRenderer
	{
		ElmSharp.GestureLayer _gestureLayer;

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
			_refreshLayout.Parent = Element;
			_refreshLayout.RefreshIconColor = RefreshView.RefreshColor;
			_refreshLayoutRenderer = Platform.GetOrCreateRenderer(_refreshLayout);
			(_refreshLayoutRenderer as ILayoutRenderer).RegisterOnLayoutUpdated();

			Control.Children.Add(_refreshLayoutRenderer.NativeView);
			var measured = _refreshLayout.Measure(Element.Width, Element.Height);
			var parentBound = NativeView.Geometry;
			var bound = new ERect
			{
				X = parentBound.X,
				Y = parentBound.Y,
				Width = parentBound.Width,
				Height = Forms.ConvertToScaledPixel(measured.Request.Height)
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

		//TODO: the following method is not trimming safe
		[UnconditionalSuppressMessage("Trimming", "IL2026")]
		[UnconditionalSuppressMessage("Trimming", "IL2075")]
		[UnconditionalSuppressMessage("Trimming", "IL2091")]
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
				_refreshLayout?.SetDistance(Forms.ConvertToScaledDP(dy));
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
