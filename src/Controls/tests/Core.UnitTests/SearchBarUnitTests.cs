using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class SearchBarUnitTests : VisualElementCommandSourceTests<SearchBar>
	{
		[Fact]
		public void TestConstructor()
		{
			SearchBar searchBar = new SearchBar();

			Assert.Null(searchBar.Placeholder);
			Assert.Null(searchBar.Text);
		}

		[Fact]
		public void TestContentsChanged()
		{
			SearchBar searchBar = new SearchBar();

			bool thrown = false;

			searchBar.TextChanged += (sender, e) => thrown = true;

			searchBar.Text = "Foo";

			Assert.True(thrown);
		}

		[Fact]
		public void TestSearchButtonPressed()
		{
			SearchBar searchBar = new SearchBar();

			bool thrown = false;
			searchBar.SearchButtonPressed += (sender, e) => thrown = true;

			((ISearchBarController)searchBar).OnSearchButtonPressed();

			Assert.True(thrown);
		}

		[Fact]
		public void TestSearchCommandParameter()
		{
			var searchBar = new SearchBar();

			object param = "Testing";
			object result = null;
			searchBar.SearchCommand = new Command(p => { result = p; });
			searchBar.SearchCommandParameter = param;

			((ISearchBarController)searchBar).OnSearchButtonPressed();

			Assert.Equal(param, result);
		}

		[Theory]
		[InlineData(null, "Text Changed")]
		[InlineData("Initial Text", null)]
		[InlineData("Initial Text", "Text Changed")]
		public void SearchBarTextChangedEventArgs(string initialText, string finalText)
		{
			var searchBar = new SearchBar
			{
				Text = initialText
			};

			SearchBar searchBarFromSender = null;
			string oldText = null;
			string newText = null;

			searchBar.TextChanged += (s, e) =>
			{
				searchBarFromSender = (SearchBar)s;
				oldText = e.OldTextValue;
				newText = e.NewTextValue;
			};

			searchBar.Text = finalText;

			Assert.Equal(searchBar, searchBarFromSender);
			Assert.Equal(initialText, oldText);
			Assert.Equal(finalText, newText);
		}

		class MyCommand : ICommand
		{
			public bool CanExecute(object parameter)
			{
				return true;
			}

			public void Execute(object parameter)
			{
			}

			public event EventHandler CanExecuteChanged;
		}

		[Fact]
		public void DoesNotCrashWithNonCommandICommand()
		{
			var searchBar = new SearchBar();
			searchBar.SearchCommand = new MyCommand();
		}

		[Fact]
		public void TestSearchCommandExecutesWithEmptyText()
		{
			var searchBar = new SearchBar();

			bool executed = false;
			string executedWithParameter = null;
			searchBar.SearchCommand = new Command<string>(p => { 
				executed = true; 
				executedWithParameter = p;
			});
			
			// Set empty text explicitly
			searchBar.Text = "";
			searchBar.SearchCommandParameter = searchBar.Text;

			((ISearchBarController)searchBar).OnSearchButtonPressed();

			Assert.True(executed);
			Assert.Equal("", executedWithParameter);
		}

		[Fact]
		public void TestSearchCommandExecutesWithNullText()
		{
			var searchBar = new SearchBar();

			bool executed = false;
			object executedWithParameter = "not_set";
			searchBar.SearchCommand = new Command<object>(p => { 
				executed = true; 
				executedWithParameter = p;
			});
			
			// Set null text explicitly
			searchBar.Text = null;
			searchBar.SearchCommandParameter = searchBar.Text;

			((ISearchBarController)searchBar).OnSearchButtonPressed();

			Assert.True(executed);
			Assert.Null(executedWithParameter);
		}

		[Fact]
		public void TestSearchCommandExecutesWhenTextBecomesEmpty()
		{
			var searchBar = new SearchBar();

			bool executed = false;
			string executedWithParameter = null;
			searchBar.SearchCommand = new Command<string>(p => { 
				executed = true; 
				executedWithParameter = p;
			});
			
			// Set initial non-empty text
			searchBar.Text = "initial text";
			
			// Reset the flag
			executed = false;
			
			// Clear the text - this should trigger SearchCommand
			// Set SearchCommandParameter to empty text before clearing
			searchBar.SearchCommandParameter = "";
			searchBar.Text = "";

			Assert.True(executed);
			Assert.Equal("", executedWithParameter);
		}

		[Fact]
		public void TestSearchCommandExecutesWhenTextBecomesNull()
		{
			var searchBar = new SearchBar();

			bool executed = false;
			object executedWithParameter = "not_set";
			searchBar.SearchCommand = new Command<object>(p => { 
				executed = true; 
				executedWithParameter = p;
			});
			
			// Set initial non-empty text
			searchBar.Text = "initial text";
			
			// Reset the flag
			executed = false;
			
			// Clear the text to null - this should trigger SearchCommand
			// Set SearchCommandParameter to null before clearing
			searchBar.SearchCommandParameter = null;
			searchBar.Text = null;

			Assert.True(executed);
			Assert.Null(executedWithParameter);
		}

		[Fact]
		public void TestSearchCommandDoesNotExecuteWhenTextRemainsEmpty()
		{
			var searchBar = new SearchBar();

			bool executed = false;
			searchBar.SearchCommand = new Command<string>(p => { 
				executed = true;
			});
			
			// Start with empty text
			searchBar.Text = "";
			
			// Reset the flag
			executed = false;
			
			// Set to empty again - this should NOT trigger SearchCommand
			searchBar.Text = "";

			Assert.False(executed);
		}

		[Fact]
		public void TestSearchCommandDoesNotExecuteWhenTextRemainsNull()
		{
			var searchBar = new SearchBar();

			bool executed = false;
			searchBar.SearchCommand = new Command<object>(p => { 
				executed = true;
			});
			
			// Start with null text (default)
			Assert.Null(searchBar.Text);
			
			// Reset the flag
			executed = false;
			
			// Set to null again - this should NOT trigger SearchCommand
			searchBar.Text = null;

			Assert.False(executed);
		}

		protected override BindableProperty IsEnabledProperty => SearchBar.IsEnabledProperty;

		protected override BindableProperty CommandProperty => SearchBar.SearchCommandProperty;

		protected override BindableProperty CommandParameterProperty => SearchBar.SearchCommandParameterProperty;

		protected override SearchBar CreateSource() => new SearchBar();

		protected override void Activate(SearchBar source) => ((ISearchBarController)source).OnSearchButtonPressed();
	}
}
