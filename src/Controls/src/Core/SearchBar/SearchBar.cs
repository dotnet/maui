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
	/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="Type[@FullName='Microsoft.Maui.Controls.SearchBar']/Docs/*" />
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler(typeof(SearchBarHandler))]
	public partial class SearchBar : InputView, ITextAlignmentElement, ISearchBarController, IElementConfiguration<SearchBar>, ICommandElement, ISearchBar
	{
		/// <summary>Bindable property for <see cref="ReturnType"/>.</summary>
		public static readonly BindableProperty ReturnTypeProperty = BindableProperty.Create(nameof(ReturnType), typeof(ReturnType), typeof(SearchBar), ReturnType.Search);

		/// <summary>Bindable property for <see cref="SearchCommand"/>.</summary>
		public static readonly BindableProperty SearchCommandProperty = BindableProperty.Create(
			nameof(SearchCommand), typeof(ICommand), typeof(SearchBar), null,
			propertyChanging: CommandElement.OnCommandChanging, propertyChanged: CommandElement.OnCommandChanged);

		/// <summary>Bindable property for <see cref="SearchCommandParameter"/>.</summary>
		public static readonly BindableProperty SearchCommandParameterProperty = BindableProperty.Create(
			nameof(SearchCommandParameter), typeof(object), typeof(SearchBar), null,
			propertyChanged: CommandElement.OnCommandParameterChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='TextProperty']/Docs/*" />
		public new static readonly BindableProperty TextProperty = InputView.TextProperty;

		/// <summary>Bindable property for <see cref="CancelButtonColor"/>.</summary>
		public static readonly BindableProperty CancelButtonColorProperty = BindableProperty.Create(nameof(CancelButtonColor), typeof(Color), typeof(SearchBar), default(Color));

		/// <summary>Bindable property for <see cref="SearchIconColor"/>.</summary>
		public static readonly BindableProperty SearchIconColorProperty = BindableProperty.Create(nameof(SearchIconColor), typeof(Color), typeof(SearchBar), default(Color));

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='PlaceholderProperty']/Docs/*" />
		public new static readonly BindableProperty PlaceholderProperty = InputView.PlaceholderProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='PlaceholderColorProperty']/Docs/*" />
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

		/// <summary>Bindable property for <see cref="HorizontalTextAlignment"/>.</summary>
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		/// <summary>Bindable property for <see cref="VerticalTextAlignment"/>.</summary>
		public static readonly BindableProperty VerticalTextAlignmentProperty = TextAlignmentElement.VerticalTextAlignmentProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='TextColorProperty']/Docs/*" />
		public new static readonly BindableProperty TextColorProperty = InputView.TextColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='CharacterSpacingProperty']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='CancelButtonColor']/Docs/*" />
		public Color CancelButtonColor
		{
			get { return (Color)GetValue(CancelButtonColorProperty); }
			set { SetValue(CancelButtonColorProperty, value); }
		}
		/// <summary>
		/// Gets or sets the color of the search icon in the <see cref="SearchBar"/>.
		/// </summary>
		public Color SearchIconColor
		{
			get { return (Color)GetValue(SearchIconColorProperty); }
			set { SetValue(SearchIconColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='HorizontalTextAlignment']/Docs/*" />
		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='VerticalTextAlignment']/Docs/*" />
		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.VerticalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.VerticalTextAlignmentProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='SearchCommand']/Docs/*" />
		public ICommand SearchCommand
		{
			get { return (ICommand)GetValue(SearchCommandProperty); }
			set { SetValue(SearchCommandProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='SearchCommandParameter']/Docs/*" />
		public object SearchCommandParameter
		{
			get { return GetValue(SearchCommandParameterProperty); }
			set { SetValue(SearchCommandParameterProperty, value); }
		}

		public event EventHandler SearchButtonPressed;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
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
			base.IsEnabledCore && CommandElement.GetCanExecute(this);

		void ICommandElement.CanExecuteChanged(object sender, EventArgs e) =>
			RefreshIsEnabledProperty();

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='OnSearchButtonPressed']/Docs/*" />
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
