#nullable disable
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.StyleSheets;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="Type[@FullName='Microsoft.Maui.Controls.MenuItem']/Docs/*" />
	public partial class MenuItem : BaseMenuItem, IMenuItemController, ICommandElement, IMenuElement, IPropertyPropagationController
	{
		/// <summary>Bindable property for <see cref="Accelerator"/>.</summary>
		[Obsolete("Use MenuFlyoutItem.KeyboardAcceleratorsProperty instead.")]
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
		public static readonly BindableProperty IconImageSourceProperty = BindableProperty.Create(nameof(IconImageSource), typeof(ImageSource), typeof(MenuItem), default(ImageSource),
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				((MenuItem)bindable).AddRemoveLogicalChildren(oldValue, newValue);
			}
		);

		/// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
		public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
			nameof(IsEnabled), typeof(bool), typeof(MenuItem), true,
			propertyChanged: OnIsEnabledPropertyChanged, coerceValue: CoerceIsEnabledProperty);

		/// <summary>Bindable property for <see cref="Text"/>.</summary>
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(MenuItem), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='GetAccelerator']/Docs/*" />
		[Obsolete("Use MenuFlyoutItem.KeyboardAcceleratorsProperty instead.")]
		public static Accelerator GetAccelerator(BindableObject bindable) => (Accelerator)bindable.GetValue(AcceleratorProperty);

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='SetAccelerator']/Docs/*" />
		[Obsolete("Use MenuFlyoutItem.KeyboardAcceleratorsProperty instead.")]
		public static void SetAccelerator(BindableObject bindable, Accelerator value) => bindable.SetValue(AcceleratorProperty, value);
		bool _isEnabledExplicit = (bool)IsEnabledProperty.DefaultValue;

		/// <include file="../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public MenuItem()
		{
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
			if (bindable is not MenuItem menuItem)
			{
				return false;
			}

			menuItem._isEnabledExplicit = (bool)value;

			if (!menuItem._isEnabledExplicit)
			{
				// No need to check GetCanExecute or the Parent's state
				return false;
			}

			var canExecute = CommandElement.GetCanExecute(menuItem);
			if (!canExecute)
			{
				return false;
			}

			// IsEnabled is not explicitly set to false, and the command can be
			// executed. The only thing left to verify is Parent.IsEnabled
			if (menuItem.Parent is MenuItem parentMenuItem && !parentMenuItem.IsEnabled)
			{
				return false;
			}

			return true;
		}

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

		void IPropertyPropagationController.PropagatePropertyChanged(string propertyName)
		{
			if (propertyName == null || propertyName == IsEnabledProperty.PropertyName)
				this.RefreshPropertyValue(IsEnabledProperty, _isEnabledExplicit);

			PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, ((IVisualTreeElement)this).GetVisualChildren());
		}

		static void OnIsEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is not MenuItem || bindable is not IPropertyPropagationController ppc)
			{
				return;
			}

			ppc.PropagatePropertyChanged(IsEnabledProperty.PropertyName);
		}
	}
}
