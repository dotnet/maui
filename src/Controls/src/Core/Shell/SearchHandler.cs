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
	/// <summary>
	/// Provides search functionality in a <see cref="Shell"/> application.
	/// </summary>
	public class SearchHandler : BindableObject, ISearchHandlerController, IPlaceholderElement, IFontElement, ITextElement, ITextAlignmentElement
	{
		/// <summary>Bindable property key for <see cref="IsFocused"/>.</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindablePropertyKey IsFocusedPropertyKey = BindableProperty.CreateReadOnly(nameof(IsFocused),
			typeof(bool), typeof(VisualElement), default(bool), propertyChanged: OnIsFocusedPropertyChanged);

		public event EventHandler<EventArgs> Focused;
		public event EventHandler<EventArgs> Unfocused;

		internal event EventHandler<EventArgs> ShowSoftInputRequested;
		internal event EventHandler<EventArgs> HideSoftInputRequested;

		/// <summary>Bindable property for <see cref="IsFocused"/>.</summary>
		public static readonly BindableProperty IsFocusedProperty = IsFocusedPropertyKey.BindableProperty;

		/// <summary>Gets a value indicating whether this search handler currently has focus. This is a bindable property.</summary>
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

		/// <summary>Sets the value of the <see cref="IsFocused"/> property. For internal use by platform renderers.</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsFocused(bool value)
		{
			SetValue(IsFocusedPropertyKey, value, specificity: SetterSpecificity.FromHandler);
		}
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<FocusRequestArgs> FocusChangeRequested;

		/// <summary>Sets focus to the search handler, causing the input field to become the current focus.</summary>
		/// <returns><see langword="true"/> if the search handler gained focus; otherwise, <see langword="false"/>.</returns>
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

		/// <summary>Removes focus from the search handler.</summary>
		public void Unfocus()
		{
			if (!IsFocused)
				return;

			FocusChangeRequested?.Invoke(this, new FocusRequestArgs());
		}

		public void ShowSoftInputAsync()
		{
			ShowSoftInputRequested?.Invoke(this, new EventArgs());
		}

		public void HideSoftInputAsync()
		{
			HideSoftInputRequested?.Invoke(this, new EventArgs());
		}

		protected virtual void OnFocused()
		{

		}

		protected virtual void OnUnfocus()
		{

		}

		/// <summary>Bindable property for <see cref="Keyboard"/>.</summary>
		public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(SearchHandler), Keyboard.Default, coerceValue: (o, v) => (Keyboard)v ?? Keyboard.Default);

		/// <summary>Gets or sets the keyboard type for the search input. This is a bindable property.</summary>
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

		/// <summary>Gets or sets the horizontal alignment of the search text. This is a bindable property.</summary>
		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		/// <summary>Gets or sets the vertical alignment of the search text. This is a bindable property.</summary>
		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.VerticalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.VerticalTextAlignmentProperty, value); }
		}

		/// <summary>Bindable property for <see cref="TextColor"/>.</summary>
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <summary>Bindable property for <see cref="CharacterSpacing"/>.</summary>
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <summary>Gets or sets the color of the search text. This is a bindable property.</summary>
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

		/// <summary>Gets or sets the text transformation applied to the search text. This is a bindable property.</summary>
		public TextTransform TextTransform
		{
			get => (TextTransform)GetValue(TextTransformProperty);
			set => SetValue(TextTransformProperty, value);
		}

		void ITextElement.OnTextTransformChanged(TextTransform oldValue, TextTransform newValue)
		{
		}

		/// <summary>Returns the transformed text using the specified <see cref="TextTransform"/>.</summary>
		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilities.GetTransformedText(source, textTransform);

		/// <summary>Gets or sets the color of the cancel button. This is a bindable property.</summary>
		public Color CancelButtonColor
		{
			get { return (Color)GetValue(CancelButtonColorProperty); }
			set { SetValue(CancelButtonColorProperty, value); }
		}


		/// <summary>Gets or sets the font attributes for the search text. This is a bindable property.</summary>
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		/// <summary>Gets or sets the font family for the search text. This is a bindable property.</summary>
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <summary>Gets or sets the character spacing for the search text. This is a bindable property.</summary>
		public double CharacterSpacing
		{
			get { return (double)GetValue(TextElement.CharacterSpacingProperty); }
			set { SetValue(TextElement.CharacterSpacingProperty, value); }
		}

		/// <summary>Gets or sets the font size for the search text. This is a bindable property.</summary>
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

		/// <summary>Gets or sets the color of the placeholder text. This is a bindable property.</summary>
		public Color PlaceholderColor
		{
			get => (Color)GetValue(PlaceholderElement.PlaceholderColorProperty);
			set => SetValue(PlaceholderElement.PlaceholderColorProperty, value);
		}

		/// <summary>Gets or sets the text displayed when the search box is empty. This is a bindable property.</summary>
		public string Placeholder
		{
			get => (string)GetValue(PlaceholderElement.PlaceholderProperty);
			set => SetValue(PlaceholderElement.PlaceholderProperty, value);
		}

		/// <summary>Bindable property for <see cref="BackgroundColor"/>.</summary>
		public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(SearchHandler), null);

		/// <summary>Gets or sets the background color of the search box. This is a bindable property.</summary>
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

