using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides pointer gesture recognition and events.
	/// </summary>
	public sealed class PointerGestureRecognizer : GestureRecognizer
	{
		/// <summary>
		/// The command to invoke when the pointer has entered the view. This is a bindable property.
		/// </summary>
		public static readonly BindableProperty PointerEnteredCommandProperty = BindableProperty.Create(nameof(PointerEnteredCommand), typeof(ICommand), typeof(PointerGestureRecognizer), null);

		/// <summary>
		/// An object to be passed to PointerEnteredCommand. This is a bindable property.
		/// </summary>
		public static readonly BindableProperty PointerEnteredCommandParameterProperty = BindableProperty.Create(nameof(PointerEnteredCommandParameter), typeof(object), typeof(PointerGestureRecognizer), null);

		/// <summary>
		/// The command to invoke when the pointer has exited the view. This is a bindable property.
		/// </summary>
		public static readonly BindableProperty PointerExitedCommandProperty = BindableProperty.Create(nameof(PointerExitedCommand), typeof(ICommand), typeof(PointerGestureRecognizer), null);

		/// <summary>
		/// An object to be passed to PointerExitedCommand. This is a bindable property.
		/// </summary>
		public static readonly BindableProperty PointerExitedCommandParameterProperty = BindableProperty.Create(nameof(PointerExitedCommandParameter), typeof(object), typeof(PointerGestureRecognizer), null);

		/// <summary>
		/// The command to invoke when the pointer has moved within the view. This is a bindable property.
		/// </summary>
		public static readonly BindableProperty PointerMovedCommandProperty = BindableProperty.Create(nameof(PointerMovedCommand), typeof(ICommand), typeof(PointerGestureRecognizer), null);

		/// <summary>
		/// An object to be passed to the PointerMovedCommand. This is a bindable property.
		/// </summary>
		public static readonly BindableProperty PointerMovedCommandParameterProperty = BindableProperty.Create(nameof(PointerMovedCommandParameter), typeof(object), typeof(PointerGestureRecognizer), null);

		/// <summary>
		/// The command to invoke when the pointer initiates a press within the view. This is a bindable property.
		/// </summary>
		public static readonly BindableProperty PointerPressedCommandProperty = BindableProperty.Create(nameof(PointerPressedCommand), typeof(ICommand), typeof(PointerGestureRecognizer), null);

		/// <summary>
		/// An object to be passed to the PointerPressedCommand. This is a bindable property.
		/// </summary>
		public static readonly BindableProperty PointerPressedCommandParameterProperty = BindableProperty.Create(nameof(PointerPressedCommandParameter), typeof(object), typeof(PointerGestureRecognizer), null);

		/// <summary>
		/// A command to invoke when the pointer that has previous initiated a press is released within the view. This is a bindable property.
		/// </summary>
		public static readonly BindableProperty PointerReleasedCommandProperty = BindableProperty.Create(nameof(PointerReleasedCommand), typeof(ICommand), typeof(PointerGestureRecognizer), null);

		/// <summary>
		/// An object to be passed to the PointerReleasedCommand. This is a bindable property.
		/// </summary>
		public static readonly BindableProperty PointerReleasedCommandParameterProperty = BindableProperty.Create(nameof(PointerReleasedCommandParameter), typeof(object), typeof(PointerGestureRecognizer), null);


		/// <summary>
		/// Initializes a new instance of PointerGestureRecognizer.
		/// </summary>
		public PointerGestureRecognizer()
		{
		}

		/// <summary>
		/// Raised when the pointer enters the view.
		/// </summary>
		public event EventHandler<PointerEventArgs>? PointerEntered;

		/// <summary>
		/// Raised when the pointer exits the view.
		/// </summary>
		public event EventHandler<PointerEventArgs>? PointerExited;

		/// <summary>
		/// Raised when the pointer moves within the view.
		/// </summary>
		public event EventHandler<PointerEventArgs>? PointerMoved;

		/// <summary>
		/// Raised when the pointer initiates a press within the view.
		/// </summary>
		public event EventHandler<PointerEventArgs>? PointerPressed;

		/// <summary>
		/// Raised when the pointer that has previous initiated a press is released within the view.
		/// </summary>
		public event EventHandler<PointerEventArgs>? PointerReleased;

		/// <summary>
		/// Identifies the PointerEnteredCommand bindable property.
		/// </summary>
		public ICommand PointerEnteredCommand
		{
			get { return (ICommand)GetValue(PointerEnteredCommandProperty); }
			set { SetValue(PointerEnteredCommandProperty, value); }
		}

		/// <summary>
		/// Identifies the PointerEnteredCommandParameter bindable property.
		/// </summary>
		public object PointerEnteredCommandParameter
		{
			get { return GetValue(PointerEnteredCommandParameterProperty); }
			set { SetValue(PointerEnteredCommandParameterProperty, value); }
		}

		/// <summary>
		/// Identifies the PointerExitedCommand bindable property.
		/// </summary>
		public ICommand PointerExitedCommand
		{
			get { return (ICommand)GetValue(PointerExitedCommandProperty); }
			set { SetValue(PointerExitedCommandProperty, value); }
		}

		/// <summary>
		/// Identifies the PointerExitedCommandParameter bindable property.
		/// </summary>
		public object PointerExitedCommandParameter
		{
			get { return GetValue(PointerExitedCommandParameterProperty); }
			set { SetValue(PointerExitedCommandParameterProperty, value); }
		}

		/// <summary>
		/// Identifies the PointerMovedCommand bindable property.
		/// </summary>
		public ICommand PointerMovedCommand
		{
			get { return (ICommand)GetValue(PointerMovedCommandProperty); }
			set { SetValue(PointerMovedCommandProperty, value); }
		}

		/// <summary>
		/// Identifies the PointerMovedCommandParameter bindable property.
		/// </summary>
		public object PointerMovedCommandParameter
		{
			get { return GetValue(PointerMovedCommandParameterProperty); }
			set { SetValue(PointerMovedCommandParameterProperty, value); }
		}

		/// <summary>
		/// Identifies the PointerPressedCommand bindable property.
		/// </summary>
		public ICommand PointerPressedCommand
		{
			get { return (ICommand)GetValue(PointerPressedCommandProperty); }
			set { SetValue(PointerPressedCommandProperty, value); }
		}

		/// <summary>
		/// Identifies the PointerPressedCommandParameter bindable property.
		/// </summary>
		public object PointerPressedCommandParameter
		{
			get { return GetValue(PointerPressedCommandParameterProperty); }
			set { SetValue(PointerPressedCommandParameterProperty, value); }
		}

		/// <summary>
		/// Identifies the PointerReleasedCommand bindable property.
		/// </summary>
		public ICommand PointerReleasedCommand
		{
			get { return (ICommand)GetValue(PointerReleasedCommandProperty); }
			set { SetValue(PointerReleasedCommandProperty, value); }
		}

		/// <summary>
		/// Identifies the PointerReleasedCommandParameter bindable property.
		/// </summary>
		public object PointerReleasedCommandParameter
		{
			get { return GetValue(PointerReleasedCommandParameterProperty); }
			set { SetValue(PointerReleasedCommandParameterProperty, value); }
		}

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// </summary>
		internal void SendPointerEntered(View sender, Func<IElement?, Point?>? getPosition, PlatformPointerEventArgs? platformArgs = null)
		{
			ICommand cmd = PointerEnteredCommand;
			if (cmd?.CanExecute(PointerEnteredCommandParameter) == true)
				cmd.Execute(PointerEnteredCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerEntered;
			handler?.Invoke(sender, new PointerEventArgs(getPosition, platformArgs));
		}

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// </summary>
		internal void SendPointerExited(View sender, Func<IElement?, Point?>? getPosition, PlatformPointerEventArgs? platformArgs = null)
		{
			ICommand cmd = PointerExitedCommand;
			if (cmd?.CanExecute(PointerExitedCommandParameter) == true)
				cmd.Execute(PointerExitedCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerExited;
			handler?.Invoke(sender, new PointerEventArgs(getPosition, platformArgs));
		}

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// </summary>
		internal void SendPointerMoved(View sender, Func<IElement?, Point?>? getPosition, PlatformPointerEventArgs? platformArgs = null)
		{
			ICommand cmd = PointerMovedCommand;
			if (cmd?.CanExecute(PointerMovedCommandParameter) == true)
				cmd.Execute(PointerMovedCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerMoved;
			handler?.Invoke(sender, new PointerEventArgs(getPosition, platformArgs));
		}

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// </summary>
		internal void SendPointerPressed(View sender, Func<IElement?, Point?>? getPosition, PlatformPointerEventArgs? platformArgs = null)
		{
			ICommand cmd = PointerPressedCommand;
			if (cmd?.CanExecute(PointerPressedCommandParameter) == true)
				cmd.Execute(PointerPressedCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerPressed;
			handler?.Invoke(sender, new PointerEventArgs(getPosition, platformArgs));
		}

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// </summary>
		internal void SendPointerReleased(View sender, Func<IElement?, Point?>? getPosition, PlatformPointerEventArgs? platformArgs = null)
		{
			ICommand cmd = PointerReleasedCommand;
			if (cmd?.CanExecute(PointerReleasedCommandParameter) == true)
				cmd.Execute(PointerReleasedCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerReleased;
			handler?.Invoke(sender, new PointerEventArgs(getPosition, platformArgs));
		}

		internal static void SetupForPointerOverVSM(
			VisualElement element,
			Action<bool> updatePointerState,
			ref PointerGestureRecognizer? recognizer)
		{
			bool hasPointerOverVSM =
				element.HasVisualState(VisualStateManager.CommonStates.PointerOver);

			if (hasPointerOverVSM && recognizer == null)
			{
				recognizer = new PointerGestureRecognizer();

				recognizer.PointerEntered += (s, e) =>
				{
					updatePointerState.Invoke(true);
				};

				recognizer.PointerExited += (s, e) =>
				{
					updatePointerState.Invoke(false);
				};

				((IGestureController)element).CompositeGestureRecognizers.Add(recognizer);
			}
			else if (!hasPointerOverVSM && recognizer != null)
			{
				((IGestureController)element).CompositeGestureRecognizers.Remove(recognizer);
				recognizer = null;
			}
		}
	}
}
