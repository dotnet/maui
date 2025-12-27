#nullable disable
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.StyleSheets;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>Class that presents a menu item and associates it with a command.</summary>
	public partial class MenuItem : BaseMenuItem, IMenuItemController, ICommandElement, IMenuElement, IPropertyPropagationController
	{
		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty;

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty;

		static MenuItem()
		{
			CommandParameterProperty = BindableProperty.Create(
				nameof(CommandParameter), typeof(object), typeof(MenuItem), null,
				propertyChanged: CommandElement.OnCommandParameterChanged);

			CommandProperty = BindableProperty.Create(
				nameof(Command), typeof(ICommand), typeof(MenuItem), null,
				propertyChanging: CommandElement.OnCommandChanging,
				propertyChanged: CommandElement.OnCommandChanged);

			// Register dependency: Command depends on CommandParameter for CanExecute evaluation
			// See https://github.com/dotnet/maui/issues/31939
			CommandProperty.DependsOn(CommandParameterProperty);
		}

		/// <summary>Bindable property for <see cref="IsDestructive"/>.</summary>
		public static readonly BindableProperty IsDestructiveProperty = BindableProperty.Create(nameof(IsDestructive), typeof(bool), typeof(MenuItem), false);

		/// <summary>Bindable property for <see cref="IconImageSource"/>.</summary>
		public static readonly BindableProperty IconImageSourceProperty = BindableProperty.Create(nameof(IconImageSource), typeof(ImageSource), typeof(MenuItem), default(ImageSource),
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				((MenuItem)bindable).AddRemoveLogicalChildren(oldValue, newValue);
				OnImageSourceChanged(bindable, oldValue, newValue);
			}
		);

		/// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
		public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
			nameof(IsEnabled), typeof(bool), typeof(MenuItem), true,
			propertyChanged: OnIsEnabledPropertyChanged, coerceValue: CoerceIsEnabledProperty);

		/// <summary>Bindable property for <see cref="Text"/>.</summary>
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(MenuItem), null);

		bool _isEnabledExplicit = (bool)IsEnabledProperty.DefaultValue;

		/// <summary>Intitializes a new <see cref="Microsoft.Maui.Controls.MenuItem"/> instance.</summary>
		public MenuItem()
		{
		}

		/// <summary>Gets or sets the command that is run when the menu is clicked. This is a bindable property.</summary>
		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		/// <summary>Gets or sets the parameter that is passed to the command. This is a bindable property.</summary>
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

		/// <summary>Gets or sets a value that indicates whether or not the menu item removes its associated UI element.</summary>
		/// <remarks>The following example shows how to add a MenuItem with IsDestructive set to True.</remarks>
		public bool IsDestructive
		{
			get => (bool)GetValue(IsDestructiveProperty);
			set => SetValue(IsDestructiveProperty, value);
		}

		/// <summary>The text of the menu item. This is a bindable property.</summary>
		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		/// <summary>For internal use by the Microsoft.Maui.Controls platform. This is a bindable property.</summary>
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

			var canExecute = CommandElement.GetCanExecute(menuItem, CommandProperty);
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

		static void OnImageSourceChanged(BindableObject bindable, object oldvalue, object newValue)
		{
			if (oldvalue is ImageSource oldImageSource)
				oldImageSource.SourceChanged -= ((MenuItem)bindable).OnImageSourceSourceChanged;

			if (newValue is ImageSource newImageSource)
				newImageSource.SourceChanged += ((MenuItem)bindable).OnImageSourceSourceChanged;
		}

		void OnImageSourceSourceChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(IconImageSourceProperty.PropertyName);
		}

		WeakCommandSubscription ICommandElement.CleanupTracker
		{
			get;
			set;
		}
	}
}
