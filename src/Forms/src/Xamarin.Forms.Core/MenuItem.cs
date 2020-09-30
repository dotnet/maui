using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms.StyleSheets;

namespace Xamarin.Forms
{
	public class MenuItem : BaseMenuItem, IMenuItemController, IStyleSelectable
	{
		public static readonly BindableProperty AcceleratorProperty = BindableProperty.CreateAttached(nameof(Accelerator), typeof(Accelerator), typeof(MenuItem), null);

		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(MenuItem), null,
			propertyChanging: (bo, o, n) => ((MenuItem)bo).OnCommandChanging(),
			propertyChanged: (bo, o, n) => ((MenuItem)bo).OnCommandChanged());

		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(MenuItem), null,
			propertyChanged: (bo, o, n) => ((MenuItem)bo).OnCommandParameterChanged());

		public static readonly BindableProperty IsDestructiveProperty = BindableProperty.Create(nameof(IsDestructive), typeof(bool), typeof(MenuItem), false);

		public static readonly BindableProperty IconImageSourceProperty = BindableProperty.Create(nameof(IconImageSource), typeof(ImageSource), typeof(MenuItem), default(ImageSource));

		[Obsolete("IconProperty is obsolete as of 4.0.0. Please use IconImageSourceProperty instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindableProperty IconProperty = IconImageSourceProperty;

		static readonly BindablePropertyKey IsEnabledPropertyKey = BindableProperty.CreateReadOnly(nameof(IsEnabled), typeof(bool), typeof(ToolbarItem), true);
		public static readonly BindableProperty IsEnabledProperty = IsEnabledPropertyKey.BindableProperty;

		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(MenuItem), null);

		public static Accelerator GetAccelerator(BindableObject bindable) => (Accelerator)bindable.GetValue(AcceleratorProperty);

		public static void SetAccelerator(BindableObject bindable, Accelerator value) => bindable.SetValue(AcceleratorProperty, value);

		internal readonly MergedStyle _mergedStyle;

		public MenuItem()
		{
			_mergedStyle = new MergedStyle(GetType(), this);
		}

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

		[Obsolete("Icon is obsolete as of 4.0.0. Please use IconImageSource instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public FileImageSource Icon
		{
			get => GetValue(IconProperty) as FileImageSource ?? default(FileImageSource);
			set => SetValue(IconProperty, value);
		}

		public ImageSource IconImageSource
		{
			get => (ImageSource)GetValue(IconImageSourceProperty);
			set => SetValue(IconImageSourceProperty, value);
		}

		public bool IsDestructive
		{
			get => (bool)GetValue(IsDestructiveProperty);
			set => SetValue(IsDestructiveProperty, value);
		}

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public bool IsEnabled
		{
			get => (bool)GetValue(IsEnabledProperty);
			[EditorBrowsable(EditorBrowsableState.Never)]
			set => SetValue(IsEnabledPropertyKey, value);
		}

		[TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> StyleClass
		{
			get { return @class; }
			set { @class = value; }
		}

		[TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> @class
		{
			get { return _mergedStyle.StyleClass; }
			set
			{
				_mergedStyle.StyleClass = value;
			}
		}

		IList<string> IStyleSelectable.Classes => StyleClass;

		bool IsEnabledCore
		{
			set => SetValueCore(IsEnabledPropertyKey, value);
		}

		[Obsolete("This property is obsolete as of 3.5.0. Please use MenuItem.IsEnabledProperty.PropertyName instead.")]
		public string IsEnabledPropertyName => MenuItem.IsEnabledProperty.PropertyName;

		public event EventHandler Clicked;

		protected virtual void OnClicked() => Clicked?.Invoke(this, EventArgs.Empty);

		[EditorBrowsable(EditorBrowsableState.Never)]
		void IMenuItemController.Activate()
		{
			if (IsEnabled)
				Command?.Execute(CommandParameter);

			OnClicked();
		}

		void OnCommandCanExecuteChanged(object sender, EventArgs eventArgs)
		{
			IsEnabledCore = Command.CanExecute(CommandParameter);
		}

		void OnCommandChanged()
		{
			IsEnabledCore = Command?.CanExecute(CommandParameter) ?? true;

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

			IsEnabledCore = Command.CanExecute(CommandParameter);
		}
	}
}