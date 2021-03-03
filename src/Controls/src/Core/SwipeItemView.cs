using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	public class SwipeItemView : ContentView, ISwipeItem
	{
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SwipeItemView), null,
			propertyChanging: (bo, o, n) => ((SwipeItemView)bo).OnCommandChanging(),
			propertyChanged: (bo, o, n) => ((SwipeItemView)bo).OnCommandChanged());

		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SwipeItemView), null,
			propertyChanged: (bo, o, n) => ((SwipeItemView)bo).OnCommandParameterChanged());

		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		public event EventHandler<EventArgs> Invoked;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void OnInvoked()
		{
			if (Command != null && Command.CanExecute(CommandParameter))
				Command.Execute(CommandParameter);

			Invoked?.Invoke(this, EventArgs.Empty);
		}

		void OnCommandChanged()
		{
			IsEnabled = Command?.CanExecute(CommandParameter) ?? true;

			if (Command == null)
				return;

			Command.CanExecuteChanged += OnCommandCanExecuteChanged;
		}

		void OnCommandChanging()
		{
			if (Command == null)
				return;

			Command.CanExecuteChanged -= OnCommandCanExecuteChanged;
		}

		void OnCommandParameterChanged()
		{
			if (Command == null)
				return;

			IsEnabled = Command.CanExecute(CommandParameter);
		}

		void OnCommandCanExecuteChanged(object sender, EventArgs eventArgs)
		{
			IsEnabled = Command.CanExecute(CommandParameter);
		}
	}
}