#nullable disable
using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a swipe item that displays custom content in a <see cref="SwipeView"/>.
	/// </summary>
	[ContentProperty(nameof(Content))]
	public partial class SwipeItemView : ContentView, Controls.ISwipeItem, Maui.ISwipeItemView, ICommandElement
	{
		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SwipeItemView), null,
			propertyChanging: CommandElement.OnCommandChanging,
			propertyChanged: CommandElement.OnCommandChanged);

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SwipeItemView), null,
			propertyChanged: CommandElement.OnCommandParameterChanged);

		static SwipeItemView()
		{
			CommandProperty.DependsOn(CommandParameterProperty);
		}

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

		protected override bool IsEnabledCore =>
			base.IsEnabledCore && CommandElement.GetCanExecute(this, CommandProperty);

		void ICommandElement.CanExecuteChanged(object sender, EventArgs e) =>
			RefreshIsEnabledProperty();

		WeakCommandSubscription ICommandElement.CleanupTracker { get; set; }
	}
}
