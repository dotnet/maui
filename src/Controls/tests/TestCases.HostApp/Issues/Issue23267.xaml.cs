using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
#if ANDROID
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Widget;
#endif

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 23267, "CornerRadius of GradientDrawable doesn't work anymore for ImageButton", PlatformAffected.Android)]
	public partial class Issue23267 : ContentPage
	{
		Microsoft.Maui.Controls.ScrollView scrollView;
		VerticalStackLayout verticalStackLayout;
		Grid gridLayout;
		Microsoft.Maui.Controls.ImageButton imageButton;
		public Issue23267()
		{
			scrollView = new Microsoft.Maui.Controls.ScrollView();
			verticalStackLayout = new VerticalStackLayout() { Padding = new Thickness(30, 0), Spacing = 25 };
			verticalStackLayout.Add(new Label() { Text = "ImageButton:" });
			imageButton = new Microsoft.Maui.Controls.ImageButton()
			{
				AutomationId = "ImageButtonId",
				Source = "dotnet_bot.png",
				WidthRequest = 185,
				HeightRequest = 185,
				Aspect = Aspect.AspectFit

			};
#if ANDROID
			imageButton.Behaviors.Add(new RippleBehavior() { BackgroundColor = Colors.Red, RippleColor = Colors.Green, CornerRadius = 360 });
#endif
			verticalStackLayout.Add(imageButton);
			verticalStackLayout.Add(new Label() { Text = "Grid with Image:" });

			gridLayout = new Grid()
			{
				HeightRequest = 185,
				WidthRequest = 185
			};

#if ANDROID
			gridLayout.Behaviors.Add(new RippleBehavior() { BackgroundColor = Colors.Red, RippleColor = Colors.Green, CornerRadius = 360 });
#endif
			gridLayout.Add(new Image()
			{
				Source = "dotnet_bot.png",

				Aspect = Aspect.AspectFit
			});
			verticalStackLayout.Add(gridLayout);


			this.Content = scrollView;
		}
	}

