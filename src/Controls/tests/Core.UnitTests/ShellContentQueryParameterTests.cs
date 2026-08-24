using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ShellContentQueryParameterTests : ShellTestBase
	{
		[Fact]
		public void StaticSelectionDeliversDistinctParametersToSharedPageTemplate()
		{
			var pageTemplate = new DataTemplate(() => new ReminderPage());
			var yesterday = CreateContent("Yesterday", "2021-12-13");
			var today = CreateContent("Today", "2021-12-14");
			var tomorrow = CreateContent("Tomorrow", "2021-12-15");
			var tab = new Tab
			{
				Items =
				{
					yesterday,
					today,
					tomorrow,
				}
			};
			var tabBar = new TabBar
			{
				Items = { tab }
			};
			var shell = new Shell
			{
				Items = { tabBar }
			};

			Assert.Equal("2021-12-13", GetPage(yesterday).Date);

			tab.CurrentItem = today;
			Assert.Equal("2021-12-14", GetPage(today).Date);

			tab.CurrentItem = tomorrow;
			Assert.Equal("2021-12-15", GetPage(tomorrow).Date);

			ShellContent CreateContent(string title, string date)
			{
				var content = new ShellContent
				{
					Title = title,
					ContentTemplate = pageTemplate,
				};
				content.QueryParameters.Add(new ShellContentQueryParameter
				{
					Name = "date",
					Value = date,
				});
				return content;
			}

			static ReminderPage GetPage(ShellContent content) =>
				(ReminderPage)((IShellContentController)content).GetOrCreateContent();
		}

		[Fact]
		public void QueryStringSelectionDeliversDistinctParametersToSharedPageTemplate()
		{
			var pageTemplate = new DataTemplate(() => new ReminderPage());
			var yesterday = CreateContent("Yesterday", "date=2021-12-13");
			var today = CreateContent("Today", "date=2021-12-14");
			var tomorrow = CreateContent("Tomorrow", "date=2021-12-15");
			var (_, tab) = CreateShell(yesterday, today, tomorrow);

			Assert.Equal("2021-12-13", GetPage<ReminderPage>(yesterday).Date);

			tab.CurrentItem = today;
			Assert.Equal("2021-12-14", GetPage<ReminderPage>(today).Date);

			tab.CurrentItem = tomorrow;
			Assert.Equal("2021-12-15", GetPage<ReminderPage>(tomorrow).Date);

			ShellContent CreateContent(string title, string queryString) =>
				new()
				{
					Title = title,
					QueryString = queryString,
					ContentTemplate = pageTemplate,
				};
		}

		[Fact]
		public void QueryStringDeliversParametersViaIQueryAttributable()
		{
			var content = CreateQueryStringContent("value=first&other=second");
			CreateShell(content);

			var page = GetPage<QueryAttributablePage>(content);

			Assert.Equal("first", page.Value);
			Assert.Equal("second", page.LastQuery["other"]);
		}

		[Fact]
		public void QueryStringDecodesQueryProperty()
		{
			var content = CreateQueryStringContent(
				"value=A%2BB",
				() => new QueryPropertyPage());
			CreateShell(content);

			Assert.Equal("A+B", GetPage<QueryPropertyPage>(content).Value);
		}

		[Fact]
		public void QueryStringConvertsUsingInvariantCulture()
		{
			var originalCulture = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
				var content = CreateQueryStringContent(
					"value=3.5",
					() => new NumericQueryPropertyPage());
				CreateShell(content);

				Assert.Equal(3.5, GetPage<NumericQueryPropertyPage>(content).Value);
			}
			finally
			{
				CultureInfo.CurrentCulture = originalCulture;
			}
		}

		[Fact]
		public void ClearingQueryStringResetsNonNullableQueryProperty()
		{
			var content = CreateQueryStringContent(
				"value=42",
				() => new IntegerQueryPropertyPage());
			CreateShell(content);
			var page = GetPage<IntegerQueryPropertyPage>(content);

			Assert.Equal(42, page.Value);

			content.QueryString = null;
			Assert.Equal(0, page.Value);
		}

		[Fact]
		public void QueryStringChangeUpdatesCurrentPageAndClearRemovesValue()
		{
			var content = CreateQueryStringContent("value=first");
			CreateShell(content);
			var page = GetPage<QueryAttributablePage>(content);

			content.QueryString = "value=updated";
			Assert.Equal("updated", page.Value);

			content.QueryString = null;
			Assert.Null(page.Value);
		}

		[Fact]
		public void QueryStringIgnoresEmptyParameterName()
		{
			var content = CreateQueryStringContent("=ignored&value=valid");
			CreateShell(content);

			var page = GetPage<QueryAttributablePage>(content);

			Assert.Equal("valid", page.Value);
			Assert.DoesNotContain("", page.LastQuery);
		}

		[Fact]
		public void BoundQueryStringInheritsBindingContext()
		{
			var viewModel = new TestViewModel { Value = "value=first" };
			var content = CreateQueryStringContent(null);
			content.SetBinding(ShellContent.QueryStringProperty, nameof(TestViewModel.Value));
			var (shell, _) = CreateShell(content);
			shell.BindingContext = viewModel;
			var page = GetPage<QueryAttributablePage>(content);

			Assert.Equal("first", page.Value);

			viewModel.Value = "value=updated";
			Assert.Equal("updated", page.Value);
		}

		[Fact]
		public void StructuredParameterOverridesQueryString()
		{
			var content = CreateQueryStringContent("value=query-string");
			content.QueryParameters.Add(new ShellContentQueryParameter
			{
				Name = "value",
				Value = "structured",
			});
			CreateShell(content);

			Assert.Equal("structured", GetPage<QueryAttributablePage>(content).Value);
		}

		[Fact]
		public async Task NavigationParameterOverridesQueryString()
		{
			var content = CreateQueryStringContent("value=query-string");
			var (shell, _) = CreateShell(content);

			await shell.GoToAsync("//content?value=navigation");

			Assert.Equal("navigation", GetPage<QueryAttributablePage>(content).Value);
		}

		[Fact]
		public void SwitchingBackReappliesParametersToReusedPage()
		{
			var first = CreateContent("first");
			var second = CreateContent("second");
			var (_, tab) = CreateShell(first, second);
			var firstPage = GetPage<QueryAttributablePage>(first);

			tab.CurrentItem = second;
			var secondPage = GetPage<QueryAttributablePage>(second);
			tab.CurrentItem = first;

			Assert.Same(firstPage, GetPage<QueryAttributablePage>(first));
			Assert.Equal(2, firstPage.ApplyCount);
			Assert.Equal("first", firstPage.Value);
			Assert.Equal(1, secondPage.ApplyCount);
			Assert.Equal("second", secondPage.Value);
		}

		[Fact]
		public void ContentPagesRemainLazyUntilSelected()
		{
			var createdPages = 0;
			var first = CreateContent("first", () =>
			{
				createdPages++;
				return new QueryAttributablePage();
			});
			var second = CreateContent("second", () =>
			{
				createdPages++;
				return new QueryAttributablePage();
			});
			var (_, tab) = CreateShell(first, second);

			Assert.Equal(0, createdPages);

			GetPage<QueryAttributablePage>(first);
			Assert.Equal(1, createdPages);

			tab.CurrentItem = second;
			GetPage<QueryAttributablePage>(second);
			Assert.Equal(2, createdPages);
			Assert.Equal("second", GetPage<QueryAttributablePage>(second).Value);
		}

		[Fact]
		public void ParameterChangesUpdateCurrentCreatedPage()
		{
			var parameter = new ShellContentQueryParameter { Name = "value", Value = "first" };
			var content = CreateContent(parameter);
			CreateShell(content);
			var page = GetPage<QueryAttributablePage>(content);

			parameter.Value = "updated";

			Assert.Equal("updated", page.Value);
			Assert.Equal(2, page.ApplyCount);
		}

		[Fact]
		public void AddingParameterUpdatesCurrentCreatedPage()
		{
			var content = new ShellContent
			{
				Route = "content",
				ContentTemplate = new DataTemplate(() => new QueryAttributablePage()),
			};
			CreateShell(content);
			var page = GetPage<QueryAttributablePage>(content);

			content.QueryParameters.Add(new ShellContentQueryParameter { Name = "value", Value = "added" });

			Assert.Equal("added", page.Value);
			Assert.Equal(1, page.ApplyCount);
		}

		[Fact]
		public void InactiveParameterChangesApplyOnNextSelection()
		{
			var first = CreateContent("first");
			var parameter = new ShellContentQueryParameter { Name = "value", Value = "second" };
			var second = CreateContent(parameter);
			var (_, tab) = CreateShell(first, second);
			var secondPage = GetPage<QueryAttributablePage>(second);

			parameter.Value = "updated";

			Assert.Equal(0, secondPage.ApplyCount);
			tab.CurrentItem = second;
			Assert.Equal("updated", secondPage.Value);
			Assert.Equal(1, secondPage.ApplyCount);
		}

		[Fact]
		public void InactiveParentItemParameterChangesApplyOnNextSelection()
		{
			var firstContent = CreateContent("first");
			firstContent.Route = "first-content";
			var firstSection = new Tab { Route = "first-tab", Items = { firstContent } };
			var firstItem = new FlyoutItem { Route = "first-item", Items = { firstSection } };
			var parameter = new ShellContentQueryParameter { Name = "value", Value = "second" };
			var secondContent = CreateContent(parameter);
			secondContent.Route = "second-content";
			var secondSection = new Tab { Route = "second-tab", Items = { secondContent } };
			var secondItem = new FlyoutItem { Route = "second-item", Items = { secondSection } };
			var shell = new Shell { Items = { firstItem, secondItem } };

			parameter.Value = "updated";
			shell.CurrentItem = secondItem;

			Assert.Equal("updated", GetPage<QueryAttributablePage>(secondContent).Value);
		}

		[Fact]
		public void BoundParameterInheritsBindingContextAndUpdatesCurrentPage()
		{
			var viewModel = new TestViewModel { Value = "first" };
			var parameter = new ShellContentQueryParameter { Name = "value" };
			parameter.SetBinding(ShellContentQueryParameter.ValueProperty, nameof(TestViewModel.Value));
			var content = CreateContent(parameter);
			content.BindingContext = viewModel;
			CreateShell(content);
			var page = GetPage<QueryAttributablePage>(content);

			Assert.Equal("first", page.Value);

			viewModel.Value = "updated";

			Assert.Equal("updated", page.Value);
		}

		[Fact]
		public void BoundParameterInheritsBindingContextFromParentShell()
		{
			var viewModel = new TestViewModel { Value = "first" };
			var parameter = new ShellContentQueryParameter { Name = "value" };
			parameter.SetBinding(ShellContentQueryParameter.ValueProperty, nameof(TestViewModel.Value));
			var content = CreateContent(parameter);
			var (shell, _) = CreateShell(content);

			shell.BindingContext = viewModel;
			var page = GetPage<QueryAttributablePage>(content);

			Assert.Equal("first", page.Value);

			viewModel.Value = "updated";
			Assert.Equal("updated", page.Value);
		}

		[Fact]
		public void ReplacingParameterUnsubscribesOldParameter()
		{
			var oldParameter = new ShellContentQueryParameter { Name = "value", Value = "first" };
			var content = CreateContent(oldParameter);
			CreateShell(content);
			var page = GetPage<QueryAttributablePage>(content);
			var newParameter = new ShellContentQueryParameter { Name = "value", Value = "replacement" };

			content.QueryParameters[0] = newParameter;
			int applyCountAfterReplacement = page.ApplyCount;
			oldParameter.Value = "ignored";

			Assert.Equal("replacement", page.Value);
			Assert.Equal(applyCountAfterReplacement, page.ApplyCount);
		}

		[Fact]
		public void DuplicateNamesUseLastValueAndRevealPreviousValueOnRemoval()
		{
			var first = new ShellContentQueryParameter { Name = "value", Value = "first" };
			var second = new ShellContentQueryParameter { Name = "value", Value = "second" };
			var content = CreateContent(first, second);
			CreateShell(content);
			var page = GetPage<QueryAttributablePage>(content);

			Assert.Equal("second", page.Value);

			content.QueryParameters.Remove(second);

			Assert.Equal("first", page.Value);
		}

		[Fact]
		public void NullValueIsDeliveredAndRemovalClearsQueryProperty()
		{
			var parameter = new ShellContentQueryParameter { Name = "value", Value = "first" };
			var content = new ShellContent
			{
				Route = "content",
				ContentTemplate = new DataTemplate(() => new QueryPropertyPage()),
			};
			content.QueryParameters.Add(parameter);
			CreateShell(content);
			var page = GetPage<QueryPropertyPage>(content);

			parameter.Value = null;
			Assert.Null(page.Value);

			parameter.Value = "second";
			Assert.Equal("second", page.Value);

			content.QueryParameters.Remove(parameter);
			Assert.Null(page.Value);
		}

		[Fact]
		public void RenamingParameterClearsOldQueryPropertyAndSetsNewQueryProperty()
		{
			var parameter = new ShellContentQueryParameter { Name = "first", Value = "value" };
			var content = new ShellContent
			{
				Route = "content",
				ContentTemplate = new DataTemplate(() => new TwoQueryPropertyPage()),
			};
			content.QueryParameters.Add(parameter);
			CreateShell(content);
			var page = GetPage<TwoQueryPropertyPage>(content);

			Assert.Equal("value", page.First);
			Assert.Null(page.Second);

			parameter.Name = "second";

			Assert.Null(page.First);
			Assert.Equal("value", page.Second);
		}

		[Fact]
		public void QueryPropertyPreservesLiteralStaticString()
		{
			var content = new ShellContent
			{
				Route = "content",
				ContentTemplate = new DataTemplate(() => new QueryPropertyPage()),
			};
			content.QueryParameters.Add(new ShellContentQueryParameter
			{
				Name = "value",
				Value = "A+B%2B",
			});
			CreateShell(content);

			Assert.Equal("A+B%2B", GetPage<QueryPropertyPage>(content).Value);
		}

		[Fact]
		public void QueryPropertyConvertsStaticValueToString()
		{
			var content = new ShellContent
			{
				Route = "content",
				ContentTemplate = new DataTemplate(() => new QueryPropertyPage()),
			};
			content.QueryParameters.Add(new ShellContentQueryParameter
			{
				Name = "value",
				Value = 42,
			});
			CreateShell(content);

			Assert.Equal("42", GetPage<QueryPropertyPage>(content).Value);
		}

		[Fact]
		public void QueryPropertyConvertsStaticStringUsingInvariantCulture()
		{
			var originalCulture = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
				var content = new ShellContent
				{
					Route = "content",
					ContentTemplate = new DataTemplate(() => new NumericQueryPropertyPage()),
				};
				content.QueryParameters.Add(new ShellContentQueryParameter
				{
					Name = "value",
					Value = "3.5",
				});
				CreateShell(content);

				Assert.Equal(3.5, GetPage<NumericQueryPropertyPage>(content).Value);
			}
			finally
			{
				CultureInfo.CurrentCulture = originalCulture;
			}
		}

		[Fact]
		public async Task QueryPropertyPreservesLiteralStaticStringAfterRelativePop()
		{
			Routing.RegisterRoute("details", typeof(ContentPage));
			var content = new ShellContent
			{
				Route = "content",
				ContentTemplate = new DataTemplate(() => new QueryPropertyPage()),
			};
			content.QueryParameters.Add(new ShellContentQueryParameter
			{
				Name = "value",
				Value = "A+B%2B",
			});
			var (shell, _) = CreateShell(content);
			var page = GetPage<QueryPropertyPage>(content);

			await shell.GoToAsync("details");
			await shell.GoToAsync("..");

			Assert.Equal("A+B%2B", page.Value);
		}

		[Fact]
		public async Task QueryStringRemainsSingleDecodedAndUpdatesAfterRelativePop()
		{
			Routing.RegisterRoute("details", typeof(ContentPage));
			var content = CreateQueryStringContent(
				"value=A%2BB",
				() => new QueryPropertyPage());
			var (shell, _) = CreateShell(content);
			var page = GetPage<QueryPropertyPage>(content);

			await shell.GoToAsync("details");
			await shell.GoToAsync("..");

			Assert.Equal("A+B", page.Value);

			content.QueryString = "value=updated";
			Assert.Equal("updated", page.Value);
		}

		[Fact]
		public async Task RelativePopDoesNotReapplyStaleNavigationParametersToRootContent()
		{
			Routing.RegisterRoute("details", typeof(ContentPage));
			var content = CreateQueryStringContent("value=query-string");
			var (shell, _) = CreateShell(content);
			var page = GetPage<QueryAttributablePage>(content);

			await shell.GoToAsync("//content/details?stale=previous");
			Assert.DoesNotContain("stale", page.LastQuery);
			await shell.GoToAsync("..");

			Assert.Equal("query-string", page.Value);
			Assert.DoesNotContain("stale", page.LastQuery);
		}

		[Fact]
		public async Task StaticParameterCanUpdateAndBeRemovedAfterRelativePop()
		{
			Routing.RegisterRoute("details", typeof(ContentPage));
			var parameter = new ShellContentQueryParameter
			{
				Name = "value",
				Value = "first",
			};
			var content = new ShellContent
			{
				Route = "content",
				ContentTemplate = new DataTemplate(() => new QueryPropertyPage()),
			};
			content.QueryParameters.Add(parameter);
			var (shell, _) = CreateShell(content);
			var page = GetPage<QueryPropertyPage>(content);

			await shell.GoToAsync("details");
			await shell.GoToAsync("..");
			parameter.Value = "updated";

			Assert.Equal("updated", page.Value);

			content.QueryParameters.Remove(parameter);
			Assert.Null(page.Value);
		}

		[Fact]
		public void EmptyAndNullNamesAreIgnored()
		{
			var content = CreateContent(
				new ShellContentQueryParameter { Name = null, Value = "null" },
				new ShellContentQueryParameter { Name = "", Value = "empty" },
				new ShellContentQueryParameter { Name = "value", Value = "valid" });
			CreateShell(content);

			var page = GetPage<QueryAttributablePage>(content);

			Assert.Equal("valid", page.Value);
			Assert.Single(page.LastQuery);
		}

		[Fact]
		public async Task NavigationParametersOverrideContentParameters()
		{
			var content = CreateContent("static");
			var (shell, _) = CreateShell(content);

			await shell.GoToAsync("//content?value=navigation");

			Assert.Equal("navigation", GetPage<QueryAttributablePage>(content).Value);
		}

		[Fact]
		public async Task ParameterChangeDoesNotReplaceCurrentNavigationOverride()
		{
			var parameter = new ShellContentQueryParameter { Name = "value", Value = "static" };
			var content = CreateContent(parameter);
			var (shell, _) = CreateShell(content);

			await shell.GoToAsync("//content?value=navigation");
			var page = GetPage<QueryAttributablePage>(content);
			parameter.Value = "updated";

			Assert.Equal("navigation", page.Value);
		}

		[Fact]
		public async Task DirectSelectionReclaimsShellNavigationQueryParameterKey()
		{
			var first = CreateContent("first");
			first.Route = "first";
			var second = CreateContent("static");
			second.Route = "second";
			var (shell, tab) = CreateShell(first, second);
			var navigationParameters = new ShellNavigationQueryParameters
			{
				["value"] = "navigation",
			};

			await shell.GoToAsync(new ShellNavigationState("//second"), navigationParameters);
			var page = GetPage<QueryAttributablePage>(second);
			tab.CurrentItem = first;
			tab.CurrentItem = second;

			Assert.Equal("static", page.Value);
		}

		[Fact]
		public async Task ParameterRemovalDoesNotRemoveCurrentNavigationOverride()
		{
			var parameter = new ShellContentQueryParameter { Name = "value", Value = "static" };
			var content = CreateContent(parameter);
			var (shell, _) = CreateShell(content);

			await shell.GoToAsync("//content?value=navigation");
			var page = GetPage<QueryAttributablePage>(content);
			content.QueryParameters.Remove(parameter);

			Assert.Equal("navigation", page.Value);
		}

		[Fact]
		public async Task NavigationThatChangesContentAppliesQueryOnce()
		{
			var first = CreateContent("first");
			first.Route = "first";
			var second = CreateContent("static");
			second.Route = "second";
			var (shell, _) = CreateShell(first, second);

			await shell.GoToAsync("//second?value=navigation");
			var page = GetPage<QueryAttributablePage>(second);

			Assert.Equal("navigation", page.Value);
			Assert.Equal(1, page.ApplyCount);
		}

		[Fact]
		public async Task DeepLinkAppliesStaticParametersToSelectedRootContent()
		{
			Routing.RegisterRoute("details", typeof(ContentPage));
			var first = CreateContent("first");
			first.Route = "first";
			var second = CreateContent("static");
			second.Route = "second";
			var (shell, _) = CreateShell(first, second);

			await shell.GoToAsync("//second/details");

			var page = GetPage<QueryAttributablePage>(second);
			Assert.Equal("static", page.Value);
			Assert.Equal(1, page.ApplyCount);
		}

		[Fact]
		public async Task DeepLinkAppliesQueryStringToSelectedRootContent()
		{
			Routing.RegisterRoute("details", typeof(ContentPage));
			var first = CreateQueryStringContent("value=first");
			first.Route = "first";
			var second = CreateQueryStringContent("value=query-string");
			second.Route = "second";
			var (shell, _) = CreateShell(first, second);

			await shell.GoToAsync("//second/details");

			var page = GetPage<QueryAttributablePage>(second);
			Assert.Equal("query-string", page.Value);
			Assert.Equal(1, page.ApplyCount);
		}

		[Fact]
		public async Task DeepLinkDoesNotApplyDetailNavigationParametersToRootContent()
		{
			Routing.RegisterRoute("details", typeof(QueryAttributablePage));
			var first = CreateContent("first");
			first.Route = "first";
			var second = CreateContent("static");
			second.Route = "second";
			var (shell, _) = CreateShell(first, second);
			var navigationParameters = new ShellNavigationQueryParameters
			{
				["detailId"] = 42,
			};

			await shell.GoToAsync(new ShellNavigationState("//second/details"), navigationParameters);

			var rootPage = GetPage<QueryAttributablePage>(second);
			Assert.Equal("static", rootPage.Value);
			Assert.DoesNotContain("detailId", rootPage.LastQuery);

			var detailPage = Assert.IsType<QueryAttributablePage>(shell.CurrentPage);
			Assert.Equal(42, detailPage.LastQuery["detailId"]);
		}

		[Fact]
		public async Task NavigationThatChangesShellItemAppliesQueryOnce()
		{
			var firstContent = CreateContent("first");
			firstContent.Route = "first-content";
			var firstSection = new Tab { Route = "first-tab", Items = { firstContent } };
			var firstItem = new FlyoutItem { Route = "first-item", Items = { firstSection } };
			var secondContent = CreateContent("static");
			secondContent.Route = "second-content";
			var secondSection = new Tab { Route = "second-tab", Items = { secondContent } };
			var secondItem = new FlyoutItem { Route = "second-item", Items = { secondSection } };
			var shell = new Shell { Items = { firstItem, secondItem } };

			await shell.GoToAsync("//second-item/second-tab/second-content?value=navigation");
			var page = GetPage<QueryAttributablePage>(secondContent);

			Assert.Equal("navigation", page.Value);
			Assert.Equal(1, page.ApplyCount);
		}

		[Fact]
		public async Task DirectSelectionAfterNavigationRestoresContentParameter()
		{
			var first = CreateContent("first");
			first.Route = "first";
			var second = CreateContent("static");
			second.Route = "second";
			var (shell, tab) = CreateShell(first, second);

			await shell.GoToAsync("//second?value=navigation");
			var page = GetPage<QueryAttributablePage>(second);
			tab.CurrentItem = first;
			tab.CurrentItem = second;

			Assert.Equal("static", page.Value);
			Assert.Equal(2, page.ApplyCount);
		}

		[Fact]
		public async Task ContentParametersApplyWhenNavigationDoesNotSupplySameKey()
		{
			var content = CreateContent("static");
			content.QueryParameters.Add(new ShellContentQueryParameter { Name = "other", Value = "content" });
			var (shell, _) = CreateShell(content);

			await shell.GoToAsync("//content?navigation=value");

			var query = GetPage<QueryAttributablePage>(content).LastQuery;
			Assert.Equal("static", query["value"]);
			Assert.Equal("content", query["other"]);
			Assert.Equal("value", query["navigation"]);
		}

		[Fact]
		public async Task DirectSelectionRetainsUnrelatedNavigationParameter()
		{
			var first = CreateContent("first");
			first.Route = "first";
			var second = CreateContent("static");
			second.Route = "second";
			second.QueryParameters.Add(new ShellContentQueryParameter { Name = "other", Value = "content" });
			var (shell, tab) = CreateShell(first, second);

			await shell.GoToAsync("//second?navigation=value");
			var page = GetPage<QueryAttributablePage>(second);
			tab.CurrentItem = first;
			tab.CurrentItem = second;

			Assert.Equal("static", page.LastQuery["value"]);
			Assert.Equal("content", page.LastQuery["other"]);
			Assert.Equal("value", page.LastQuery["navigation"]);
		}

		[Fact]
		public async Task NavigationWithoutParameterRestoresContentParameter()
		{
			var content = CreateContent("static");
			var (shell, _) = CreateShell(content);

			await shell.GoToAsync("//content?value=navigation");
			await shell.GoToAsync("//content");

			Assert.Equal("static", GetPage<QueryAttributablePage>(content).Value);
		}

		[Fact]
		public void QueryExceptionsAreNotSwallowed()
		{
			var content = new ShellContent
			{
				ContentTemplate = new DataTemplate(() => new ThrowingQueryPage()),
			};
			content.QueryParameters.Add(new ShellContentQueryParameter { Name = "value", Value = "test" });
			CreateShell(content);

			Assert.Throws<InvalidOperationException>(() =>
				((IShellContentController)content).GetOrCreateContent());
		}

		[Fact]
		public void InactiveSectionCurrentItemDoesNotApplyUntilParentIsSelected()
		{
			var first = CreateContent("first");
			first.Route = "first";
			var second = CreateContent("second");
			second.Route = "second";
			var inactiveSection = new Tab { Route = "inactive", Items = { first, second } };
			var activeContent = CreateContent("active");
			activeContent.Route = "active-content";
			var activeSection = new Tab { Route = "active", Items = { activeContent } };
			var item = new TabBar { Items = { activeSection, inactiveSection } };
			var shell = new Shell { Items = { item } };
			var secondPage = GetPage<QueryAttributablePage>(second);

			inactiveSection.CurrentItem = second;

			Assert.Equal(0, secondPage.ApplyCount);
			item.CurrentItem = inactiveSection;
			Assert.Equal("second", secondPage.Value);
			Assert.Equal(1, secondPage.ApplyCount);
		}

		[Fact]
		public async Task ParameterChangeWhileModalIsVisibleUpdatesSelectedContent()
		{
			var parameter = new ShellContentQueryParameter { Name = "value", Value = "first" };
			var content = CreateContent(parameter);
			var (shell, _) = CreateShell(content);
			var page = GetPage<QueryAttributablePage>(content);

			await shell.Navigation.PushModalAsync(new ContentPage(), false);
			parameter.Value = "updated";

			Assert.Equal("updated", page.Value);

			await shell.Navigation.PopModalAsync(false);
			Assert.Equal("updated", page.Value);
		}

		static ShellContent CreateContent(string value, Func<Page> factory = null) =>
			CreateContent(
				new ShellContentQueryParameter { Name = "value", Value = value },
				factory);

		static ShellContent CreateContent(params ShellContentQueryParameter[] parameters) =>
			CreateContent(parameters, null);

		static ShellContent CreateContent(ShellContentQueryParameter parameter) =>
			CreateContent(new[] { parameter }, null);

		static ShellContent CreateContent(
			ShellContentQueryParameter parameter,
			Func<Page> factory) =>
			CreateContent(new[] { parameter }, factory);

		static ShellContent CreateContent(
			ShellContentQueryParameter[] parameters,
			Func<Page> factory)
		{
			var content = new ShellContent
			{
				Route = "content",
				ContentTemplate = new DataTemplate(factory ?? (() => new QueryAttributablePage())),
			};

			foreach (var parameter in parameters)
				content.QueryParameters.Add(parameter);

			return content;
		}

		static ShellContent CreateQueryStringContent(string queryString, Func<Page> factory = null) =>
			new()
			{
				Route = "content",
				QueryString = queryString,
				ContentTemplate = new DataTemplate(factory ?? (() => new QueryAttributablePage())),
			};

		static (Shell Shell, Tab Tab) CreateShell(params ShellContent[] contents)
		{
			var tab = new Tab { Route = "tab" };
			foreach (var content in contents)
				tab.Items.Add(content);

			var tabBar = new TabBar { Route = "root", Items = { tab } };
			var shell = new Shell { Items = { tabBar } };
			return (shell, tab);
		}

		static T GetPage<T>(ShellContent content)
			where T : Page =>
			(T)((IShellContentController)content).GetOrCreateContent();

		sealed class QueryAttributablePage : ContentPage, IQueryAttributable
		{
			public int ApplyCount { get; private set; }
			public IDictionary<string, object> LastQuery { get; private set; }
			public string Value { get; private set; }

			public void ApplyQueryAttributes(IDictionary<string, object> query)
			{
				ApplyCount++;
				LastQuery = new Dictionary<string, object>(query);
				Value = query.TryGetValue("value", out var value) ? (string)value : null;
			}
		}

		[QueryProperty(nameof(Value), "value")]
		sealed class QueryPropertyPage : ContentPage
		{
			public string Value { get; set; }
		}

		[QueryProperty(nameof(Value), "value")]
		sealed class NumericQueryPropertyPage : ContentPage
		{
			public double Value { get; set; }
		}

		[QueryProperty(nameof(Value), "value")]
		sealed class IntegerQueryPropertyPage : ContentPage
		{
			public int Value { get; set; }
		}

		[QueryProperty(nameof(First), "first")]
		[QueryProperty(nameof(Second), "second")]
		sealed class TwoQueryPropertyPage : ContentPage
		{
			public string First { get; set; }
			public string Second { get; set; }
		}

		sealed class ThrowingQueryPage : ContentPage, IQueryAttributable
		{
			public void ApplyQueryAttributes(IDictionary<string, object> query) =>
				throw new InvalidOperationException("Query failure");
		}

		sealed class TestViewModel : INotifyPropertyChanged
		{
			string _value;

			public event PropertyChangedEventHandler PropertyChanged;

			public string Value
			{
				get => _value;
				set
				{
					if (_value == value)
						return;

					_value = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
				}
			}
		}

		sealed class ReminderPage : ContentPage, IQueryAttributable
		{
			public string Date { get; private set; }

			public void ApplyQueryAttributes(IDictionary<string, object> query)
			{
				Date = (string)query["date"];
			}
		}
	}
}
