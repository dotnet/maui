#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.StyleSheets;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="Type[@FullName='Microsoft.Maui.Controls.MenuItem']/Docs/*" />
	public partial class MenuItem : BaseMenuItem, IMenuItemController, IStyleSelectable, ICommandElement, IMenuElement
	{
		/// <summary>Bindable property for <see cref="Accelerator"/>.</summary>
		public static readonly BindableProperty AcceleratorProperty = BindableProperty.CreateAttached(nameof(Accelerator), typeof(Accelerator), typeof(MenuItem), null);

		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(
			nameof(Command), typeof(ICommand), typeof(MenuItem), null,
			propertyChanging: CommandElement.OnCommandChanging,
			propertyChanged: CommandElement.OnCommandChanged);

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
			nameof(CommandParameter), typeof(object), typeof(MenuItem), null,
			propertyChanged: CommandElement.OnCommandParameterChanged);

		/// <summary>Bindable property for <see cref="IsDestructive"/>.</summary>
		public static readonly BindableProperty IsDestructiveProperty = BindableProperty.Create(nameof(IsDestructive), typeof(bool), typeof(MenuItem), false);

		/// <summary>Bindable property for <see cref="IconImageSource"/>.</summary>
		public static readonly BindableProperty IconImageSourceProperty = BindableProperty.Create(nameof(IconImageSource), typeof(ImageSource), typeof(MenuItem), default(ImageSource));

		/// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
		public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
			nameof(IsEnabled), typeof(bool), typeof(MenuItem), true,
			coerceValue: CoerceIsEnabledProperty);

		/// <summary>Bindable property for <see cref="Text"/>.</summary>
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(MenuItem), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='GetAccelerator']/Docs/*" />
		public static Accelerator GetAccelerator(BindableObject bindable) => (Accelerator)bindable.GetValue(AcceleratorProperty);

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='SetAccelerator']/Docs/*" />
		public static void SetAccelerator(BindableObject bindable, Accelerator value) => bindable.SetValue(AcceleratorProperty, value);

		internal readonly MergedStyle _mergedStyle;
		bool _isEnabledExplicit = (bool)IsEnabledProperty.DefaultValue;

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public MenuItem()
		{
			_mergedStyle = new MergedStyle(GetType(), this);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='Command']/Docs/*" />
		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='CommandParameter']/Docs/*" />
		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='IconImageSource']/Docs/*" />
		public ImageSource IconImageSource
		{
			get => (ImageSource)GetValue(IconImageSourceProperty);
			set => SetValue(IconImageSourceProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='IsDestructive']/Docs/*" />
		public bool IsDestructive
		{
			get => (bool)GetValue(IsDestructiveProperty);
			set => SetValue(IsDestructiveProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='Text']/Docs/*" />
		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='IsEnabled']/Docs/*" />
		public bool IsEnabled
		{
			get => (bool)GetValue(IsEnabledProperty);
			set => SetValue(IsEnabledProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='StyleClass']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> StyleClass
		{
			get { return @class; }
			set { @class = value; }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='class']/Docs/*" />
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

		public event EventHandler Clicked;

		protected virtual void OnClicked() => Clicked?.Invoke(this, EventArgs.Empty);

		void IMenuItemController.Activate()
		{
			if (IsEnabled)
				Command?.Execute(CommandParameter);

			OnClicked();
		}

		void ICommandElement.CanExecuteChanged(object sender, EventArgs e) =>
			this.RefreshPropertyValue(IsEnabledProperty, _isEnabledExplicit);

		static object CoerceIsEnabledProperty(BindableObject bindable, object value)
		{
			if (bindable is MenuItem menuitem)
			{
				menuitem._isEnabledExplicit = (bool)value;

				return menuitem._isEnabledExplicit && CommandElement.GetCanExecute(menuitem);
			}

			return false;
		}

		IReadOnlyList<IAccelerator> IMenuElement.Accelerators =>
			GetAccelerator(this) is Accelerator acc ? new[] { acc } : null;

		IImageSource IImageSourcePart.Source => this.IconImageSource;

		bool IImageSourcePart.IsAnimationPlaying => false;

		Color ITextStyle.TextColor => null;

		Font ITextStyle.Font => Font.Default;

		double ITextStyle.CharacterSpacing => 0;

		void IMenuElement.Clicked()
		{
			((IMenuItemController)this).Activate();
		}

		void IImageSourcePart.UpdateIsLoading(bool isLoading)
		{
		}
	}
}
