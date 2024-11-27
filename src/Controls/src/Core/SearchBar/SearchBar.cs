#nullable disable
using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="Type[@FullName='Microsoft.Maui.Controls.SearchBar']/Docs/*" />
	public partial class SearchBar : InputView, ITextAlignmentElement, ISearchBarController, IElementConfiguration<SearchBar>, ICommandElement, ISearchBar
	{
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

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='CancelButtonColor']/Docs/*" />
		public Color CancelButtonColor
		{
			get { return (Color)GetValue(CancelButtonColorProperty); }
			set { SetValue(CancelButtonColorProperty, value); }
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

		bool ITextInput.IsTextPredictionEnabled => true;

		void ISearchBar.SearchButtonPressed()
		{
			(this as ISearchBarController).OnSearchButtonPressed();
		}
	}
}
