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
		/// Bindable property for <see cref="Buttons"/>.
		/// </summary>
		public static readonly BindableProperty ButtonsProperty = BindableProperty.Create(nameof(Buttons), typeof(ButtonsMask), typeof(PointerGestureRecognizer), ButtonsMask.Primary);

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
		/// <remarks>
		/// <para>
		/// <b>Secondary / Right Button behavior (iOS &amp; Mac Catalyst):</b> When <see cref="Buttons"/> is set to include
		/// <see cref="ButtonsMask.Secondary"/> on iOS or Mac Catalyst, a secondary pointer press is simulated using an internal
		/// ("fake") context menu gesture – the same approach used by <see cref="TapGestureRecognizer"/>. Because UIKit does not
		/// expose a stable API for tracking a continuous right-button (secondary) down state, the framework will raise
		/// <see cref="PointerPressed"/> followed immediately by <see cref="PointerReleased"/> for a secondary click. There is no
		/// intermediate prolonged pressed state for secondary button interactions on these platforms.
		/// </para>
		/// <para>
		/// If you need to distinguish a primary press/release sequence from a secondary one, inspect <see cref="PointerEventArgs.Button"/>
		/// in the event handlers. Do not rely on timing (e.g., expecting a noticeable delay between pressed and released) for secondary
		/// interactions on iOS/Mac Catalyst as both events may fire in immediate succession.
		/// </para>
		/// </remarks>
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
		/// <remarks>
		/// See the remarks on <see cref="PointerReleased"/> for platform-specific behavior when handling secondary (right) button
		/// interactions. Command handlers may receive a release immediately after a press for secondary clicks on iOS/Mac Catalyst.
		/// </remarks>
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
		/// Gets or sets the mouse buttons that should trigger the pointer events. This is a bindable property.
		/// </summary>
		public ButtonsMask Buttons
		{
			get { return (ButtonsMask)GetValue(ButtonsProperty); }
			set { SetValue(ButtonsProperty, value); }
		}

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// </summary>
		internal void SendPointerEntered(View sender, Func<IElement?, Point?>? getPosition, PlatformPointerEventArgs? platformArgs = null, ButtonsMask button = ButtonsMask.Primary)
		{
			ICommand cmd = PointerEnteredCommand;
			if (cmd?.CanExecute(PointerEnteredCommandParameter) == true)
				cmd.Execute(PointerEnteredCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerEntered;
			handler?.Invoke(sender, new PointerEventArgs(getPosition, platformArgs, button));
		}

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// </summary>
		internal void SendPointerExited(View sender, Func<IElement?, Point?>? getPosition, PlatformPointerEventArgs? platformArgs = null, ButtonsMask button = ButtonsMask.Primary)
		{
			ICommand cmd = PointerExitedCommand;
			if (cmd?.CanExecute(PointerExitedCommandParameter) == true)
				cmd.Execute(PointerExitedCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerExited;
			handler?.Invoke(sender, new PointerEventArgs(getPosition, platformArgs, button));
		}

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// </summary>
		internal void SendPointerMoved(View sender, Func<IElement?, Point?>? getPosition, PlatformPointerEventArgs? platformArgs = null, ButtonsMask button = ButtonsMask.Primary)
		{
			ICommand cmd = PointerMovedCommand;
			if (cmd?.CanExecute(PointerMovedCommandParameter) == true)
				cmd.Execute(PointerMovedCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerMoved;
			handler?.Invoke(sender, new PointerEventArgs(getPosition, platformArgs, button));
		}

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// </summary>
		internal void SendPointerPressed(View sender, Func<IElement?, Point?>? getPosition, PlatformPointerEventArgs? platformArgs = null, ButtonsMask button = ButtonsMask.Primary)
		{
			ICommand cmd = PointerPressedCommand;
			if (cmd?.CanExecute(PointerPressedCommandParameter) == true)
				cmd.Execute(PointerPressedCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerPressed;
			handler?.Invoke(sender, new PointerEventArgs(getPosition, platformArgs, button));
		}

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// </summary>
		internal void SendPointerReleased(View sender, Func<IElement?, Point?>? getPosition, PlatformPointerEventArgs? platformArgs = null, ButtonsMask button = ButtonsMask.Primary)
		{
			ICommand cmd = PointerReleasedCommand;
			if (cmd?.CanExecute(PointerReleasedCommandParameter) == true)
				cmd.Execute(PointerReleasedCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerReleased;
			handler?.Invoke(sender, new PointerEventArgs(getPosition, platformArgs, button));
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
