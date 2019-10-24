using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.WinRT
{
	public class SearchBoxQuerySubmittedEventArgs
		: EventArgs
	{
	}

	public class SearchBoxQueryChangedEventArgs
		: EventArgs
	{
		public SearchBoxQueryChangedEventArgs (string query)
		{
			QueryText = query;
		}

		public string QueryText
		{
			get;
			private set;
		}
	}

	public delegate void QueryChangedEventHandler (SearchBox search, SearchBoxQueryChangedEventArgs args);
	public delegate void QuerySubmittedEventHandler (SearchBox search, SearchBoxQuerySubmittedEventArgs args);

	public sealed partial class SearchBox
	{
		public SearchBox ()
		{
			InitializeComponent ();

			IsEnabledChanged += OnIsEnabledChanged;
		}

		public event QuerySubmittedEventHandler QuerySubmitted;
		public event QueryChangedEventHandler QueryChanged;

		public static readonly DependencyProperty QueryTextProperty = DependencyProperty.Register (
			"QueryText", typeof(string), typeof(SearchBox), new PropertyMetadata (null, OnQueryTextChanged));

		public string QueryText
		{
			get { return (string)GetValue (QueryTextProperty); }
			set { SetValue (QueryTextProperty, value); }
		}

		public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register (
			"PlaceholderText", typeof(string), typeof(SearchBox), new PropertyMetadata (null, OnPlaceholderChanged));

		public string PlaceholderText
		{
			get { return (string)GetValue (PlaceholderTextProperty); }
			set { SetValue (PlaceholderTextProperty, value); }
		}

		public static readonly DependencyProperty HorizontalTextAlignmentProperty = DependencyProperty.Register (
			"HorizontalTextAlignment", typeof(string), typeof(SearchBox), new PropertyMetadata (null, OnAlignmentChanged));

		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue (HorizontalTextAlignmentProperty); }
			set { SetValue (HorizontalTextAlignmentProperty, value); }
		}

		protected override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			GoToNormal ();

			_searchTextBox = (TextBox)GetTemplateChild ("SearchTextBox");

			((Windows.UI.Xaml.Controls.Button) GetTemplateChild ("SearchButton")).Click += OnSearchButtonClicked;

			UpdatePlaceholder ();
			UpdateAlignment ();
		}

		protected override void OnGotFocus (RoutedEventArgs e)
		{
			base.OnGotFocus (e);

			VisualStateManager.GoToState (this, "Focused", true);
		}

		protected override void OnLostFocus (RoutedEventArgs e)
		{
			base.OnLostFocus (e);

			GoToNormal ();
		}

		void OnSearchButtonClicked (object sender, RoutedEventArgs e)
		{
			var querySubmitted = QuerySubmitted;
			if (querySubmitted != null)
				querySubmitted (this, new SearchBoxQuerySubmittedEventArgs());
		}

		TextBox _searchTextBox;

		void GoToNormal ()
		{
			VisualStateManager.GoToState (this, (IsEnabled) ? "Normal" : "Disabled", false);
		}

		void UpdatePlaceholder ()
		{
			if (_searchTextBox == null)
				return;

			_searchTextBox.PlaceholderText = PlaceholderText;
		}

		void OnIsEnabledChanged (object sender, DependencyPropertyChangedEventArgs e)
		{
			string state = "Normal";
			if (!(bool) e.NewValue)
				state = "Disabled";
			else if (FocusState != FocusState.Unfocused)
				state = "Focused";

			VisualStateManager.GoToState (this, state, true);
		}

		static void OnQueryTextChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var search = (SearchBox) d;
			var changed = search.QueryChanged;
			if (changed != null)
				changed (search, new SearchBoxQueryChangedEventArgs ((string) e.NewValue));
		}

		static void OnPlaceholderChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((SearchBox) d).UpdatePlaceholder ();
		}

		static void OnAlignmentChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((SearchBox) d).UpdateAlignment ();
		}

		void UpdateAlignment ()
		{
			if (_searchTextBox == null) {
				return;
			}

			_searchTextBox.TextAlignment = HorizontalTextAlignment.ToNativeTextAlignment();
		}
	}
}
