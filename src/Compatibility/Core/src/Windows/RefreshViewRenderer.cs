using System;
using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.RefreshView;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class RefreshViewRenderer : ViewRenderer<RefreshView, RefreshContainer>
	{
		bool _isDisposed;
		Deferral _refreshCompletionDeferral;

		public RefreshViewRenderer()
		{
			AutoPackage = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (Control != null && disposing)
			{
				Control.RefreshRequested -= OnRefresh;

				if (_refreshCompletionDeferral != null)
				{
					_refreshCompletionDeferral.Complete();
					_refreshCompletionDeferral.Dispose();
					_refreshCompletionDeferral = null;
				}
			}

			_isDisposed = true;

			base.Dispose(disposing);
		}

		bool _isLoaded = false;
		protected override void OnElementChanged(ElementChangedEventArgs<RefreshView> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var refreshControl = new RefreshContainer
					{
						PullDirection = RefreshPullDirection.TopToBottom
					};

					refreshControl.RefreshRequested += OnRefresh;
					refreshControl.Loaded += OnLoaded;

					// Telling the refresh to start before the control has been sized
					// causes no refresh circle to show up
					void OnLoaded(object sender, object args)
					{
						refreshControl.Loaded -= OnLoaded;
						_ = Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
						{
							_isLoaded = true;
							UpdateIsRefreshing();
						});
					}

					// There's a bug with RefreshContainer where if you assign the Visualizer
					// yourself on creation it will cause RefreshRequested to fire twice
					// https://github.com/microsoft/microsoft-ui-xaml/issues/1282
					long callbackToken = 0;
					callbackToken = refreshControl.RegisterPropertyChangedCallback(RefreshContainer.VisualizerProperty,
						(_, __) =>
						{
							if (refreshControl?.Visualizer == null)
								return;

							UpdateColors();
							refreshControl.UnregisterPropertyChangedCallback(RefreshContainer.VisualizerProperty, callbackToken);
						});

					SetNativeControl(refreshControl);
				}

				UpdateContent();
				UpdateIsEnabled();
				UpdateRefreshPullDirection();
			}

			base.OnElementChanged(e);
		}



		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ContentView.ContentProperty.PropertyName)
				UpdateContent();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsEnabled();
			else if (e.PropertyName == RefreshView.IsRefreshingProperty.PropertyName)
				UpdateIsRefreshing();
			else if (e.PropertyName == RefreshView.RefreshColorProperty.PropertyName)
				UpdateColors();
			else if (e.PropertyName == Specifics.RefreshPullDirectionProperty.PropertyName)
				UpdateRefreshPullDirection();
		}

		protected override void UpdateBackgroundColor()
		{
			if (Element == null || Control?.Visualizer == null)
				return;

			if (Element.BackgroundColor != Color.Default)
				Control.Visualizer.Background = Element.BackgroundColor.ToBrush();
			else
				Control.Visualizer.Background = Color.White.ToBrush();
		}

		void UpdateContent()
		{
			if (Element.Content == null)
				return;

			IVisualElementRenderer renderer = Element.Content.GetOrCreateRenderer();
			Control.Content = renderer.ContainerElement;
		}

		void UpdateIsEnabled()
		{
			Control.IsEnabled = Element.IsEnabled;
		}

		void UpdateIsRefreshing()
		{
			if (!_isLoaded)
				return;

			if (!Element?.IsRefreshing??false)
			{
				CompleteRefresh();
			}
			else if (_refreshCompletionDeferral == null)
			{
				Control?.RequestRefresh();
			}
		}

		void UpdateColors()
		{
			if (Control?.Visualizer == null)
				return;

			Control.Visualizer.Foreground = Element.RefreshColor != Color.Default
				? Element.RefreshColor.ToBrush()
				: (WBrush)Microsoft.UI.Xaml.Application.Current.Resources["DefaultTextForegroundThemeBrush"];

			UpdateBackgroundColor();
		}

		void UpdateRefreshPullDirection()
		{
			if (Element.IsSet(Specifics.RefreshPullDirectionProperty))
			{
				var refreshPullDirection = Element.OnThisPlatform().GetRefreshPullDirection();

				switch (refreshPullDirection)
				{
					case Specifics.RefreshPullDirection.TopToBottom:
						Control.PullDirection = RefreshPullDirection.TopToBottom;
						break;
					case Specifics.RefreshPullDirection.BottomToTop:
						Control.PullDirection = RefreshPullDirection.BottomToTop;
						break;
					case Specifics.RefreshPullDirection.LeftToRight:
						Control.PullDirection = RefreshPullDirection.LeftToRight;
						break;
					case Specifics.RefreshPullDirection.RightToLeft:
						Control.PullDirection = RefreshPullDirection.RightToLeft;
						break;
					default:
						goto case Specifics.RefreshPullDirection.TopToBottom;
				}
			}
		}

		void CompleteRefresh()
		{
			if (_refreshCompletionDeferral != null)
			{
				_refreshCompletionDeferral.Complete();
				_refreshCompletionDeferral.Dispose();
				_refreshCompletionDeferral = null;
			}
		}

		void OnRefresh(object sender, RefreshRequestedEventArgs args)
		{
			CompleteRefresh();
			_refreshCompletionDeferral = args.GetDeferral();
			Element.SetValueFromRenderer(RefreshView.IsRefreshingProperty, true);
		}
	}
}
