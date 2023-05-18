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

		protected override BindableProperty IsEnabledProperty => SearchBar.IsEnabledProperty;

		protected override BindableProperty CommandProperty => SearchBar.SearchCommandProperty;

		protected override BindableProperty CommandParameterProperty => SearchBar.SearchCommandParameterProperty;

		protected override SearchBar CreateSource() => new SearchBar();

		protected override void Activate(SearchBar source) => ((ISearchBarController)source).OnSearchButtonPressed();
	}
}
