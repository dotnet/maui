#nullable enable
using System;
using Microsoft.Maui.Graphics;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides pointer gesture recognition and events.
	/// </summary>
	public sealed class PointerGestureRecognizer : GestureRecognizer
	{
		public static readonly BindableProperty PointerEnteredCommandProperty = BindableProperty.Create(nameof(PointerEnteredCommand), typeof(ICommand), typeof(PointerGestureRecognizer), null);

		public static readonly BindableProperty PointerEnteredCommandParameterProperty = BindableProperty.Create(nameof(PointerEnteredCommandParameter), typeof(object), typeof(PointerGestureRecognizer), null);

		public static readonly BindableProperty PointerExitedCommandProperty = BindableProperty.Create(nameof(PointerExitedCommand), typeof(ICommand), typeof(PointerGestureRecognizer), null);

		public static readonly BindableProperty PointerExitedCommandParameterProperty = BindableProperty.Create(nameof(PointerExitedCommandParameter), typeof(object), typeof(PointerGestureRecognizer), null);

		public static readonly BindableProperty PointerMovedCommandProperty = BindableProperty.Create(nameof(PointerMovedCommand), typeof(ICommand), typeof(PointerGestureRecognizer), null);

		public static readonly BindableProperty PointerMovedCommandParameterProperty = BindableProperty.Create(nameof(PointerMovedCommandParameter), typeof(object), typeof(PointerGestureRecognizer), null);

		public PointerGestureRecognizer()
		{
		}

		public event EventHandler<PointerEventArgs>? PointerEntered;
		public event EventHandler<PointerEventArgs>? PointerExited;
		public event EventHandler<PointerEventArgs>? PointerMoved;

		public ICommand PointerEnteredCommand
		{
			get { return (ICommand)GetValue(PointerEnteredCommandProperty); }
			set { SetValue(PointerEnteredCommandProperty, value); }
		}

		public ICommand PointerEnteredCommandParameter
		{
			get { return (ICommand)GetValue(PointerEnteredCommandParameterProperty); }
			set { SetValue(PointerEnteredCommandParameterProperty, value); }
		}
		public ICommand PointerExitedCommand
		{
			get { return (ICommand)GetValue(PointerExitedCommandProperty); }
			set { SetValue(PointerExitedCommandProperty, value); }
		}
		public ICommand PointerExitedCommandParameter
		{
			get { return (ICommand)GetValue(PointerExitedCommandParameterProperty); }
			set { SetValue(PointerExitedCommandParameterProperty, value); }
		}

		public ICommand PointerMovedCommand
		{
			get { return (ICommand)GetValue(PointerMovedCommandProperty); }
			set { SetValue(PointerMovedCommandProperty, value); }
		}

		public ICommand PointerMovedCommandParameter
		{
			get { return (ICommand)GetValue(PointerMovedCommandParameterProperty); }
			set { SetValue(PointerMovedCommandParameterProperty, value); }
		}

		internal void SendPointerEntered(View sender, Func<IElement?, Point?>? getPosition)
		{
			ICommand cmd = PointerEnteredCommand;
			if (cmd?.CanExecute(PointerEnteredCommandParameter) == true)
				cmd.Execute(PointerEnteredCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerEntered;
			handler?.Invoke(sender, new PointerEventArgs(getPosition));
		}

		internal void SendPointerExited(View sender, Func<IElement?, Point?>? getPosition)
		{
			ICommand cmd = PointerExitedCommand;
			if (cmd?.CanExecute(PointerExitedCommandParameter) == true)
				cmd.Execute(PointerExitedCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerExited;
			handler?.Invoke(sender, new PointerEventArgs(getPosition));
		}

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
