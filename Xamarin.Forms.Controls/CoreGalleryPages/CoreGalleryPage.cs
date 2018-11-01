using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class CoreGalleryPage<T> : ContentPage
		where T : View, new ()
	{
		List<View> _views;
		int _currentIndex;
		Picker _picker;
		StackLayout _moveNextStack;

		ViewContainer<T> _backgroundColorViewContainer;
		ViewContainer<T> _opacityViewContainer;
		ViewContainer<T> _rotationViewContainer;
		ViewContainer<T> _rotationXViewContainer;
		ViewContainer<T> _rotationYViewContainer;
		ViewContainer<T> _scaleViewContainer;
		ViewContainer<T> _translationXViewContainer;
		ViewContainer<T> _translationYViewContainer;

		StateViewContainer<T> _focusStateViewContainer;
		StateViewContainer<T> _isFocusedStateViewContainer;
		StateViewContainer<T> _isVisibleStateViewContainer;

		EventViewContainer<T> _gestureRecognizerEventViewContainer;
		EventViewContainer<T> _focusedEventViewContainer;
		EventViewContainer<T> _unfocusedEventViewContainer;

		LayeredViewContainer<T> _inputTransparentViewContainer;

		protected StateViewContainer<T> IsEnabledStateViewContainer { get; private set; }

		protected StackLayout Layout { get; private set; }

		internal CoreGalleryPage ()
		{
			Layout = new StackLayout {
				Padding = new Thickness (20)
			};

			var modalDismissButton = new Button () {
				Text = "Dismiss Page",
				Command = new Command (async () =>
				{
					if (_picker.SelectedIndex == 0)
						await Navigation.PopAsync();
					else
						_picker.SelectedIndex--;
				})
			};
			Layout.Children.Add (modalDismissButton);

			Build (Layout);

			Content = new ScrollView { AutomationId = "GalleryScrollView", Content = Layout };

		}

		protected virtual void InitializeElement (T element) {}

		protected virtual void Build (StackLayout stackLayout)
		{
			var isFocusedView = new T ();
			isFocusedView.SetValueCore (IsFocusedPropertyKey, true);

			var viewContainers = new[] {
				_isFocusedStateViewContainer = new StateViewContainer<T> (Test.VisualElement.IsFocused, isFocusedView),
				_backgroundColorViewContainer = new ViewContainer<T> (Test.VisualElement.BackgroundColor, new T { BackgroundColor = Color.Blue }),
				_focusStateViewContainer = new StateViewContainer<T> (Test.VisualElement.Focus, new T ()),
				_gestureRecognizerEventViewContainer = new EventViewContainer<T> (Test.View.GestureRecognizers, new T ()),
				_inputTransparentViewContainer = new LayeredViewContainer<T> (Test.VisualElement.InputTransparent, new T { InputTransparent = true }),
				IsEnabledStateViewContainer = new StateViewContainer<T> (Test.VisualElement.IsEnabled, new T { IsEnabled = true }),
				_focusedEventViewContainer = new EventViewContainer<T> (Test.VisualElement.Focused, new T ()),
				_unfocusedEventViewContainer = new EventViewContainer<T> (Test.VisualElement.Unfocused, new T ()),
				_isVisibleStateViewContainer = new StateViewContainer<T> (Test.VisualElement.IsVisible, new T { IsVisible = true }),
				_opacityViewContainer = new ViewContainer<T> (Test.VisualElement.Opacity, new T { Opacity = 0.5 }),
				_rotationViewContainer = new ViewContainer<T> (Test.VisualElement.Rotation, new T { Rotation = 10 }),
				_rotationXViewContainer = new ViewContainer<T> (Test.VisualElement.RotationX, new T { RotationX = 33 }),
				_rotationYViewContainer = new ViewContainer<T> (Test.VisualElement.RotationY, new T { RotationY = 10 }),
				_scaleViewContainer = new ViewContainer<T> (Test.VisualElement.Scale, new T { Scale = 0.5 }),
				_translationXViewContainer = new ViewContainer<T> (Test.VisualElement.TranslationX, new T { TranslationX = 30 }),
				_translationYViewContainer = new ViewContainer<T> (Test.VisualElement.TranslationY, new T { TranslationY = 30 }),
			};

			// Set state
			IsEnabledStateViewContainer.StateChangeButton.Command = new Command (() => {
				IsEnabledStateViewContainer.View.IsEnabled = !IsEnabledStateViewContainer.View.IsEnabled;
			});
			_isVisibleStateViewContainer.StateChangeButton.Command = new Command (() => {
				_isVisibleStateViewContainer.View.IsVisible = !_isVisibleStateViewContainer.View.IsVisible;
			});

			_focusStateViewContainer.StateChangeButton.Command = new Command (() => {
				if (_focusStateViewContainer.View.IsFocused) {
					_focusStateViewContainer.View.Unfocus ();
				} else {
					_focusStateViewContainer.View.Focus ();
				}
			});

			_focusedEventViewContainer.View.Focused += (sender, args) => _focusedEventViewContainer.EventFired ();
			_unfocusedEventViewContainer.View.Unfocused += (sender, args) => _unfocusedEventViewContainer.EventFired ();

			_gestureRecognizerEventViewContainer.View.GestureRecognizers.Add (
				new TapGestureRecognizer {
					Command = new Command (() => _gestureRecognizerEventViewContainer.EventFired ())
				}
			);

			_views = new List<View> (viewContainers.Select (o => o.ContainerLayout));

			_moveNextStack = new StackLayout ();
			var moveNextButton = new Button ();
			moveNextButton.Text = "Move Next";
			moveNextButton.AutomationId = "MoveNextButton";
			moveNextButton.Clicked += delegate (object sender, EventArgs e) {
				if (!_views.Any ())
					return;

				if (_currentIndex + 1 >= _views.Count) {
					return;
				}

				_currentIndex += 1;

				_moveNextStack.Children.RemoveAt (2);
				_moveNextStack.Children.Add (_views[_currentIndex]);
				_picker.SelectedIndexChanged -= PickerSelectedIndexChanged;
				_picker.SelectedIndex = _currentIndex;
				_picker.SelectedIndexChanged += PickerSelectedIndexChanged;
			};
			
			_picker = new Picker();
			foreach (var container in viewContainers) {
				_picker.Items.Add(container.TitleLabel.Text);
			}
			
			_picker.SelectedIndex = _currentIndex;

			_picker.SelectedIndexChanged += PickerSelectedIndexChanged;

			_moveNextStack.Children.Add (_picker);
			_moveNextStack.Children.Add (moveNextButton);
			_moveNextStack.Children.Add (_views[_currentIndex]);

			stackLayout.Children.Add (_moveNextStack);

			if (!SupportsFocus) {
				stackLayout.Children.Remove (_focusStateViewContainer.ContainerLayout);
				stackLayout.Children.Remove (_isFocusedStateViewContainer.ContainerLayout);
			}

			if (!SupportsTapGestureRecognizer)
				stackLayout.Children.Remove (_gestureRecognizerEventViewContainer.ContainerLayout);

			foreach (var element in viewContainers)
				InitializeElement (element.View);
		}

		void PickerSelectedIndexChanged  (object sender, EventArgs eventArgs)
		{
			_currentIndex = _picker.SelectedIndex;
				_moveNextStack.Children.RemoveAt (2);
				_moveNextStack.Children.Add (_views[_currentIndex]);
		}


		protected virtual bool SupportsTapGestureRecognizer
		{
			get { return true; }
		}

		protected virtual bool SupportsFocus
		{
			get { return true; }
		}

		protected void Add (ViewContainer<T> view) {
			_views.Add (view.ContainerLayout);
			_picker.Items.Add(view.TitleLabel.Text);
		}
	}
}