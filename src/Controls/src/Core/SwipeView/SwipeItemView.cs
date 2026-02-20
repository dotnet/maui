#nullable disable
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a swipe item that displays custom content in a <see cref="SwipeView"/>.
	/// </summary>
	[ContentProperty(nameof(Content))]
	public partial class SwipeItemView : ContentView, Controls.ISwipeItem, Maui.ISwipeItemView
	{
		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SwipeItemView), null,
			propertyChanging: (bo, o, n) => ((SwipeItemView)bo).OnCommandChanging(),
			propertyChanged: (bo, o, n) => ((SwipeItemView)bo).OnCommandChanged());

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SwipeItemView), null,
			propertyChanged: (bo, o, n) => ((SwipeItemView)bo).OnCommandParameterChanged());

		/// <summary>
		/// Gets or sets the command invoked when this swipe item is activated. This is a bindable property.
		/// </summary>
		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		/// <summary>
		/// Gets or sets the parameter passed to the <see cref="Command"/>. This is a bindable property.
		/// </summary>
		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		public event EventHandler<EventArgs> Invoked;

		/// <summary>
		/// Invokes the swipe item, executing the command and raising the <see cref="Invoked"/> event.
		/// </summary>
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

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			base.OnHandlerChangingCore(args);

			if (Application.Current is null)
				return;

			if (args.NewHandler is null || args.OldHandler is not null)
				Application.Current.RequestedThemeChanged -= OnRequestedThemeChanged;
			if (args.NewHandler is not null && args.OldHandler is null)
				Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
		}

		private void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
		{
			// Force refresh/re-evaluate AppTheme binding
			this.RefreshPropertyValue(BackgroundColorProperty, BackgroundColor);
		}
	}
}