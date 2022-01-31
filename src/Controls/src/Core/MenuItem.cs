using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="Type[@FullName='Microsoft.Maui.Controls.MenuItem']/Docs" />
	public partial class MenuItem : BaseMenuItem, IMenuItemController, IStyleSelectable
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='AcceleratorProperty']/Docs" />
		public static readonly BindableProperty AcceleratorProperty = BindableProperty.CreateAttached(nameof(Accelerator), typeof(Accelerator), typeof(MenuItem), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='CommandProperty']/Docs" />
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(MenuItem), null,
			propertyChanging: (bo, o, n) => ((MenuItem)bo).OnCommandChanging(),
			propertyChanged: (bo, o, n) => ((MenuItem)bo).OnCommandChanged());

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='CommandParameterProperty']/Docs" />
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(MenuItem), null,
			propertyChanged: (bo, o, n) => ((MenuItem)bo).OnCommandParameterChanged());

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='IsDestructiveProperty']/Docs" />
		public static readonly BindableProperty IsDestructiveProperty = BindableProperty.Create(nameof(IsDestructive), typeof(bool), typeof(MenuItem), false);

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='IconImageSourceProperty']/Docs" />
		public static readonly BindableProperty IconImageSourceProperty = BindableProperty.Create(nameof(IconImageSource), typeof(ImageSource), typeof(MenuItem), default(ImageSource));

		static readonly BindablePropertyKey IsEnabledPropertyKey = BindableProperty.CreateReadOnly(nameof(IsEnabled), typeof(bool), typeof(ToolbarItem), true);
		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='IsEnabledProperty']/Docs" />
		public static readonly BindableProperty IsEnabledProperty = IsEnabledPropertyKey.BindableProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='TextProperty']/Docs" />
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(MenuItem), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='GetAccelerator']/Docs" />
		public static Accelerator GetAccelerator(BindableObject bindable) => (Accelerator)bindable.GetValue(AcceleratorProperty);

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='SetAccelerator']/Docs" />
		public static void SetAccelerator(BindableObject bindable, Accelerator value) => bindable.SetValue(AcceleratorProperty, value);

		internal readonly MergedStyle _mergedStyle;

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public MenuItem()
		{
			_mergedStyle = new MergedStyle(GetType(), this);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='Command']/Docs" />
		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='CommandParameter']/Docs" />
		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='IconImageSource']/Docs" />
		public ImageSource IconImageSource
		{
			get => (ImageSource)GetValue(IconImageSourceProperty);
			set => SetValue(IconImageSourceProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='IsDestructive']/Docs" />
		public bool IsDestructive
		{
			get => (bool)GetValue(IsDestructiveProperty);
			set => SetValue(IsDestructiveProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='Text']/Docs" />
		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='IsEnabled']/Docs" />
		public bool IsEnabled
		{
			get => (bool)GetValue(IsEnabledProperty);
			[EditorBrowsable(EditorBrowsableState.Never)]
			set => SetValue(IsEnabledPropertyKey, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='StyleClass']/Docs" />
		[System.ComponentModel.TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> StyleClass
		{
			get { return @class; }
			set { @class = value; }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='class']/Docs" />
		[System.ComponentModel.TypeConverter(typeof(ListStringTypeConverter))]
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