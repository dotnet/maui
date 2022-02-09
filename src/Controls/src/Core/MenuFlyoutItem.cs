using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
	public partial class MenuFlyoutItem : MenuFlyoutItemBase, IMenuFlyoutItem
	{
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(MenuFlyoutItem), null,
			   propertyChanging: (bo, o, n) => ((MenuFlyoutItem)bo).OnCommandChanging(),
			   propertyChanged: (bo, o, n) => ((MenuFlyoutItem)bo).OnCommandChanged());

		public static readonly BindableProperty CommandParameterProperty = 
			BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(MenuFlyoutItem), null,
			propertyChanged: (bo, o, n) => ((MenuFlyoutItem)bo).OnCommandParameterChanged());

		public static readonly BindableProperty IconProperty = 
			BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(MenuFlyoutItem), default(ImageSource));

		public static readonly BindableProperty TextProperty = 
			BindableProperty.Create(nameof(Text), typeof(string), typeof(MenuFlyoutItem), null);
		
		static readonly BindablePropertyKey IsEnabledPropertyKey = 
			BindableProperty.CreateReadOnly("IsEnabled", typeof(bool), typeof(MenuFlyoutItem), true);

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public ImageSource Icon
		{
			get => (ImageSource)GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}

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

		bool IsEnabledCore
		{
			set => SetValueCore(IsEnabledPropertyKey, value);
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