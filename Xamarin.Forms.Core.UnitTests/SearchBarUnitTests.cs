using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class SearchBarUnitTests : BaseTestFixture
	{
		[Test]
		public void TestConstructor()
		{
			SearchBar searchBar = new SearchBar();

			Assert.Null(searchBar.Placeholder);
			Assert.Null(searchBar.Text);
		}

		[Test]
		public void TestContentsChanged()
		{
			SearchBar searchBar = new SearchBar();

			bool thrown = false;

			searchBar.TextChanged += (sender, e) => thrown = true;

			searchBar.Text = "Foo";

			Assert.True(thrown);
		}

		[Test]
		public void TestSearchButtonPressed()
		{
			SearchBar searchBar = new SearchBar();

			bool thrown = false;
			searchBar.SearchButtonPressed += (sender, e) => thrown = true;

			((ISearchBarController)searchBar).OnSearchButtonPressed();

			Assert.True(thrown);
		}

		[Test]
		public void TestSearchCommandParameter()
		{
			var searchBar = new SearchBar();

			object param = "Testing";
			object result = null;
			searchBar.SearchCommand = new Command(p => { result = p; });
			searchBar.SearchCommandParameter = param;

			((ISearchBarController)searchBar).OnSearchButtonPressed();

			Assert.AreEqual(param, result);
		}

		[TestCase(null, "Text Changed")]
		[TestCase("Initial Text", null)]
		[TestCase("Initial Text", "Text Changed")]
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

			Assert.AreEqual(searchBar, searchBarFromSender);
			Assert.AreEqual(initialText, oldText);
			Assert.AreEqual(finalText, newText);
		}

		[Test]
		public void CommandCanExecuteUpdatesEnabled()
		{
			var searchBar = new SearchBar();

			bool result = false;

			var bindingContext = new
			{
				Command = new Command(() => { }, () => result)
			};

			searchBar.SetBinding(SearchBar.SearchCommandProperty, "Command");
			searchBar.BindingContext = bindingContext;

			Assert.False(searchBar.IsEnabled);

			result = true;

			bindingContext.Command.ChangeCanExecute();

			Assert.True(searchBar.IsEnabled);
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

		[Test]
		public void DoesNotCrashWithNonCommandICommand()
		{
			var searchBar = new SearchBar();
			Assert.DoesNotThrow(() => searchBar.SearchCommand = new MyCommand());
		}
	}
}