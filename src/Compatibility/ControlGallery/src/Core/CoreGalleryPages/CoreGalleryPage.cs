using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery
{
	internal class CoreGalleryPage<T> : ContentPage
		where T : View, new()
	{
		List<ViewContainer<T>> _viewContainers;

		int _currentIndex;
		Picker _picker;
		Entry _targetEntry;
		StackLayout _layout;

		protected StateViewContainer<T> IsEnabledStateViewContainer { get; private set; }

		protected new StackLayout Layout { get; private set; }

		internal CoreGalleryPage()
		{
			Initialize();

			Layout = new StackLayout
			{
				Padding = new Thickness(20)
			};

			var modalDismissButton = new Button()
			{
				Text = "Dismiss Page",
				Command = new Command(async () =>
			   {
				   if (_picker.SelectedIndex == 0)
				   {
					   await Navigation.PopAsync();
				   }
				   else
				   {
					   _picker.SelectedIndex--;
				   }
			   })
			};
			Layout.Children.Add(modalDismissButton);

			Build(Layout);

			if (SupportsScroll)
				Content = new ScrollView { AutomationId = "GalleryScrollView", Content = Layout };
			else
			{
				var content = new Grid { AutomationId = "GalleryScrollView" };
				content.Children.Add(Layout);
				Content = content;
			}
		}

		protected virtual void Initialize() { }

		protected virtual void InitializeElement(T element) { }

		protected virtual void Build(StackLayout stackLayout)
		{
			var isFocusedView = new T();
			isFocusedView.SetValue(IsFocusedPropertyKey, true, specificity: SetterSpecificity.FromHandler);

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
					isFocusedView.SetValue(IsFocusedPropertyKey, false, specificity: SetterSpecificity.FromHandler);
				}
				else
				{
					isFocusedView.SetValue(IsFocusedPropertyKey, true, specificity: SetterSpecificity.FromHandler);
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
				}
			);

			_viewContainers = new List<ViewContainer<T>> {
				isFocusedStateViewContainer,
				new ViewContainer<T> (Test.VisualElement.BackgroundColor, new T { BackgroundColor = Colors.Blue }),
				new ViewContainer<T>(Test.VisualElement.Background, new T { Background = new LinearGradientBrush
				{
					StartPoint = new Point(0, 0),
					EndPoint = new Point(1, 0),
					GradientStops = new GradientStopCollection
					{
						new GradientStop(Colors.Yellow, 0.0f),
						new GradientStop(Colors.Orange, 0.5f),
						new GradientStop(Colors.Red, 1.0f)
					}
				} }),
				focusStateViewContainer,
				gestureRecognizerEventViewContainer,
				new LayeredViewContainer<T> (Test.VisualElement.InputTransparent, new T { InputTransparent = true }),
				IsEnabledStateViewContainer,
				focusedEventViewContainer,
				unfocusedEventViewContainer,
				isVisibleStateViewContainer,
				new ViewContainer<T> (Test.VisualElement.Opacity, new T { Opacity = 0.5 }),
				new ViewContainer<T> (Test.VisualElement.Rotation, new T { Rotation = 10 }),
				new ViewContainer<T> (Test.VisualElement.RotationX, new T { RotationX = 33 }),
				new ViewContainer<T> (Test.VisualElement.RotationY, new T { RotationY = 10 }),
				new ViewContainer<T> (Test.VisualElement.Scale, new T { Scale = 0.5 }),
				new ViewContainer<T> (Test.VisualElement.TranslationX, new T { TranslationX = 30 }),
				new ViewContainer<T> (Test.VisualElement.TranslationY, new T { TranslationY = 30 }),
			};

			_layout = new StackLayout();

			_targetEntry = new Entry { AutomationId = "TargetViewContainer", Placeholder = "Jump To ViewContainer" };

			var goButton = new Button
			{
				Text = "Go",
				AutomationId = "GoButton"
			};
			goButton.Clicked += GoClicked;

			_picker = new Picker();
			foreach (var container in _viewContainers)
			{
				_picker.Items.Add(container.TitleLabel.Text);
			}

			_picker.SelectedIndex = _currentIndex;

			_picker.SelectedIndexChanged += PickerSelectedIndexChanged;

			_layout.Children.Add(_picker);
			_layout.Children.Add(_targetEntry);
			_layout.Children.Add(goButton);
			_layout.Children.Add(_viewContainers[_currentIndex].ContainerLayout);

			stackLayout.Children.Add(_layout);

			if (!SupportsFocus)
			{
				stackLayout.Children.Remove(focusStateViewContainer.ContainerLayout);
				stackLayout.Children.Remove(isFocusedStateViewContainer.ContainerLayout);
			}

			if (!SupportsTapGestureRecognizer)
			{
				stackLayout.Children.Remove(gestureRecognizerEventViewContainer.ContainerLayout);
			}

			foreach (var element in _viewContainers)
			{
				InitializeElement(element.View);
			}
		}

		void GoClicked(object sender, EventArgs e)
		{
			if (!_viewContainers.Any())
			{
				return;
			}

			var target = _targetEntry.Text;
			_targetEntry.Text = "";
			var index = -1;

			if (string.IsNullOrEmpty(target))
			{
				return;
			}

			for (int n = 0; n < _viewContainers.Count; n++)
			{
				if (_viewContainers[n].View.AutomationId == target)
				{
					index = n;
					break;
				}
			}

			if (index < 0)
			{
				return;
			}

			var targetContainer = _viewContainers[index];

			_layout.Children.RemoveAt(3);
			_layout.Children.Add(targetContainer.ContainerLayout);

			_picker.SelectedIndexChanged -= PickerSelectedIndexChanged;
			_picker.SelectedIndex = index;
			_picker.SelectedIndexChanged += PickerSelectedIndexChanged;
		}

		void PickerSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			_currentIndex = _picker.SelectedIndex;
			_layout.Children.RemoveAt(3);
			_layout.Children.Add(_viewContainers[_currentIndex].ContainerLayout);
		}

		protected virtual bool SupportsTapGestureRecognizer
		{
			get { return true; }
		}

		protected virtual bool SupportsFocus
		{
			get { return true; }
		}

		protected virtual bool SupportsScroll
		{
			get { return true; }
		}

		protected void Add(ViewContainer<T> viewContainer)
		{
			_viewContainers.Add(viewContainer);
			_picker.Items.Add(viewContainer.TitleLabel.Text);
		}
	}
}