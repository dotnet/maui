using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	internal class CoreGalleryPage<T> : CoreGalleryBasePage<T>
		where T : View, new()
	{
		protected StateViewContainer<T> IsEnabledStateViewContainer { get; private set; }

		protected virtual bool SupportsTapGestureRecognizer => true;

		protected virtual bool SupportsFocus => true;

		/// <summary>
		/// Code that needs to run for each element that was added during the build.
		/// </summary>
		/// <param name="element">The element to initialize.</param>
		protected virtual void InitializeElement(T element) { }

		protected override void Build()
		{
			var isFocusedView = new T();
			isFocusedView.SetValueFromRenderer(IsFocusedPropertyKey.BindableProperty, true);

			IsEnabledStateViewContainer = new StateViewContainer<T>(Test.VisualElement.IsEnabled, new T { IsEnabled = true });
			IsEnabledStateViewContainer.StateChangeButton.Command = new Command(() =>
			{
				IsEnabledStateViewContainer.View.IsEnabled = !IsEnabledStateViewContainer.View.IsEnabled;
			});

			var isVisibleStateViewContainer = new StateViewContainer<T>(Test.VisualElement.IsVisible, new T { IsVisible = true });
			isVisibleStateViewContainer.StateChangeButton.Command = new Command(() =>
			{
				isVisibleStateViewContainer.View.IsVisible = !isVisibleStateViewContainer.View.IsVisible;
			});

			var isFocusedStateViewContainer = new StateViewContainer<T>(Test.VisualElement.IsFocused, isFocusedView);
			isFocusedStateViewContainer.StateChangeButton.Command = new Command(() =>
			{
				if ((bool)isFocusedView.GetValue(VisualElement.IsFocusedProperty))
				{
					isFocusedView.SetValueFromRenderer(IsFocusedPropertyKey.BindableProperty, false);
				}
				else
				{
					isFocusedView.SetValueFromRenderer(IsFocusedPropertyKey.BindableProperty, true);
				}
			});

			var focusStateViewContainer = new StateViewContainer<T>(Test.VisualElement.Focus, new T());
			focusStateViewContainer.StateChangeButton.Command = new Command(() =>
			{
				if (focusStateViewContainer.View.IsFocused)
				{
					focusStateViewContainer.View.Unfocus();
				}
				else
				{
					focusStateViewContainer.View.Focus();
				}
			});

			var focusedEventViewContainer = new EventViewContainer<T>(Test.VisualElement.Focused, new T());
			focusedEventViewContainer.View.Focused += (sender, args) => focusedEventViewContainer.EventFired();

			var unfocusedEventViewContainer = new EventViewContainer<T>(Test.VisualElement.Unfocused, new T());
			unfocusedEventViewContainer.View.Unfocused += (sender, args) => unfocusedEventViewContainer.EventFired();

			var gestureRecognizerEventViewContainer = new EventViewContainer<T>(Test.View.GestureRecognizers, new T());
			gestureRecognizerEventViewContainer.View.GestureRecognizers.Add(
				new TapGestureRecognizer
				{
					Command = new Command(() => gestureRecognizerEventViewContainer.EventFired())
				});

			if (SupportsFocus)
			{
				Add(isFocusedStateViewContainer);
				Add(focusStateViewContainer);
			}

			Add(new ViewContainer<T>(Test.VisualElement.BackgroundColor, new T { BackgroundColor = Colors.Blue }));
			Add(new ViewContainer<T>(Test.VisualElement.Background, new T
			{
				Background = new LinearGradientBrush
				{
					StartPoint = new Point(0, 0),
					EndPoint = new Point(1, 0),
					GradientStops = new GradientStopCollection
						{
							new GradientStop(Colors.Yellow, 0.0f),
							new GradientStop(Colors.Orange, 0.5f),
							new GradientStop(Colors.Red, 1.0f)
						}
				}
			}));

			if (SupportsTapGestureRecognizer)
				Add(gestureRecognizerEventViewContainer);

			Add(new LayeredViewContainer<T>(Test.VisualElement.InputTransparent, new T { InputTransparent = true }));
			Add(new LayeredViewContainer<T>(Test.VisualElement.NotInputTransparent, new T { InputTransparent = false }));
			Add(IsEnabledStateViewContainer);
			Add(focusedEventViewContainer);
			Add(unfocusedEventViewContainer);
			Add(isVisibleStateViewContainer);
			Add(new ViewContainer<T>(Test.VisualElement.Opacity, new T { Opacity = 0.5 }));
			Add(new ViewContainer<T>(Test.VisualElement.Rotation, new T { Rotation = 10 }));
			Add(new ViewContainer<T>(Test.VisualElement.RotationX, new T { RotationX = 33 }));
			Add(new ViewContainer<T>(Test.VisualElement.RotationY, new T { RotationY = 10 }));
			Add(new ViewContainer<T>(Test.VisualElement.Scale, new T { Scale = 0.5 }));
			Add(new ViewContainer<T>(Test.VisualElement.TranslationX, new T { TranslationX = 30 }));
			Add(new ViewContainer<T>(Test.VisualElement.TranslationY, new T { TranslationY = 30 }));

			foreach (var element in ViewContainers)
			{
				InitializeElement(element.View);
			}
		}
	}
}
