#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Controls.VisualElement;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="Type[@FullName='Microsoft.Maui.Controls.SearchHandler']/Docs/*" />
	public class SearchHandler : BindableObject, ISearchHandlerController, IPlaceholderElement, IFontElement, ITextElement, ITextAlignmentElement
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='IsFocusedPropertyKey']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindablePropertyKey IsFocusedPropertyKey = BindableProperty.CreateReadOnly(nameof(IsFocused),
			typeof(bool), typeof(VisualElement), default(bool), propertyChanged: OnIsFocusedPropertyChanged);

		public event EventHandler<EventArgs> Focused;
		public event EventHandler<EventArgs> Unfocused;

		/// <summary>Bindable property for <see cref="IsFocused"/>.</summary>
		public static readonly BindableProperty IsFocusedProperty = IsFocusedPropertyKey.BindableProperty;

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='IsFocused']/Docs/*" />
		public bool IsFocused
		{
			get { return (bool)GetValue(IsFocusedProperty); }
		}

		static void OnIsFocusedPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			var element = (SearchHandler)bindable;

			if (element == null)
			{
				return;
			}

			var isFocused = (bool)newvalue;
			if (isFocused)
			{
				element.Focused?.Invoke(element, new EventArgs());
				element.OnFocused();
			}
			else
			{
				element.Unfocused?.Invoke(element, new EventArgs());
				element.OnUnfocus();
			}
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='SetIsFocused']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsFocused(bool value)
		{
			SetValue(IsFocusedPropertyKey, value, specificity: SetterSpecificity.FromHandler);
		}
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<FocusRequestArgs> FocusChangeRequested;

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='Focus']/Docs/*" />
		public bool Focus()
		{
			if (IsFocused)
				return true;

			if (FocusChangeRequested == null)
				return false;

			var arg = new FocusRequestArgs { Focus = true };
			FocusChangeRequested(this, arg);
			return arg.Result;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='Unfocus']/Docs/*" />
		public void Unfocus()
		{
			if (!IsFocused)
				return;

			FocusChangeRequested?.Invoke(this, new FocusRequestArgs());
		}

		protected virtual void OnFocused()
		{

		}

		protected virtual void OnUnfocus()
		{

		}

		/// <summary>Bindable property for <see cref="Keyboard"/>.</summary>
		public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(SearchHandler), Keyboard.Default, coerceValue: (o, v) => (Keyboard)v ?? Keyboard.Default);

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='Keyboard']/Docs/*" />
		public Keyboard Keyboard
		{
			get { return (Keyboard)GetValue(KeyboardProperty); }
			set { SetValue(KeyboardProperty, value); }
		}

		/// <summary>Bindable property for <see cref="HorizontalTextAlignment"/>.</summary>
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		/// <summary>Bindable property for <see cref="VerticalTextAlignment"/>.</summary>
		public static readonly BindableProperty VerticalTextAlignmentProperty = TextAlignmentElement.VerticalTextAlignmentProperty;

		void ITextAlignmentElement.OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue)
		{
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='HorizontalTextAlignment']/Docs/*" />
		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='VerticalTextAlignment']/Docs/*" />
		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.VerticalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.VerticalTextAlignmentProperty, value); }
		}

		/// <summary>Bindable property for <see cref="TextColor"/>.</summary>
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <summary>Bindable property for <see cref="CharacterSpacing"/>.</summary>
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='TextColor']/Docs/*" />
		public Color TextColor
		{
			get { return (Color)GetValue(TextElement.TextColorProperty); }
			set { SetValue(TextElement.TextColorProperty, value); }
		}

		/// <summary>Bindable property for <see cref="CancelButtonColor"/>.</summary>
		public static readonly BindableProperty CancelButtonColorProperty = BindableProperty.Create(nameof(CancelButtonColor), typeof(Color), typeof(SearchHandler), default(Color));

		/// <summary>Bindable property for <see cref="FontFamily"/>.</summary>
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <summary>Bindable property for <see cref="FontSize"/>.</summary>
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <summary>Bindable property for <see cref="FontAttributes"/>.</summary>
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		/// <summary>Bindable property for <see cref="FontAutoScalingEnabled"/>.</summary>
		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <summary>Bindable property for <see cref="Placeholder"/>.</summary>
		public static readonly BindableProperty PlaceholderProperty = PlaceholderElement.PlaceholderProperty;

		/// <summary>Bindable property for <see cref="PlaceholderColor"/>.</summary>
		public static readonly BindableProperty PlaceholderColorProperty = PlaceholderElement.PlaceholderColorProperty;

		/// <summary>Bindable property for <see cref="TextTransform"/>.</summary>
		public static readonly BindableProperty TextTransformProperty = TextElement.TextTransformProperty;

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='TextTransform']/Docs/*" />
		public TextTransform TextTransform
		{
			get => (TextTransform)GetValue(TextTransformProperty);
			set => SetValue(TextTransformProperty, value);
		}

		void ITextElement.OnTextTransformChanged(TextTransform oldValue, TextTransform newValue)
		{
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='UpdateFormsText']/Docs/*" />
		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilites.GetTransformedText(source, textTransform);

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='CancelButtonColor']/Docs/*" />
		public Color CancelButtonColor
		{
			get { return (Color)GetValue(CancelButtonColorProperty); }
			set { SetValue(CancelButtonColorProperty, value); }
		}


		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='FontAttributes']/Docs/*" />
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='FontFamily']/Docs/*" />
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='CharacterSpacing']/Docs/*" />
		public double CharacterSpacing
		{
			get { return (double)GetValue(TextElement.CharacterSpacingProperty); }
			set { SetValue(TextElement.CharacterSpacingProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='FontSize']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public bool FontAutoScalingEnabled
		{
			get => (bool)GetValue(FontAutoScalingEnabledProperty);
			set => SetValue(FontAutoScalingEnabledProperty, value);
		}

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue)
		{
		}

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue)
		{
		}

		void IFontElement.OnFontAutoScalingEnabledChanged(bool oldValue, bool newValue)
		{
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
			Application.Current.GetDefaultFontSize();

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue)
		{
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='PlaceholderColor']/Docs/*" />
		public Color PlaceholderColor
		{
			get => (Color)GetValue(PlaceholderElement.PlaceholderColorProperty);
			set => SetValue(PlaceholderElement.PlaceholderColorProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='Placeholder']/Docs/*" />
		public string Placeholder
		{
			get => (string)GetValue(PlaceholderElement.PlaceholderProperty);
			set => SetValue(PlaceholderElement.PlaceholderProperty, value);
		}

		/// <summary>Bindable property for <see cref="BackgroundColor"/>.</summary>
		public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(SearchHandler), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='BackgroundColor']/Docs/*" />
		public Color BackgroundColor
		{
			get { return (Color)GetValue(BackgroundColorProperty); }
			set { SetValue(BackgroundColorProperty, value); }
		}

		event EventHandler<ListProxyChangedEventArgs> ISearchHandlerController.ListProxyChanged
		{
			add { _listProxyChanged += value; }
			remove { _listProxyChanged -= value; }
		}

		event EventHandler<ListProxyChangedEventArgs> _listProxyChanged;

		IReadOnlyList<object> ISearchHandlerController.ListProxy => ListProxy;

		ListProxy ListProxy
		{
			get { return _listProxy; }
			set
			{
				if (_listProxy == value)
					return;
				var oldProxy = _listProxy;
				_listProxy = value;
				_listProxyChanged?.Invoke(this, new ListProxyChangedEventArgs(oldProxy, value));
			}
		}

		void ISearchHandlerController.ClearPlaceholderClicked()
		{
			OnClearPlaceholderClicked();
		}

		void ISearchHandlerController.ItemSelected(object obj)
		{
			OnItemSelected(obj);
			SetValue(SelectedItemPropertyKey, obj, specificity: SetterSpecificity.FromHandler);
		}

		void ISearchHandlerController.QueryConfirmed()
		{
			OnQueryConfirmed();
		}

		/// <summary>Bindable property for <see cref="AutomationId"/>.</summary>
		public static readonly BindableProperty AutomationIdProperty = BindableProperty.Create(nameof(AutomationId), typeof(string), typeof(SearchHandler), null);

		/// <summary>Bindable property for <see cref="ClearIconHelpText"/>.</summary>
		public static readonly BindableProperty ClearIconHelpTextProperty =
			BindableProperty.Create(nameof(ClearIconHelpText), typeof(string), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: (b, o, n) => ((SearchHandler)b).UpdateAutomationProperties());

		/// <summary>Bindable property for <see cref="ClearIconName"/>.</summary>
		public static readonly BindableProperty ClearIconNameProperty =
			BindableProperty.Create(nameof(ClearIconName), typeof(string), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: (b, o, n) => ((SearchHandler)b).UpdateAutomationProperties());

		/// <summary>Bindable property for <see cref="ClearIcon"/>.</summary>
		public static readonly BindableProperty ClearIconProperty =
			BindableProperty.Create(nameof(ClearIcon), typeof(ImageSource), typeof(SearchHandler), null, BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="ClearPlaceholderCommandParameter"/>.</summary>
		public static readonly BindableProperty ClearPlaceholderCommandParameterProperty =
			BindableProperty.Create(nameof(ClearPlaceholderCommandParameter), typeof(object), typeof(SearchHandler), null,
				propertyChanged: OnClearPlaceholderCommandParameterChanged);

		/// <summary>Bindable property for <see cref="ClearPlaceholderCommand"/>.</summary>
		public static readonly BindableProperty ClearPlaceholderCommandProperty =
			BindableProperty.Create(nameof(ClearPlaceholderCommand), typeof(ICommand), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: OnClearPlaceholderCommandChanged);

		/// <summary>Bindable property for <see cref="ClearPlaceholderEnabled"/>.</summary>
		public static readonly BindableProperty ClearPlaceholderEnabledProperty =
			BindableProperty.Create(nameof(ClearPlaceholderEnabled), typeof(bool), typeof(SearchHandler), false);

		/// <summary>Bindable property for <see cref="ClearPlaceholderHelpText"/>.</summary>
		public static readonly BindableProperty ClearPlaceholderHelpTextProperty =
			BindableProperty.Create(nameof(ClearPlaceholderHelpText), typeof(string), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: (b, o, n) => ((SearchHandler)b).UpdateAutomationProperties());

		/// <summary>Bindable property for <see cref="ClearPlaceholderIcon"/>.</summary>
		public static readonly BindableProperty ClearPlaceholderIconProperty =
			BindableProperty.Create(nameof(ClearPlaceholderIcon), typeof(ImageSource), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: (b, o, n) => ((SearchHandler)b).UpdateAutomationProperties());

		/// <summary>Bindable property for <see cref="ClearPlaceholderName"/>.</summary>
		public static readonly BindableProperty ClearPlaceholderNameProperty =
			BindableProperty.Create(nameof(ClearPlaceholderName), typeof(string), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: (b, o, n) => ((SearchHandler)b).UpdateAutomationProperties());

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty =
			BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SearchHandler), null,
				propertyChanged: OnCommandParameterChanged);

		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty =
			BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: OnCommandChanged);

		/// <summary>Bindable property for <see cref="DisplayMemberName"/>.</summary>
		public static readonly BindableProperty DisplayMemberNameProperty =
			BindableProperty.Create(nameof(DisplayMemberName), typeof(string), typeof(SearchHandler), null, BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="IsSearchEnabled"/>.</summary>
		public static readonly BindableProperty IsSearchEnabledProperty =
			BindableProperty.Create(nameof(IsSearchEnabled), typeof(bool), typeof(SearchHandler), true, BindingMode.OneWay);

		/// <summary>Bindable property for <see cref="ItemsSource"/>.</summary>
		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: OnItemsSourceChanged);

		/// <summary>Bindable property for <see cref="ItemTemplate"/>.</summary>
		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(SearchHandler), null, BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="QueryIconHelpText"/>.</summary>
		public static readonly BindableProperty QueryIconHelpTextProperty =
			BindableProperty.Create(nameof(QueryIconHelpText), typeof(string), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: (b, o, n) => ((SearchHandler)b).UpdateAutomationProperties());

		/// <summary>Bindable property for <see cref="QueryIconName"/>.</summary>
		public static readonly BindableProperty QueryIconNameProperty =
			BindableProperty.Create(nameof(QueryIconName), typeof(string), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: (b, o, n) => ((SearchHandler)b).UpdateAutomationProperties());

		/// <summary>Bindable property for <see cref="QueryIcon"/>.</summary>
		public static readonly BindableProperty QueryIconProperty =
			BindableProperty.Create(nameof(QueryIcon), typeof(ImageSource), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: (b, o, n) => ((SearchHandler)b).UpdateAutomationProperties());

		/// <summary>Bindable property for <see cref="Query"/>.</summary>
		public static readonly BindableProperty QueryProperty =
			BindableProperty.Create(nameof(Query), typeof(string), typeof(SearchHandler), null, BindingMode.TwoWay,
				propertyChanged: OnQueryChanged);

		/// <summary>Bindable property for <see cref="SearchBoxVisibility"/>.</summary>
		public static readonly BindableProperty SearchBoxVisibilityProperty =
			BindableProperty.Create(nameof(SearchBoxVisibility), typeof(SearchBoxVisibility), typeof(SearchHandler), SearchBoxVisibility.Expanded, BindingMode.OneWay);

		static readonly BindablePropertyKey SelectedItemPropertyKey =
			BindableProperty.CreateReadOnly(nameof(SelectedItem), typeof(object), typeof(SearchHandler), null, BindingMode.OneWayToSource);

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='SelectedItemProperty']/Docs/*" />
		public static BindableProperty SelectedItemProperty = SelectedItemPropertyKey.BindableProperty;

		/// <summary>Bindable property for <see cref="ShowsResults"/>.</summary>
		public static readonly BindableProperty ShowsResultsProperty =
			BindableProperty.Create(nameof(ShowsResults), typeof(bool), typeof(SearchHandler), false, BindingMode.OneTime);

		private ListProxy _listProxy;

		public string AutomationId
		{
			get { return (string)GetValue(AutomationIdProperty); }
			set { SetValue(AutomationIdProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='ClearIcon']/Docs/*" />
		public ImageSource ClearIcon
		{
			get { return (ImageSource)GetValue(ClearIconProperty); }
			set { SetValue(ClearIconProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='ClearIconHelpText']/Docs/*" />
		public string ClearIconHelpText
		{
			get { return (string)GetValue(ClearIconHelpTextProperty); }
			set { SetValue(ClearIconHelpTextProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='ClearIconName']/Docs/*" />
		public string ClearIconName
		{
			get { return (string)GetValue(ClearIconNameProperty); }
			set { SetValue(ClearIconNameProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='ClearPlaceholderCommand']/Docs/*" />
		public ICommand ClearPlaceholderCommand
		{
			get { return (ICommand)GetValue(ClearPlaceholderCommandProperty); }
			set { SetValue(ClearPlaceholderCommandProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='ClearPlaceholderCommandParameter']/Docs/*" />
		public object ClearPlaceholderCommandParameter
		{
			get { return GetValue(ClearPlaceholderCommandParameterProperty); }
			set { SetValue(ClearPlaceholderCommandParameterProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='ClearPlaceholderEnabled']/Docs/*" />
		public bool ClearPlaceholderEnabled
		{
			get { return (bool)GetValue(ClearPlaceholderEnabledProperty); }
			set { SetValue(ClearPlaceholderEnabledProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='ClearPlaceholderHelpText']/Docs/*" />
		public string ClearPlaceholderHelpText
		{
			get { return (string)GetValue(ClearPlaceholderHelpTextProperty); }
			set { SetValue(ClearPlaceholderHelpTextProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='ClearPlaceholderIcon']/Docs/*" />
		public ImageSource ClearPlaceholderIcon
		{
			get { return (ImageSource)GetValue(ClearPlaceholderIconProperty); }
			set { SetValue(ClearPlaceholderIconProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='ClearPlaceholderName']/Docs/*" />
		public string ClearPlaceholderName
		{
			get { return (string)GetValue(ClearPlaceholderNameProperty); }
			set { SetValue(ClearPlaceholderNameProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='Command']/Docs/*" />
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='CommandParameter']/Docs/*" />
		public object CommandParameter
		{
			get { return (object)GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='DisplayMemberName']/Docs/*" />
		public string DisplayMemberName
		{
			get { return (string)GetValue(DisplayMemberNameProperty); }
			set { SetValue(DisplayMemberNameProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='IsSearchEnabled']/Docs/*" />
		public bool IsSearchEnabled
		{
			get { return (bool)GetValue(IsSearchEnabledProperty); }
			set { SetValue(IsSearchEnabledProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='ItemsSource']/Docs/*" />
		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='ItemTemplate']/Docs/*" />
		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='Query']/Docs/*" />
		public string Query
		{
			get { return (string)GetValue(QueryProperty); }
			set { SetValue(QueryProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='QueryIcon']/Docs/*" />
		public ImageSource QueryIcon
		{
			get { return (ImageSource)GetValue(QueryIconProperty); }
			set { SetValue(QueryIconProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='QueryIconHelpText']/Docs/*" />
		public string QueryIconHelpText
		{
			get { return (string)GetValue(QueryIconHelpTextProperty); }
			set { SetValue(QueryIconHelpTextProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='QueryIconName']/Docs/*" />
		public string QueryIconName
		{
			get { return (string)GetValue(QueryIconNameProperty); }
			set { SetValue(QueryIconNameProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='SearchBoxVisibility']/Docs/*" />
		public SearchBoxVisibility SearchBoxVisibility
		{
			get { return (SearchBoxVisibility)GetValue(SearchBoxVisibilityProperty); }
			set { SetValue(SearchBoxVisibilityProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='SelectedItem']/Docs/*" />
		public object SelectedItem => GetValue(SelectedItemProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/SearchHandler.xml" path="//Member[@MemberName='ShowsResults']/Docs/*" />
		public bool ShowsResults
		{
			get { return (bool)GetValue(ShowsResultsProperty); }
			set { SetValue(ShowsResultsProperty, value); }
		}

		bool ClearPlaceholderEnabledCore { set => SetValue(ClearPlaceholderEnabledProperty, value); }

		bool IsSearchEnabledCore { set => SetValue(IsSearchEnabledProperty, value); }

		protected virtual void OnClearPlaceholderClicked()
		{
			var command = ClearPlaceholderCommand;
			var commandParameter = ClearPlaceholderCommandParameter;
			if (command != null && command.CanExecute(commandParameter))
			{
				command.Execute(commandParameter);
			}
		}

		protected virtual void OnItemSelected(object item)
		{
		}

		protected virtual void OnQueryChanged(string oldValue, string newValue)
		{
		}

		protected virtual void OnQueryConfirmed()
		{
			var command = Command;
			var commandParameter = CommandParameter;
			if (command?.CanExecute(commandParameter) == true)
			{
				command.Execute(commandParameter);
			}
		}

		static void OnClearPlaceholderCommandChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (SearchHandler)bindable;
			var oldCommand = (ICommand)oldValue;
			var newCommand = (ICommand)newValue;
			self.OnClearPlaceholderCommandChanged(oldCommand, newCommand);
		}

		static void OnClearPlaceholderCommandParameterChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((SearchHandler)bindable).OnClearPlaceholderCommandParameterChanged();
		}

		static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (SearchHandler)bindable;
			var oldCommand = (ICommand)oldValue;
			var newCommand = (ICommand)newValue;
			self.OnCommandChanged(oldCommand, newCommand);
		}

		static void OnCommandParameterChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((SearchHandler)bindable).OnCommandParameterChanged();
		}

		void ITextElement.OnCharacterSpacingPropertyChanged(double oldValue, double newValue)
		{

		}

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{

		}

		static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (SearchHandler)bindable;
			if (newValue == null)
				self.ListProxy = null;
			else
				self.ListProxy = new ListProxy((IEnumerable)newValue, dispatcher: self.Dispatcher);
		}

		static void OnQueryChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var searchHandler = (SearchHandler)bindable;
			searchHandler.OnQueryChanged((string)oldValue, (string)newValue);
		}

		void CanExecuteChanged(object sender, EventArgs e)
		{
			IsSearchEnabledCore = Command.CanExecute(CommandParameter);
		}

		void ClearPlaceholderCanExecuteChanged(object sender, EventArgs e)
		{
			ClearPlaceholderEnabledCore = ClearPlaceholderCommand.CanExecute(ClearPlaceholderCommandParameter);
		}

		void OnClearPlaceholderCommandChanged(ICommand oldCommand, ICommand newCommand)
		{
			if (oldCommand != null)
			{
				oldCommand.CanExecuteChanged -= ClearPlaceholderCanExecuteChanged;
			}

			if (newCommand != null)
			{
				newCommand.CanExecuteChanged += ClearPlaceholderCanExecuteChanged;
				ClearPlaceholderEnabledCore = ClearPlaceholderCommand.CanExecute(ClearPlaceholderCommandParameter);
			}
			else
			{
				ClearPlaceholderEnabledCore = true;
			}
		}

		void OnClearPlaceholderCommandParameterChanged()
		{
			if (ClearPlaceholderCommand != null)
				ClearPlaceholderEnabledCore = ClearPlaceholderCommand.CanExecute(CommandParameter);
		}

		void OnCommandChanged(ICommand oldCommand, ICommand newCommand)
		{
			if (oldCommand != null)
			{
				oldCommand.CanExecuteChanged -= CanExecuteChanged;
			}

			if (newCommand != null)
			{
				newCommand.CanExecuteChanged += CanExecuteChanged;
				IsSearchEnabledCore = Command.CanExecute(CommandParameter);
			}
			else
			{
				IsSearchEnabledCore = true;
			}
		}

		void OnCommandParameterChanged()
		{
			if (Command != null)
				IsSearchEnabledCore = Command.CanExecute(CommandParameter);
		}

		void UpdateAutomationProperties()
		{
			var queryIcon = QueryIcon;
			var clearIcon = ClearIcon;
			var clearPlaceholderIcon = ClearPlaceholderIcon;

			if (queryIcon != null)
			{
				var queryIconName = QueryIconName;
				var queryIconHelpText = QueryIconHelpText;
				if (queryIconName != null)
					AutomationProperties.SetName(queryIcon, queryIconName);

				if (queryIconHelpText != null)
					AutomationProperties.SetHelpText(queryIcon, queryIconHelpText);
			}

			if (clearIcon != null)
			{
				var clearIconName = ClearIconName;
				var clearIconHelpText = ClearIconHelpText;
				if (clearIconName != null)
					AutomationProperties.SetName(clearIcon, clearIconName);

				if (clearIconHelpText != null)
					AutomationProperties.SetHelpText(clearIcon, clearIconHelpText);
			}

			if (clearPlaceholderIcon != null)
			{
				var clearPlaceholderName = ClearPlaceholderName;
				var clearPlacholderHelpText = ClearPlaceholderHelpText;
				if (clearPlaceholderName != null)
					AutomationProperties.SetName(clearPlaceholderIcon, clearPlaceholderName);

				if (clearPlacholderHelpText != null)
					AutomationProperties.SetHelpText(clearPlaceholderIcon, clearPlacholderHelpText);
			}
		}
	}
}
