using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Xamarin.Forms
{
	public sealed class TapGestureRecognizer : GestureRecognizer
	{
		public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(TapGestureRecognizer), null);

		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(TapGestureRecognizer), null);

		public static readonly BindableProperty NumberOfTapsRequiredProperty = BindableProperty.Create("NumberOfTapsRequired", typeof(int), typeof(TapGestureRecognizer), 1);

		public TapGestureRecognizer()
		{
		}

		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		public int NumberOfTapsRequired
		{
			get { return (int)GetValue(NumberOfTapsRequiredProperty); }
			set { SetValue(NumberOfTapsRequiredProperty, value); }
		}

		public event EventHandler Tapped;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendTapped(View sender)
		{
			ICommand cmd = Command;
			if (cmd != null && cmd.CanExecute(CommandParameter))
				cmd.Execute(CommandParameter);

			EventHandler handler = Tapped;
			if (handler != null)
				handler(sender, new TappedEventArgs(CommandParameter));

#pragma warning disable 0618 // retain until TappedCallback removed
			Action<View, object> callback = TappedCallback;
			if (callback != null)
				callback(sender, TappedCallbackParameter);
#pragma warning restore
		}

		#region obsolete cruft

		// call empty constructor to hack around bug in mono where compiler generates invalid IL
		[Obsolete("TapGestureRecognizer(Action<View, object> tappedCallback) is obsolete as of version 1.0.2. Please use Command instead.")]
		public TapGestureRecognizer(Action<View, object> tappedCallback) : this()
		{
			if (tappedCallback == null)
				throw new ArgumentNullException("tappedCallback");
			TappedCallback = tappedCallback;
		}

		// call empty constructor to hack around bug in mono where compiler generates invalid IL
		[Obsolete("TapGestureRecognizer(Action<View> tappedCallback) is obsolete as of version 1.0.2. Please use Command instead.")]
		public TapGestureRecognizer(Action<View> tappedCallback) : this()
		{
			if (tappedCallback == null)
				throw new ArgumentNullException("tappedCallback");
			TappedCallback = (s, o) => tappedCallback(s);
		}

		[Obsolete("TappedCallbackProperty is obsolete as of version 1.0.2. Please use Command instead.")] 
		public static readonly BindableProperty TappedCallbackProperty = BindableProperty.Create("TappedCallback", typeof(Action<View, object>), typeof(TapGestureRecognizer), null);

		[Obsolete("TappedCallback is obsolete as of version 1.0.2. Please use Command instead.")]
		public Action<View, object> TappedCallback
		{
			get { return (Action<View, object>)GetValue(TappedCallbackProperty); }
			set { SetValue(TappedCallbackProperty, value); }
		}

		[Obsolete("TappedCallbackParameterProperty is obsolete as of version 1.0.2. Please use Command instead.")] 
		public static readonly BindableProperty TappedCallbackParameterProperty = BindableProperty.Create("TappedCallbackParameter", typeof(object), typeof(TapGestureRecognizer), null);

		[Obsolete("TappedCallbackParameter is obsolete as of version 1.0.2. Please use Command instead.")]
		public object TappedCallbackParameter
		{
			get { return GetValue(TappedCallbackParameterProperty); }
			set { SetValue(TappedCallbackParameterProperty, value); }
		}

		#endregion
	}
}