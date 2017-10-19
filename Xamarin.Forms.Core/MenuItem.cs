using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Xamarin.Forms
{

	public class MenuItem : BaseMenuItem, IMenuItemController
	{
		public static readonly BindableProperty AcceleratorProperty = BindableProperty.CreateAttached(nameof(Accelerator), typeof(Accelerator), typeof(MenuItem), null);

		public static Accelerator GetAccelerator(BindableObject bindable) => (Accelerator)bindable.GetValue(AcceleratorProperty);

		public static void SetAccelerator(BindableObject bindable, Accelerator value) => bindable.SetValue(AcceleratorProperty, value);

		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(MenuItem), null);

		public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(MenuItem), null,
			propertyChanging: (bo, o, n) => ((MenuItem)bo).OnCommandChanging(), propertyChanged: (bo, o, n) => ((MenuItem)bo).OnCommandChanged());

		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(MenuItem), null,
			propertyChanged: (bo, o, n) => ((MenuItem)bo).OnCommandParameterChanged());

		public static readonly BindableProperty IsDestructiveProperty = BindableProperty.Create("IsDestructive", typeof(bool), typeof(MenuItem), false);

		public static readonly BindableProperty IconProperty = BindableProperty.Create("Icon", typeof(FileImageSource), typeof(MenuItem), default(FileImageSource));

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create("IsEnabled", typeof(bool), typeof(ToolbarItem), true);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public string IsEnabledPropertyName
		{
			get
			{
				return IsEnabledProperty.PropertyName;
			}
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

		public FileImageSource Icon
		{
			get { return (FileImageSource)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public bool IsDestructive
		{
			get { return (bool)GetValue(IsDestructiveProperty); }
			set { SetValue(IsDestructiveProperty, value); }
		}

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		bool IsEnabledCore
		{
			set { SetValueCore(IsEnabledProperty, value); }
		}

		public event EventHandler Clicked;

		protected virtual void OnClicked()
		{
			EventHandler handler = Clicked;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Activate()
		{
			if (Command != null)
			{
				if (IsEnabled)
					Command.Execute(CommandParameter);
			}

			OnClicked();
		}

		void OnCommandCanExecuteChanged(object sender, EventArgs eventArgs)
		{
			IsEnabledCore = Command.CanExecute(CommandParameter);
		}

		void OnCommandChanged()
		{
			if (Command == null)
			{
				IsEnabledCore = true;
				return;
			}

			IsEnabledCore = Command.CanExecute(CommandParameter);

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

			IsEnabledCore = Command.CanExecute(CommandParameter);
		}
	}
}