#if ANDROID
	public class RippleBehavior : PlatformBehavior<Microsoft.Maui.Controls.View, Android.Views.View>
	{
		private Color _backgroundColor = Colors.Transparent;
		private Color _rippleColor = Colors.Transparent;
		private float _cornerRadius = 0f;

		private Microsoft.Maui.Controls.View _bindable = null;
		private Android.Views.View _platformView;
		private Android.Views.ViewGroup _platformGroup = null;
		private Android.Views.View _rippleView = null;

		private GradientDrawable _backgroundDrawable = null;
		private RippleDrawable _rippleDrawable = null;

		private object _platformLock = new object();

		public static readonly BindableProperty IsAttachedProperty =
			BindableProperty.CreateAttached("IsAttached", typeof(bool), typeof(RippleBehavior), true, propertyChanged: OnAttachedPropertyChanged);

		public static readonly BindableProperty BackgroundColorProperty =
			BindableProperty.CreateAttached("BackgroundColor", typeof(Color), typeof(RippleBehavior), Colors.Transparent, propertyChanged: OnAttachedPropertyChanged);

		public static readonly BindableProperty CornerRadiusProperty =
			BindableProperty.CreateAttached("CornerRadius", typeof(float), typeof(RippleBehavior), 0f, propertyChanged: OnAttachedPropertyChanged);

		public static readonly BindableProperty RippleColorProperty =
			BindableProperty.CreateAttached("RippleColor", typeof(Color), typeof(RippleBehavior), Colors.Transparent, propertyChanged: OnAttachedPropertyChanged);

		public static bool GetIsAttached(BindableObject view)
		{
			return (bool)view.GetValue(IsAttachedProperty);
		}

		public static void SetIsAttached(BindableObject view, bool value)
		{
			view.SetValue(IsAttachedProperty, value);
		}

		public static Color GetBackgroundColor(BindableObject view)
		{
			return (Color)view.GetValue(BackgroundColorProperty);
		}

		public static void SetBackgroundColor(BindableObject view, Color value)
		{
			view.SetValue(BackgroundColorProperty, value);
		}

		public Color BackgroundColor
		{
			get
			{
				return _backgroundColor;
			}
			set
			{
				if (_backgroundColor != value)
				{
					_backgroundColor = value;

					lock (_platformLock)
						CreateRipple();
				}
			}
		}

		public static float GetCornerRadius(BindableObject view)
		{
			return (float)view.GetValue(CornerRadiusProperty);
		}

		public static void SetCornerRadius(BindableObject view, float value)
		{
			view.SetValue(CornerRadiusProperty, value);
		}

		public float CornerRadius
		{
			get
			{
				return _cornerRadius;
			}
			set
			{
				if (_cornerRadius != value)
				{
					_cornerRadius = value;

					lock (_platformLock)
						CreateRipple();
				}
			}
		}

		public static Color GetRippleColor(BindableObject view)
		{
			return (Color)view.GetValue(RippleColorProperty);
		}

		public static void SetRippleColor(BindableObject view, Color value)
		{
			view.SetValue(RippleColorProperty, value);
		}

		public Color RippleColor
		{
			get
			{
				return _rippleColor;
			}
			set
			{
				if (_rippleColor != value)
				{
					_rippleColor = value;

					lock (_platformLock)
						CreateRipple();
				}
			}
		}

		private void DestroyRipple()
		{
			if (_rippleView is object)
			{
				_platformGroup?.RemoveView(_rippleView);

				_rippleView.Pressed = false;
				_rippleView.Background = null;
				_rippleView.Dispose();
			}

			_backgroundDrawable?.Dispose();
			_rippleDrawable?.Dispose();

			_backgroundDrawable = null;
			_rippleDrawable = null;
			_rippleView = null;
		}

		private void CreateRipple()
		{
			if (_platformView is object)
			{
				if (_platformView.Background is RippleDrawable ripple)
				{
					ripple.SetColor(ColorStateList.ValueOf(Android.Graphics.Color.LightGray));
				}
				else
				{
					DestroyRipple();

					_backgroundDrawable = new GradientDrawable();

					_backgroundDrawable.SetColors(new int[] { Android.Graphics.Color.LightGray, Android.Graphics.Color.LightGray });
					_backgroundDrawable.SetCornerRadius(_cornerRadius);

					_rippleDrawable = new RippleDrawable(ColorStateList.ValueOf(Android.Graphics.Color.LightGray), _backgroundDrawable, _backgroundDrawable);

					var stateList = new StateListDrawable();
					stateList.AddState(new[] { Android.Resource.Attribute.StatePressed }, _rippleDrawable);

					if (_platformGroup is null)
					{
						_platformView.SetBackground(stateList);
					}
					else
					{
						_rippleView = new FrameLayout(_platformGroup.Context!);
						_rippleView.SetBackground(stateList);

						_platformGroup.AddView(_rippleView, 0);
					}
				}
			}
		}

		private static void OnAttachedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var element = bindable as VisualElement;

			if (element is null)
				return;

			var isAttached = RippleBehavior.GetIsAttached(element);
			var backgroundColor = RippleBehavior.GetBackgroundColor(element);
			var cornerRadius = RippleBehavior.GetCornerRadius(element);
			var rippleColor = RippleBehavior.GetRippleColor(element);

			var behavior = element.Behaviors.FirstOrDefault(b => b is RippleBehavior) as RippleBehavior;

			if (isAttached)
			{
				if (behavior is null)
					element.Behaviors.Add(new RippleBehavior()
					{
						BackgroundColor = backgroundColor,
						CornerRadius = cornerRadius,
						RippleColor = rippleColor
					});
				else
				{
					behavior.BackgroundColor = backgroundColor;
					behavior.CornerRadius = cornerRadius;
					behavior.RippleColor = rippleColor;
				}
			}
			else
			{
				element.Behaviors.Remove(behavior);
			}
		}

		protected override void OnAttachedTo(Microsoft.Maui.Controls.View bindable, Android.Views.View platformView)
		{
			lock (_platformLock)
			{
				base.OnAttachedTo(bindable, platformView);

				_bindable = bindable;
				_platformView = platformView;
				_platformGroup = platformView as Android.Views.ViewGroup;

				CreateRipple();

				_platformView.Clickable = true;
				_platformView.LayoutChange += OnLayoutChange;
			}
		}

		protected override void OnDetachedFrom(Microsoft.Maui.Controls.View bindable, Android.Views.View platformView)
		{
			lock (_platformLock)
			{
				base.OnDetachedFrom(bindable, platformView);

				_platformView!.LayoutChange -= OnLayoutChange;

				DestroyRipple();

				_bindable = null;
				_platformView = null;
				_platformGroup = null;
			}
		}

		private void OnLayoutChange(object sender, Android.Views.View.LayoutChangeEventArgs e)
		{
			lock (_platformLock)
			{
				if (_rippleView is object)
				{
					_rippleView.Right = _platformView!.Width;
					_rippleView.Bottom = _platformView.Height;
				}
			}
		}
	}

#endif
}