#pragma warning disable CS0618
		/// <summary>Bindable property for <see cref="DisplayMemberName"/>.</summary>
		public static readonly BindableProperty DisplayMemberNameProperty =
			BindableProperty.Create(nameof(DisplayMemberName), typeof(string), typeof(SearchHandler), null, BindingMode.OneTime);
#pragma warning restore CS0618

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

		/// <summary>Bindable property for <see cref="SelectedItem"/>.</summary>
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

		/// <summary>Gets or sets the icon displayed for the clear button. This is a bindable property.</summary>
		public ImageSource ClearIcon
		{
			get { return (ImageSource)GetValue(ClearIconProperty); }
			set { SetValue(ClearIconProperty, value); }
		}

		/// <summary>Gets or sets the accessibility help text for the clear icon. This is a bindable property.</summary>
		public string ClearIconHelpText
		{
			get { return (string)GetValue(ClearIconHelpTextProperty); }
			set { SetValue(ClearIconHelpTextProperty, value); }
		}

		/// <summary>Gets or sets the accessibility name for the clear icon. This is a bindable property.</summary>
		public string ClearIconName
		{
			get { return (string)GetValue(ClearIconNameProperty); }
			set { SetValue(ClearIconNameProperty, value); }
		}

		/// <summary>Gets or sets the command invoked when the clear placeholder button is pressed. This is a bindable property.</summary>
		public ICommand ClearPlaceholderCommand
		{
			get { return (ICommand)GetValue(ClearPlaceholderCommandProperty); }
			set { SetValue(ClearPlaceholderCommandProperty, value); }
		}

		/// <summary>Gets or sets the parameter passed to <see cref="ClearPlaceholderCommand"/>. This is a bindable property.</summary>
		public object ClearPlaceholderCommandParameter
		{
			get { return GetValue(ClearPlaceholderCommandParameterProperty); }
			set { SetValue(ClearPlaceholderCommandParameterProperty, value); }
		}

		/// <summary>Gets or sets a value indicating whether the clear placeholder button is enabled. This is a bindable property.</summary>
		public bool ClearPlaceholderEnabled
		{
			get { return (bool)GetValue(ClearPlaceholderEnabledProperty); }
			set { SetValue(ClearPlaceholderEnabledProperty, value); }
		}

		/// <summary>Gets or sets the accessibility help text for the clear placeholder icon. This is a bindable property.</summary>
		public string ClearPlaceholderHelpText
		{
			get { return (string)GetValue(ClearPlaceholderHelpTextProperty); }
			set { SetValue(ClearPlaceholderHelpTextProperty, value); }
		}

		/// <summary>Gets or sets the icon displayed for the clear placeholder button. This is a bindable property.</summary>
		public ImageSource ClearPlaceholderIcon
		{
			get { return (ImageSource)GetValue(ClearPlaceholderIconProperty); }
			set { SetValue(ClearPlaceholderIconProperty, value); }
		}

		/// <summary>Gets or sets the accessibility name for the clear placeholder icon. This is a bindable property.</summary>
		public string ClearPlaceholderName
		{
			get { return (string)GetValue(ClearPlaceholderNameProperty); }
			set { SetValue(ClearPlaceholderNameProperty, value); }
		}

		/// <summary>Gets or sets the command invoked when the search query is confirmed. This is a bindable property.</summary>
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <summary>Gets or sets the parameter passed to <see cref="Command"/>. This is a bindable property.</summary>
		public object CommandParameter
		{
			get { return (object)GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <summary>Gets or sets the name of the property to display for search results. This is a bindable property.</summary>
		[Obsolete("Use ItemTemplate instead.")]
		public string DisplayMemberName
		{
			get { return (string)GetValue(DisplayMemberNameProperty); }
			set { SetValue(DisplayMemberNameProperty, value); }
		}

		/// <summary>Gets or sets a value indicating whether search is enabled. This is a bindable property.</summary>
		public bool IsSearchEnabled
		{
			get { return (bool)GetValue(IsSearchEnabledProperty); }
			set { SetValue(IsSearchEnabledProperty, value); }
		}

		/// <summary>Gets or sets the collection of items to display as search suggestions. This is a bindable property.</summary>
		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		/// <summary>Gets or sets the template for displaying search result items. This is a bindable property.</summary>
		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		/// <summary>Gets or sets the current search query text. This is a bindable property.</summary>
		public string Query
		{
			get { return (string)GetValue(QueryProperty); }
			set { SetValue(QueryProperty, value); }
		}

		/// <summary>Gets or sets the icon displayed for the search query. This is a bindable property.</summary>
		public ImageSource QueryIcon
		{
			get { return (ImageSource)GetValue(QueryIconProperty); }
			set { SetValue(QueryIconProperty, value); }
		}

		/// <summary>Gets or sets the accessibility help text for the query icon. This is a bindable property.</summary>
		public string QueryIconHelpText
		{
			get { return (string)GetValue(QueryIconHelpTextProperty); }
			set { SetValue(QueryIconHelpTextProperty, value); }
		}

		/// <summary>Gets or sets the accessibility name for the query icon. This is a bindable property.</summary>
		public string QueryIconName
		{
			get { return (string)GetValue(QueryIconNameProperty); }
			set { SetValue(QueryIconNameProperty, value); }
		}

		/// <summary>Gets or sets the visibility mode of the search box. This is a bindable property.</summary>
		public SearchBoxVisibility SearchBoxVisibility
		{
			get { return (SearchBoxVisibility)GetValue(SearchBoxVisibilityProperty); }
			set { SetValue(SearchBoxVisibilityProperty, value); }
		}

		/// <summary>Gets the currently selected search result item. This is a bindable property.</summary>
		public object SelectedItem => GetValue(SelectedItemProperty);

		/// <summary>Gets or sets a value indicating whether search results are displayed. This is a bindable property.</summary>
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

		internal WeakCommandSubscription ClearPlaceholderCommandSubscription { get; set; }

		void OnClearPlaceholderCommandChanged(ICommand oldCommand, ICommand newCommand)
		{
			ClearPlaceholderCommandSubscription?.Dispose();
			ClearPlaceholderCommandSubscription = null;

			if (newCommand != null)
			{
				ClearPlaceholderCommandSubscription = new WeakCommandSubscription(this, newCommand, ClearPlaceholderCanExecuteChanged);
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

		internal WeakCommandSubscription CommandSubscription { get; set; }

		void OnCommandChanged(ICommand oldCommand, ICommand newCommand)
		{
			CommandSubscription?.Dispose();
			CommandSubscription = null;

			if (newCommand is not null)
			{
				CommandSubscription = new WeakCommandSubscription(this, newCommand, CanExecuteChanged);
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
