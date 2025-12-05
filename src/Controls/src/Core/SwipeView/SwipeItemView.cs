#nullable disable
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItemView.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwipeItemView']/Docs/*" />
	[ContentProperty(nameof(Content))]
#if ANDROID || IOS || MACCATALYST || TIZEN
	[ElementHandler(typeof(SwipeItemViewHandler))]
#endif
	public partial class SwipeItemView : ContentView, Controls.ISwipeItem, Maui.ISwipeItemView
	{
		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SwipeItemView), null,
			propertyChanging: (bo, o, n) => ((SwipeItemView)bo).OnCommandChanging(),
			propertyChanged: (bo, o, n) => ((SwipeItemView)bo).OnCommandChanged());

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SwipeItemView), null,
			propertyChanged: (bo, o, n) => ((SwipeItemView)bo).OnCommandParameterChanged());

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItemView.xml" path="//Member[@MemberName='Command']/Docs/*" />
		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItemView.xml" path="//Member[@MemberName='CommandParameter']/Docs/*" />
		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		public event EventHandler<EventArgs> Invoked;

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItemView.xml" path="//Member[@MemberName='OnInvoked']/Docs/*" />
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