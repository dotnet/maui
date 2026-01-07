using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ShellParameterPassingTests : ShellTestBase
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ReNavigatingToCurrentLocationPassesParameters(bool useDataTemplates)
		{
			var shell = new Shell();
			ShellTestPage pagetoTest = new ShellTestPage();
			pagetoTest.BindingContext = pagetoTest;
			var one = CreateShellItem(pagetoTest, shellContentRoute: "content", templated: useDataTemplates);
			shell.Items.Add(one);
			ShellTestPage page = null;
			if (useDataTemplates)
			{
				page = (ShellTestPage)(one.CurrentItem.CurrentItem as IShellContentController).GetOrCreateContent();
			}
			else
			{
				page = (ShellTestPage)one.CurrentItem.CurrentItem.Content;
			}

			Assert.Null(page.SomeQueryParameter);
			await shell.GoToAsync($"//content?{nameof(ShellTestPage.SomeQueryParameter)}=1234");
			Assert.Equal("1234", page.SomeQueryParameter);
			await shell.GoToAsync($"//content?{nameof(ShellTestPage.SomeQueryParameter)}=4321");
			Assert.Equal("4321", page.SomeQueryParameter);
			await shell.GoToAsync($"//content?{nameof(ShellTestPage.SomeQueryParameter)}");
			Assert.Empty(page.SomeQueryParameter);
		}

		[Fact]
		public async Task DotDotNavigationPassesParameters()
		{
			Routing.RegisterRoute(nameof(DotDotNavigationPassesParameters), typeof(ContentPage));
			var shell = new Shell();
			var one = new ShellItem { Route = "one" };

			var tabone = MakeSimpleShellSection("tabone", "content");

			one.Items.Add(tabone);

			shell.Items.Add(one);

			one.CurrentItem.CurrentItem.ContentTemplate = new DataTemplate(() =>
			{
				ShellTestPage pagetoTest = new ShellTestPage();
				pagetoTest.BindingContext = pagetoTest;
				return pagetoTest;
			});

			var page = (ShellTestPage)(one.CurrentItem.CurrentItem as IShellContentController).GetOrCreateContent();
			Assert.Null(page.SomeQueryParameter);
			await shell.GoToAsync(nameof(DotDotNavigationPassesParameters));
			await shell.GoToAsync($"..?{nameof(ShellTestPage.SomeQueryParameter)}=1234");
			Assert.Equal("1234", page.SomeQueryParameter);

		}

		[Fact]
		public async Task NavigationWithQueryStringWhenPageMatchesBindingContext()
		{
			var shell = new Shell();

			var one = new ShellItem { Route = "one" };
			var two = new ShellItem { Route = "two" };

			var tabone = MakeSimpleShellSection("tabone", "content");
			var tabfour = MakeSimpleShellSection("tabfour", "content", null);

			one.Items.Add(tabone);
			two.Items.Add(tabfour);

			shell.Items.Add(one);
			shell.Items.Add(two);

			ShellTestPage pagetoTest = new ShellTestPage();
			await shell.GoToAsync(new ShellNavigationState($"//two/tabfour/content?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			two.CurrentItem.CurrentItem.ContentTemplate = new DataTemplate(() =>
			{
				pagetoTest = new ShellTestPage();
				pagetoTest.BindingContext = pagetoTest;
				return pagetoTest;
			});


			var page = (two.CurrentItem.CurrentItem as IShellContentController).GetOrCreateContent();
			Assert.Equal("1234", (page as ShellTestPage).SomeQueryParameter);

		}


		[Fact]
		public async Task NavigationWithQueryStringThenWithoutQueryString()
		{
			var shell = new Shell();

			var one = new ShellItem { Route = "one" };
			var two = new ShellItem { Route = "two" };

			var tabone = MakeSimpleShellSection("tabone", "content");
			var tabfour = MakeSimpleShellSection("tabfour", "content", null);

			one.Items.Add(tabone);
			two.Items.Add(tabfour);

			shell.Items.Add(one);
			shell.Items.Add(two);

			ShellTestPage pagetoTest = new ShellTestPage();
			await shell.GoToAsync(new ShellNavigationState($"//two/tabfour/content?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			two.CurrentItem.CurrentItem.ContentTemplate = new DataTemplate(() =>
			{
				pagetoTest = new ShellTestPage();
				pagetoTest.BindingContext = pagetoTest;
				return pagetoTest;
			});


			await shell.GoToAsync(new ShellNavigationState($"//one/tabone/content"));
			await shell.GoToAsync(new ShellNavigationState($"//two/tabfour/content"));

			var page = (two.CurrentItem.CurrentItem as IShellContentController).GetOrCreateContent();
			Assert.Null((page as ShellTestPage).SomeQueryParameter);
		}


		[Fact]
		public async Task NavigationBetweenShellContentsPassesQueryString()
		{
			var shell = new Shell();

			var item = CreateShellItem(shellSectionRoute: "section2");
			var content = CreateShellContent(shellContentRoute: "content");
			item.Items[0].Items.Add(content);

			Routing.RegisterRoute("details", typeof(ShellTestPage));

			shell.Items.Add(item);


			await shell.GoToAsync(new ShellNavigationState($"//section2/details?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			await shell.GoToAsync(new ShellNavigationState($"//content?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			await shell.GoToAsync(new ShellNavigationState($"//section2/details?{nameof(ShellTestPage.SomeQueryParameter)}=4321"));

			var testPage = shell.CurrentPage as ShellTestPage;
			Assert.Equal("4321", testPage.SomeQueryParameter);
		}

		[Fact]
		public async Task NavigationBetweenFlyoutItemsRetainsQueryString()
		{
			var testPage1 = new ShellTestPage();
			var testPage2 = new ShellTestPage();

			var flyoutItem1 = CreateShellItem<FlyoutItem>(testPage1, shellItemRoute: "flyoutItem1");
			var flyoutItem2 = CreateShellItem<FlyoutItem>(testPage2, shellItemRoute: "flyoutItem2");

			var shell = new TestShell(flyoutItem1, flyoutItem2);
			var complexParameter = new object();

			IShellController shellController = shell;
			await shell.GoToAsync(new ShellNavigationState($"//flyoutItem2?{nameof(ShellTestPage.SomeQueryParameter)}=1234"), false,
				new Dictionary<string, object>()
				{
					{nameof(ShellTestPage.ComplexObject), complexParameter }
				});

			Assert.Equal("1234", testPage2.SomeQueryParameter);
			Assert.Equal(complexParameter, testPage2.ComplexObject);
			await shellController.OnFlyoutItemSelectedAsync(flyoutItem1);
			await shellController.OnFlyoutItemSelectedAsync(flyoutItem2);
			Assert.Equal("1234", testPage2.SomeQueryParameter);
			Assert.Equal(complexParameter, testPage2.ComplexObject);
		}

		[Fact]
		public async Task NavigationBetweenFlyoutItemWithPushedPageRetainsQueryString()
		{
			var testPage1 = new ShellTestPage();
			var testPage2 = new ShellTestPage();

			var flyoutItem1 = CreateShellItem<FlyoutItem>(testPage1, shellItemRoute: "flyoutItem1");
			var flyoutItem2 = CreateShellItem<FlyoutItem>(testPage2, shellItemRoute: "flyoutItem2");
			Routing.RegisterRoute("details", typeof(ShellTestPage));


			var shell = new TestShell(flyoutItem1, flyoutItem2);

			IShellController shellController = shell;
			await shell.GoToAsync(new ShellNavigationState($"//flyoutItem2/details?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));

			await shellController.OnFlyoutItemSelectedAsync(flyoutItem1);
			await shellController.OnFlyoutItemSelectedAsync(flyoutItem2);

			var testPage = shell.CurrentPage as ShellTestPage;
			Assert.Equal("1234", testPage.SomeQueryParameter);
		}

		[Fact]
		public async Task BasicQueryStringTest()
		{
			var shell = new Shell();

			var item = CreateShellItem(shellSectionRoute: "section2");
			Routing.RegisterRoute("details", typeof(ShellTestPage));
			shell.Items.Add(item);
			await shell.GoToAsync(new ShellNavigationState($"details?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			var testPage = shell.CurrentPage as ShellTestPage;
			Assert.Equal("1234", testPage.SomeQueryParameter);
		}

		[Fact]
		public async Task NavigationWithQueryStringAndNoDataTemplate()
		{
			var shell = new Shell();

			var one = new ShellItem { Route = "one" };
			var two = new ShellItem { Route = "two" };

			var tabone = MakeSimpleShellSection("tabone", "content");
			var tabfour = MakeSimpleShellSection("tabfour", "content");

			one.Items.Add(tabone);
			two.Items.Add(tabfour);

			shell.Items.Add(one);
			shell.Items.Add(two);

			await shell.GoToAsync(new ShellNavigationState($"//two/tabfour/content?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			Assert.Equal("1234", (two.CurrentItem.CurrentItem.Content as ShellTestPage).SomeQueryParameter);
		}


		[Fact]
		public async Task UriDecodesQueryString()
		{
			var shell = new Shell();
			var item = CreateShellItem();
			Routing.RegisterRoute("details", typeof(ShellTestPage));
			shell.Items.Add(item);

			await shell.GoToAsync(new ShellNavigationState($"details?{nameof(ShellTestPage.SomeQueryParameter)}=1 2 3 4 % ^"));
			var testPage = shell.CurrentPage as ShellTestPage;
			Assert.Equal("1 2 3 4 % ^", testPage.SomeQueryParameter);
		}

		[Fact]
		public async Task UriEncodedStringDecodesCorrectly()
		{
			var shell = new Shell();
			var item = CreateShellItem();
			Routing.RegisterRoute("details", typeof(ShellTestPage));
			shell.Items.Add(item);

			string result = "http://www.place.com 1 2 3 4 % ()(";
			string parameter = System.Net.WebUtility.UrlEncode(result);

			await shell.GoToAsync(new ShellNavigationState($"details?{nameof(ShellTestPage.SomeQueryParameter)}={parameter}"));
			var testPage = shell.CurrentPage as ShellTestPage;
			Assert.Equal(result, testPage.SomeQueryParameter);
		}

		[Fact]
		public async Task UrlParameter()
		{
			var shell = new Shell();
			var item = CreateShellItem();
			Routing.RegisterRoute("details", typeof(ShellTestPage));
			shell.Items.Add(item);

			string urlTest = @"https://www.somewebsite.com/id/545/800/600.jpg";
			await shell.GoToAsync(new ShellNavigationState($"details?{nameof(ShellTestPage.SomeQueryParameter)}={urlTest}"));
			var testPage = shell.CurrentPage as ShellTestPage;
			Assert.Equal(urlTest, testPage.SomeQueryParameter);
		}

		[Fact]
		public async Task SetParameterOfTypeThatsNotAString()
		{
			var shell = new Shell();
			var item = CreateShellItem();
			Routing.RegisterRoute("details", typeof(ShellTestPage));
			shell.Items.Add(item);
			await shell.GoToAsync(new ShellNavigationState($"details?{nameof(ShellTestPage.DoubleQueryParameter)}=1234"));
			var testPage = shell.CurrentPage as ShellTestPage;
			Assert.Equal(1234d, testPage.DoubleQueryParameter);
		}

		[Fact]
		public async Task NavigatingBackDoesntClearParametersFromPreviousPage()
		{
			var shell = new TestShell(CreateShellItem());

			Routing.RegisterRoute("details", typeof(ShellTestPage));

			await shell.GoToAsync($"details?{nameof(ShellTestPage.SomeQueryParameter)}=1");
			await shell.GoToAsync("details");
			await shell.GoToAsync("..");
			var testPage = shell.CurrentPage as ShellTestPage;
			Assert.Equal("1", testPage.SomeQueryParameter);

		}

		[Fact]
		public async Task NavigatingBackAbsolutelyClearsParametersFromPreviousPage()
		{
			var shell = new TestShell(CreateShellItem(shellItemRoute: "item"));

			Routing.RegisterRoute("details", typeof(ShellTestPage));

			await shell.GoToAsync($"details?{nameof(ShellTestPage.SomeQueryParameter)}=1");
			await shell.GoToAsync("details");
			await shell.GoToAsync("//item/details");
			var testPage = shell.CurrentPage as ShellTestPage;
			Assert.Null(testPage.SomeQueryParameter);

		}

		[Fact]
		public async Task NavigatingBackToShellContentRetainsQueryParameter()
		{
			var shell = new Shell();
			ShellTestPage pagetoTest = new ShellTestPage();
			pagetoTest.BindingContext = pagetoTest;
			var one = CreateShellItem(pagetoTest, shellContentRoute: "content");
			shell.Items.Add(one);
			ShellTestPage page = (ShellTestPage)shell.CurrentPage;
			await shell.GoToAsync($"//content?{nameof(ShellTestPage.SomeQueryParameter)}=1234");
			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PopAsync();
			Assert.Equal("1234", page.SomeQueryParameter);
		}

		[Fact]
		public async Task NavigatingBackToShellContentAbsolutelyClearsQueryParameter()
		{
			var shell = new Shell();
			ShellTestPage pagetoTest = new ShellTestPage();
			pagetoTest.BindingContext = pagetoTest;
			var one = CreateShellItem(pagetoTest, shellContentRoute: "content");
			shell.Items.Add(one);
			ShellTestPage page = (ShellTestPage)shell.CurrentPage;
			await shell.GoToAsync($"//content?{nameof(ShellTestPage.SomeQueryParameter)}=1234");
			await shell.Navigation.PushAsync(new ContentPage());
			await shell.GoToAsync($"//content");
			Assert.Null(page.SomeQueryParameter);
		}

		[Fact]
		public async Task BasicShellParameterTest()
		{
			var shell = new Shell();

			var item = CreateShellItem(shellSectionRoute: "section2");
			Routing.RegisterRoute("details", typeof(ShellTestPage));
			shell.Items.Add(item);
			var obj = new object();
			var parameter = new ShellRouteParameters
			{
				{"DoubleQueryParameter", 2d },
				{ "ComplexObject", obj}
			};

			await shell.GoToAsync(new ShellNavigationState($"details?{nameof(ShellTestPage.SomeQueryParameter)}=1234"), parameter);
			var testPage = shell.CurrentPage as ShellTestPage;
			Assert.Equal("1234", testPage.SomeQueryParameter);
			Assert.Equal(2d, testPage.DoubleQueryParameter);
			Assert.Equal(obj, testPage.ComplexObject);
		}

		[Fact]
		public async Task ValidateReadOnlyDictionary()
		{
			var obj = new object();
			var parameter = new ShellNavigationQueryParameters
			{
				{"DoubleQueryParameter", 2d },
				{ "ComplexObject", obj}
			}.SetToReadOnly();

			Assert.Throws<InvalidOperationException>(() => parameter.Add("key", "value"));
			Assert.Throws<InvalidOperationException>(() => parameter.Add(new KeyValuePair<string, object>("key", "value")));
			Assert.Throws<InvalidOperationException>(() => parameter.Remove(parameter.First()));
			Assert.Throws<InvalidOperationException>(() => parameter.Remove("DoubleQueryParameter"));
			Assert.Throws<InvalidOperationException>(() => parameter["key"] = "value");
			Assert.Throws<InvalidOperationException>(() => parameter.Clear());
		}

		[Fact]
		public async Task ShellNavigationQueryParametersPassedInAsReadOnly()
		{
			var shell = new Shell();
			var item = CreateShellItem(shellSectionRoute: "section2");
			Routing.RegisterRoute("details", typeof(ShellTestPage));
			shell.Items.Add(item);
			var obj = new object();
			var parameter = new ShellNavigationQueryParameters
			{
				{"DoubleQueryParameter", 2d },
				{ "ComplexObject", obj}
			};

			await shell.GoToAsync(new ShellNavigationState($"details"), parameter);
			var testPage = shell.CurrentPage as ShellTestPage;
			Assert.True(testPage.AppliedQueryAttributes[0].IsReadOnly);
			Assert.False(parameter.IsReadOnly);
			Assert.Single(testPage.AppliedQueryAttributes);
		}

		[Fact]
		public async Task ExtraParametersDontGetRetained()
		{
			var shell = new Shell();
			var item = CreateShellItem(shellSectionRoute: "section2");
			Routing.RegisterRoute("details", typeof(ShellTestPage));
			shell.Items.Add(item);
			var obj = new object();
			var parameter = new ShellNavigationQueryParameters
			{
				{"DoubleQueryParameter", 2d },
				{"ComplexObject", obj}
			};

			await shell.GoToAsync(new ShellNavigationState($"details?{nameof(ShellTestPage.SomeQueryParameter)}=1234"), parameter);
			var testPage = shell.CurrentPage as ShellTestPage;

			await shell.Navigation.PushAsync(new ContentPage());

			testPage.SomeQueryParameter = String.Empty;
			testPage.DoubleQueryParameter = -1d;
			testPage.ComplexObject = null;

			await shell.GoToAsync("..");

			Assert.Equal("1234", testPage.SomeQueryParameter);
			Assert.Equal(-1d, testPage.DoubleQueryParameter);
			Assert.Null(testPage.ComplexObject);

			// ensure that AppliedQueryAttributes is called with correct parameters each time
			Assert.Equal(2, testPage.AppliedQueryAttributes.Count);
			Assert.Equal(3, testPage.AppliedQueryAttributes[0].Count);
			Assert.Single(testPage.AppliedQueryAttributes[1]);
			Assert.Equal($"{nameof(ShellTestPage.SomeQueryParameter)}", testPage.AppliedQueryAttributes[1].Keys.First());
		}

		[Fact]
		public async Task ExtraParametersArentReAppliedWhenNavigatingBackToShellContent()
		{
			var shell = new Shell();
			var item = CreateShellItem(shellContentRoute: "start");
			var withParams = CreateShellItem(page: new ShellTestPage(), shellContentRoute: "withParams", templated: true);
			shell.Items.Add(item);
			shell.Items.Add(withParams);
			var obj = new object();
			var parameter = new ShellRouteParameters
			{
				{ "ComplexObject", obj},
				{ nameof(ShellTestPage.SomeQueryParameter), "1234"}
			};

			await shell.GoToAsync(new ShellNavigationState($"//start"));
			await shell.GoToAsync(new ShellNavigationState($"//withParams"), parameter);

			var testPage = (shell.CurrentItem.CurrentItem.CurrentItem as IShellContentController).GetOrCreateContent() as ShellTestPage;

			// Validate parameter was set during first navigation
			Assert.Equal(obj, testPage.ComplexObject);

			// Clear parameters
			testPage.ComplexObject = null;
			testPage.SomeQueryParameter = null;

			// Navigate away and back to page with params
			await shell.GoToAsync(new ShellNavigationState($"//start"));
			shell.CurrentItem = withParams;
			await Task.Yield();

			var testPage2 = shell.CurrentPage as ShellTestPage;
			Assert.Null(testPage2.SomeQueryParameter);
			Assert.Null(testPage2.ComplexObject);
			Assert.Equal(testPage2, testPage);
		}

		[Fact]
		public async Task SingleUseQueryParametersReplaceQueryStringParams()
		{
			var shell = new Shell();
			var item = CreateShellItem(shellSectionRoute: "section2");
			Routing.RegisterRoute("details", typeof(ShellTestPage));
			shell.Items.Add(item);

			var parameter = new ShellNavigationQueryParameters()
			{
				{nameof(ShellTestPage.SomeQueryParameter), "4321" }
			};

			await shell.GoToAsync(new ShellNavigationState($"details?{nameof(ShellTestPage.SomeQueryParameter)}=1234"), parameter);
			var testPage = shell.CurrentPage as ShellTestPage;

			// Parameters passed in will win
			Assert.Equal("4321", testPage.SomeQueryParameter);
			await shell.Navigation.PushAsync(new ContentPage());
			testPage.SomeQueryParameter = "TheseDontGetSetAgain";
			await shell.GoToAsync("..");

			Assert.Equal("TheseDontGetSetAgain", testPage.SomeQueryParameter);
		}

		[Fact]
		public async Task DotDotNavigationPassesShellParameters()
		{
			Routing.RegisterRoute(nameof(DotDotNavigationPassesParameters), typeof(ContentPage));
			var shell = new Shell();
			var one = new ShellItem { Route = "one" };

			var tabone = MakeSimpleShellSection("tabone", "content");

			one.Items.Add(tabone);

			shell.Items.Add(one);

			one.CurrentItem.CurrentItem.ContentTemplate = new DataTemplate(() =>
			{
				ShellTestPage pagetoTest = new ShellTestPage();
				pagetoTest.BindingContext = pagetoTest;
				return pagetoTest;
			});

			var obj = new object();
			var parameter = new ShellRouteParameters
			{
				{"DoubleQueryParameter", 2d },
				{ "ComplexObject", obj}
			};

			var page = (ShellTestPage)(one.CurrentItem.CurrentItem as IShellContentController).GetOrCreateContent();
			Assert.Null(page.SomeQueryParameter);
			await shell.GoToAsync(nameof(DotDotNavigationPassesParameters));
			await shell.GoToAsync($"..?{nameof(ShellTestPage.SomeQueryParameter)}=1234", parameter);
			Assert.Equal("1234", page.SomeQueryParameter);
			Assert.Equal(2d, page.DoubleQueryParameter);
			Assert.Equal(obj, page.ComplexObject);
		}

		[Fact]
		public async Task PassesUrlInShellParameter()
		{
			var shell = new Shell();
			var item = CreateShellItem();
			Routing.RegisterRoute("details", typeof(ShellTestPage));
			shell.Items.Add(item);
			string urlTest = @"https://www.somewebsite.com/id/545/800/600.jpg";

			var parameter = new ShellRouteParameters
			{
				{ nameof(ShellTestPage.SomeQueryParameter) ,urlTest }
			};

			await shell.GoToAsync(new ShellNavigationState($"details"), parameter);
			var testPage = shell.CurrentPage as ShellTestPage;
			Assert.Equal(urlTest, testPage.SomeQueryParameter);
		}

		[Fact]
		public void InitialNavigationDoesntSetQueryAttributesProperty()
		{
			var content = CreateShellContent();
			_ = new TestShell(content);
			Assert.False(content.IsSet(ShellContent.QueryAttributesProperty));
		}

		// Tests for Issue #13537 - TabBar navigation should invoke ApplyQueryAttributes
		// These tests verify that QueryAttributesProperty is set on ShellContent when switching tabs,
		// which triggers ApplyQueryAttributes on the page/viewmodel implementing IQueryAttributable.

		[Fact]
		public async Task TabBarNavigationSetsQueryAttributesProperty()
		{
			// Arrange: Create a TabBar with two tabs
			var testPage1 = new ShellTestPage();
			var testPage2 = new ShellTestPage();

			var content1 = new ShellContent { Content = testPage1, Route = "content1" };
			var content2 = new ShellContent { Content = testPage2, Route = "content2" };

			var tab1 = new Tab { Title = "Tab1", Route = "tab1" };
			tab1.Items.Add(content1);

			var tab2 = new Tab { Title = "Tab2", Route = "tab2" };
			tab2.Items.Add(content2);

			var tabBar = new TabBar();
			tabBar.Items.Add(tab1);
			tabBar.Items.Add(tab2);

			var shell = new TestShell();
			shell.Items.Add(tabBar);

			// Verify initial state - QueryAttributesProperty not set on tab2's content
			Assert.False(content2.IsSet(ShellContent.QueryAttributesProperty),
				"QueryAttributesProperty should not be set initially");

			// Act: Switch to the second tab
			tabBar.CurrentItem = tab2;
			await Task.Yield();

			// Assert: QueryAttributesProperty should now be set on tab2's content
			// This is what triggers ApplyQueryAttributes on the page
			Assert.True(content2.IsSet(ShellContent.QueryAttributesProperty),
				"QueryAttributesProperty should be set when switching tabs (Issue #13537)");
		}

		[Fact]
		public async Task TabBarNavigationWithGoToAsyncSetsQueryAttributesProperty()
		{
			// Arrange: Create a TabBar with two tabs
			var testPage1 = new ShellTestPage();
			var testPage2 = new ShellTestPage();

			var content1 = new ShellContent { Content = testPage1, Route = "content1" };
			var content2 = new ShellContent { Content = testPage2, Route = "content2" };

			var tab1 = new Tab { Title = "Tab1", Route = "tab1" };
			tab1.Items.Add(content1);

			var tab2 = new Tab { Title = "Tab2", Route = "tab2" };
			tab2.Items.Add(content2);

			var tabBar = new TabBar();
			tabBar.Items.Add(tab1);
			tabBar.Items.Add(tab2);

			var shell = new TestShell();
			shell.Items.Add(tabBar);

			// Act: Navigate to tab2 using GoToAsync without parameters
			await shell.GoToAsync("///tab2/content2");

			// Assert: QueryAttributesProperty should be set
			Assert.True(content2.IsSet(ShellContent.QueryAttributesProperty),
				"QueryAttributesProperty should be set when navigating via GoToAsync (Issue #13537)");
		}

		// Tests for Issue #28453 - ShellContent routes should call ApplyQueryAttributes
		[Fact]
		public async Task FlyoutItemNavigationSetsQueryAttributesProperty()
		{
			// Arrange: Create Shell with two FlyoutItems
			var testPage1 = new ShellTestPage();
			var testPage2 = new ShellTestPage();

			var flyoutItem1 = CreateShellItem<FlyoutItem>(testPage1, shellItemRoute: "flyout1");
			var flyoutItem2 = CreateShellItem<FlyoutItem>(testPage2, shellItemRoute: "flyout2");

			var shell = new TestShell(flyoutItem1, flyoutItem2);

			// Get the ShellContent for flyoutItem2
			var content2 = flyoutItem2.CurrentItem.CurrentItem;

			// Verify initial state
			Assert.False(content2.IsSet(ShellContent.QueryAttributesProperty),
				"QueryAttributesProperty should not be set initially");

			// Act: Navigate to the second flyout item
			IShellController shellController = shell;
			await shellController.OnFlyoutItemSelectedAsync(flyoutItem2);

			// Assert: QueryAttributesProperty should now be set
			Assert.True(content2.IsSet(ShellContent.QueryAttributesProperty),
				"QueryAttributesProperty should be set when switching FlyoutItems (Issue #28453)");
		}

		[Fact]
		public async Task NavigatingBackToTabSetsQueryAttributesProperty()
		{
			// Arrange: Create a TabBar with two tabs
			var testPage1 = new ShellTestPage();
			var testPage2 = new ShellTestPage();

			var content1 = new ShellContent { Content = testPage1, Route = "content1" };
			var content2 = new ShellContent { Content = testPage2, Route = "content2" };

			var tab1 = new Tab { Title = "Tab1", Route = "tab1" };
			tab1.Items.Add(content1);

			var tab2 = new Tab { Title = "Tab2", Route = "tab2" };
			tab2.Items.Add(content2);

			var tabBar = new TabBar();
			tabBar.Items.Add(tab1);
			tabBar.Items.Add(tab2);

			var shell = new TestShell();
			shell.Items.Add(tabBar);

			// Act: Navigate tab1 -> tab2 -> tab1
			tabBar.CurrentItem = tab2;
			await Task.Yield();

			Assert.True(content2.IsSet(ShellContent.QueryAttributesProperty),
				"QueryAttributesProperty should be set when switching to tab2");

			tabBar.CurrentItem = tab1;
			await Task.Yield();

			// Assert: QueryAttributesProperty should be set on tab1 when navigating back
			Assert.True(content1.IsSet(ShellContent.QueryAttributesProperty),
				"QueryAttributesProperty should be set when switching back to tab1");
		}

		[Fact]
		public async Task PopNavigationTriggersApplyQueryAttributes()
		{
			// Arrange
			var shell = new TestShell(CreateShellItem());
			Routing.RegisterRoute("details", typeof(ShellTestPage));

			// Navigate to details page with parameter
			await shell.GoToAsync($"details?{nameof(ShellTestPage.SomeQueryParameter)}=1234");
			var detailsPage = shell.CurrentPage as ShellTestPage;
			Assert.Equal("1234", detailsPage.SomeQueryParameter);

			// Navigate to another page
			await shell.GoToAsync("details");
			var secondPage = shell.CurrentPage as ShellTestPage;

			// Act: Pop back
			await shell.GoToAsync("..");

			// Assert: ApplyQueryAttributes should have been called on the first details page
			// to restore its parameters
			Assert.True(detailsPage.AppliedQueryAttributes.Count >= 2,
				"ApplyQueryAttributes should be triggered when popping back to a page");
			Assert.Equal("1234", detailsPage.SomeQueryParameter);
		}

		[Fact]
		public async Task ShellSectionChangedSetsQueryAttributesProperty()
		{
			// Arrange: Create Shell with multiple sections in a single ShellItem
			var testPage1 = new ShellTestPage();
			var testPage2 = new ShellTestPage();

			var content1 = new ShellContent { Content = testPage1, Route = "content1" };
			var content2 = new ShellContent { Content = testPage2, Route = "content2" };

			var section1 = new ShellSection { Route = "section1" };
			section1.Items.Add(content1);

			var section2 = new ShellSection { Route = "section2" };
			section2.Items.Add(content2);

			var shellItem = new ShellItem { Route = "item" };
			shellItem.Items.Add(section1);
			shellItem.Items.Add(section2);

			var shell = new TestShell();
			shell.Items.Add(shellItem);

			// Verify initial state
			Assert.False(content2.IsSet(ShellContent.QueryAttributesProperty),
				"QueryAttributesProperty should not be set initially");

			// Act: Change the current section
			shellItem.CurrentItem = section2;
			await Task.Yield();

			// Assert: QueryAttributesProperty should be set
			Assert.True(content2.IsSet(ShellContent.QueryAttributesProperty),
				"QueryAttributesProperty should be set when changing ShellSection");
		}
	}
}
