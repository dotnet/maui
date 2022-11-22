#nullable enable
using System;
using System.Windows.Input;
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
		public ICommand PointerEnteredCommandParameter
		{
			get { return (ICommand)GetValue(PointerEnteredCommandParameterProperty); }
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
		public ICommand PointerExitedCommandParameter
		{
			get { return (ICommand)GetValue(PointerExitedCommandParameterProperty); }
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
		public ICommand PointerMovedCommandParameter
		{
			get { return (ICommand)GetValue(PointerMovedCommandParameterProperty); }
			set { SetValue(PointerMovedCommandParameterProperty, value); }
		}

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// </summary>
		internal void SendPointerEntered(View sender, Func<IElement?, Point?>? getPosition)
		{
			ICommand cmd = PointerEnteredCommand;
			if (cmd?.CanExecute(PointerEnteredCommandParameter) == true)
				cmd.Execute(PointerEnteredCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerEntered;
			handler?.Invoke(sender, new PointerEventArgs(getPosition));
		}

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// </summary>
		internal void SendPointerExited(View sender, Func<IElement?, Point?>? getPosition)
		{
			ICommand cmd = PointerExitedCommand;
			if (cmd?.CanExecute(PointerExitedCommandParameter) == true)
				cmd.Execute(PointerExitedCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerExited;
			handler?.Invoke(sender, new PointerEventArgs(getPosition));
		}

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// </summary>
		internal void SendPointerMoved(View sender, Func<IElement?, Point?>? getPosition)
		{
			ICommand cmd = PointerMovedCommand;
			if (cmd?.CanExecute(PointerMovedCommandParameter) == true)
				cmd.Execute(PointerMovedCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerMoved;
			handler?.Invoke(sender, new PointerEventArgs(getPosition));
		}
	}
}
