#nullable disable
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a specialized input control for entering search text with a built-in search button and cancel button.
	/// </summary>
	/// <remarks>
	/// The <see cref="SearchBar"/> provides a user interface optimized for text searches, including a search icon, 
	/// placeholder text, and optionally a cancel button. Use the <see cref="SearchCommand"/> to respond to search requests.
	/// </remarks>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler<SearchBarHandler>]
	public partial class SearchBar : InputView, ITextAlignmentElement, ISearchBarController, IElementConfiguration<SearchBar>, ICommandElement, ISearchBar
	{
		/// <summary>Bindable property for <see cref="ReturnType"/>. This is a bindable property.</summary>
		public static readonly BindableProperty ReturnTypeProperty = BindableProperty.Create(nameof(ReturnType), typeof(ReturnType), typeof(SearchBar), ReturnType.Search);

		/// <summary>Bindable property for <see cref="SearchCommand"/>. This is a bindable property.</summary>
		public static readonly BindableProperty SearchCommandProperty = BindableProperty.Create(
			nameof(SearchCommand), typeof(ICommand), typeof(SearchBar), null,
			propertyChanging: CommandElement.OnCommandChanging, propertyChanged: CommandElement.OnCommandChanged);

		/// <summary>Bindable property for <see cref="SearchCommandParameter"/>. This is a bindable property.</summary>
		public static readonly BindableProperty SearchCommandParameterProperty = BindableProperty.Create(
			nameof(SearchCommandParameter), typeof(object), typeof(SearchBar), null,
			propertyChanged: CommandElement.OnCommandParameterChanged);

		/// <summary>
		/// Bindable property for the text displayed in the search bar.
		/// This is a bindable property.
		/// </summary>
		public new static readonly BindableProperty TextProperty = InputView.TextProperty;

		/// <summary>Bindable property for <see cref="CancelButtonColor"/>. This is a bindable property.</summary>
		public static readonly BindableProperty CancelButtonColorProperty = BindableProperty.Create(nameof(CancelButtonColor), typeof(Color), typeof(SearchBar), default(Color));

		/// <summary>Bindable property for <see cref="SearchIconColor"/>. This is a bindable property.</summary>
		public static readonly BindableProperty SearchIconColorProperty = BindableProperty.Create(nameof(SearchIconColor), typeof(Color), typeof(SearchBar), default(Color));

		/// <summary>
		/// Bindable property for the placeholder text displayed when the search bar is empty.
		/// This is a bindable property.
		/// </summary>
		public new static readonly BindableProperty PlaceholderProperty = InputView.PlaceholderProperty;

		/// <summary>
		/// Bindable property for the color of the placeholder text.
		/// This is a bindable property.
		/// </summary>
		public new static readonly BindableProperty PlaceholderColorProperty = InputView.PlaceholderColorProperty;

		/// <inheritdoc cref="InputView.FontFamilyProperty"/>
		public new static readonly BindableProperty FontFamilyProperty = InputView.FontFamilyProperty;

		/// <inheritdoc cref="InputView.FontSizeProperty"/>
		public new static readonly BindableProperty FontSizeProperty = InputView.FontSizeProperty;

		/// <inheritdoc cref="InputView.FontAttributesProperty"/>
		public new static readonly BindableProperty FontAttributesProperty = InputView.FontAttributesProperty;

		/// <inheritdoc cref="InputView.FontAutoScalingEnabledProperty"/>
		public new static readonly BindableProperty FontAutoScalingEnabledProperty = InputView.FontAutoScalingEnabledProperty;

		/// <summary>
		/// Backing store for the <see cref="InputView.IsTextPredictionEnabled"/> property.
		/// </summary>
		public static new readonly BindableProperty IsTextPredictionEnabledProperty = InputView.IsTextPredictionEnabledProperty;

		/// <inheritdoc cref="InputView.CursorPositionProperty"/>
		public new static readonly BindableProperty CursorPositionProperty = InputView.CursorPositionProperty;

		/// <inheritdoc cref="InputView.SelectionLengthProperty"/>
		public new static readonly BindableProperty SelectionLengthProperty = InputView.SelectionLengthProperty;

		/// <summary>Bindable property for <see cref="HorizontalTextAlignment"/>. This is a bindable property.</summary>
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		/// <summary>Bindable property for <see cref="VerticalTextAlignment"/>. This is a bindable property.</summary>
		public static readonly BindableProperty VerticalTextAlignmentProperty = TextAlignmentElement.VerticalTextAlignmentProperty;

		/// <summary>
		/// Bindable property for the color of the search text.
		/// This is a bindable property.
		/// </summary>
		public new static readonly BindableProperty TextColorProperty = InputView.TextColorProperty;

		/// <summary>
		/// Bindable property for the spacing between characters in the text.
		/// This is a bindable property.
		/// </summary>
		public new static readonly BindableProperty CharacterSpacingProperty = InputView.CharacterSpacingProperty;

		readonly Lazy<PlatformConfigurationRegistry<SearchBar>> _platformConfigurationRegistry;

		/// <summary>
		/// Determines what the return key on the on-screen keyboard should look like. This is a bindable property.
		/// </summary>
		public ReturnType ReturnType
		{
			get => (ReturnType)GetValue(ReturnTypeProperty);
			set => SetValue(ReturnTypeProperty, value);
		}

		/// <summary>
		/// Gets or sets the color of the cancel button.
		/// This is a bindable property.
		/// </summary>
		/// <value>The <see cref="Color"/> of the cancel button. The default is <see langword="null"/>, which uses the platform default.</value>
		public Color CancelButtonColor
		{
			get { return (Color)GetValue(CancelButtonColorProperty); }
			set { SetValue(CancelButtonColorProperty, value); }
		}
		/// <summary>
		/// Gets or sets the color of the search icon in the <see cref="SearchBar"/>.
		/// This is a bindable property.
		/// </summary>
		/// <value>The <see cref="Color"/> of the search icon. The default is <see langword="null"/>, which uses the platform default.</value>
		public Color SearchIconColor
		{
			get { return (Color)GetValue(SearchIconColorProperty); }
			set { SetValue(SearchIconColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets the horizontal alignment of the text within the search bar.
		/// This is a bindable property.
		/// </summary>
		/// <value>A <see cref="TextAlignment"/> value. The default is <see cref="TextAlignment.Start"/>.</value>
		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		/// <summary>
		/// Gets or sets the vertical alignment of the text within the search bar.
		/// This is a bindable property.
		/// </summary>
		/// <value>A <see cref="TextAlignment"/> value. The default is <see cref="TextAlignment.Start"/>.</value>
		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.VerticalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.VerticalTextAlignmentProperty, value); }
		}

		/// <summary>
		/// Gets or sets the command to execute when the user performs a search.
		/// This is a bindable property.
		/// </summary>
		/// <value>An <see cref="ICommand"/> to execute. The default is <see langword="null"/>.</value>
		/// <remarks>
		/// This command is executed when the user presses the search button on the keyboard or when <see cref="OnSearchButtonPressed"/> is called.
		/// The <see cref="SearchCommandParameter"/> is passed as the command parameter.
		/// </remarks>
		public ICommand SearchCommand
		{
			get { return (ICommand)GetValue(SearchCommandProperty); }
			set { SetValue(SearchCommandProperty, value); }
		}

		/// <summary>
		/// Gets or sets the parameter to pass to the <see cref="SearchCommand"/>.
		/// This is a bindable property.
		/// </summary>
		/// <value>The parameter object. The default is <see langword="null"/>.</value>
		public object SearchCommandParameter
		{
			get { return GetValue(SearchCommandParameterProperty); }
			set { SetValue(SearchCommandParameterProperty, value); }
		}

		/// <summary>
		/// Occurs when the user finalizes the search text by pressing the search/return button on the keyboard.
		/// </summary>
		public event EventHandler SearchButtonPressed;

		/// <summary>
		/// Initializes a new instance of the <see cref="SearchBar"/> class.
		/// </summary>
		public SearchBar()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<SearchBar>>(() => new PlatformConfigurationRegistry<SearchBar>(this));
		}

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			base.OnHandlerChangingCore(args);

			if (Application.Current == null)
				return;

			if (args.NewHandler == null || args.OldHandler is not null)
				Application.Current.RequestedThemeChanged -= OnRequestedThemeChanged;
			if (args.NewHandler != null && args.OldHandler == null)
				Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
		}

		private void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
		{
			OnPropertyChanged(nameof(PlaceholderColor));
			OnPropertyChanged(nameof(TextColor));
			OnPropertyChanged(nameof(CancelButtonColor));
		}

		ICommand ICommandElement.Command => SearchCommand;

		object ICommandElement.CommandParameter => SearchCommandParameter;

		protected override bool IsEnabledCore =>
			base.IsEnabledCore && CommandElement.GetCanExecute(this, SearchCommandProperty);

		void ICommandElement.CanExecuteChanged(object sender, EventArgs e) =>
			RefreshIsEnabledProperty();

		/// <summary>
		/// For internal use by the .NET MAUI platform. Raises the <see cref="SearchButtonPressed"/> event and executes the <see cref="SearchCommand"/>.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void OnSearchButtonPressed()
		{
			ICommand cmd = SearchCommand;

			if (cmd != null && !cmd.CanExecute(SearchCommandParameter))
				return;

			cmd?.Execute(SearchCommandParameter);
			SearchButtonPressed?.Invoke(this, EventArgs.Empty);
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, SearchBar> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void ITextAlignmentElement.OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue)
		{
		}

		bool ITextInput.IsTextPredictionEnabled => IsTextPredictionEnabled;

		void ISearchBar.SearchButtonPressed()
		{
			(this as ISearchBarController).OnSearchButtonPressed();
		}

		private protected override string GetDebuggerDisplay()
		{
			var debugText = DebuggerDisplayHelpers.GetDebugText(nameof(SearchCommand), SearchCommand);
			return $"{base.GetDebuggerDisplay()}, {debugText}";
		}

		WeakCommandSubscription ICommandElement.CleanupTracker
		{
			get;
			set;
		}
	}
}
