#nullable enable
using System;
using System.ComponentModel;
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

		PointerEventArgs args = new PointerEventArgs();

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
		
		public Point? GetPosition(Element? relativeTo)
		{
			throw new NotImplementedException();
		}
		
		internal void SendPointerEntered(View sender)
		{
			ICommand cmd = PointerEnteredCommand;
			if (cmd != null && cmd.CanExecute(PointerEnteredCommandParameter))
				cmd.Execute(PointerEnteredCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerEntered;
			if (handler != null)
				handler?.Invoke(sender, args);
		}

		internal void SendPointerExited(View sender)
		{
			ICommand cmd = PointerExitedCommand;
			if (cmd != null && cmd.CanExecute(PointerExitedCommandParameter))
				cmd.Execute(PointerExitedCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerExited;
			if (handler != null)
				handler?.Invoke(sender, args);
		}

		internal void SendPointerMoved(View sender)
		{
			ICommand cmd = PointerMovedCommand;
			if (cmd != null && cmd.CanExecute(PointerMovedCommandParameter))
				cmd.Execute(PointerMovedCommandParameter);

			EventHandler<PointerEventArgs>? handler = PointerMoved;
			if (handler != null)
				handler?.Invoke(sender, args);
		}
	}
}
