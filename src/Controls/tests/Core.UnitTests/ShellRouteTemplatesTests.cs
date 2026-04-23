using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ShellRouteTemplatesTests : ShellTestBase
	{
		[QueryProperty(nameof(Sku), "sku")]
		public class ProductPage : ContentPage
		{
			public string Sku { get; set; }
		}

		[QueryProperty(nameof(Sku), "sku")]
		[QueryProperty(nameof(ReviewId), "reviewId")]
		public class ReviewPage : ContentPage
		{
			public string Sku { get; set; }
			public string ReviewId { get; set; }
		}

		[Fact]
		public void RegisterRouteTemplate_DetectedAsTemplate()
		{
			Routing.RegisterRoute("product/{sku}", typeof(ProductPage));

			Assert.True(Routing.IsTemplateRoute("product/{sku}"));
			Assert.False(Routing.IsTemplateRoute("product/seed-tomato"));

			Assert.True(Routing.TryGetRouteTemplate("product/{sku}", out var template));
			Assert.NotNull(template);
			Assert.True(template.HasParameters);
		}

		[Fact]
		public async Task SinglePathParameter_DeliveredViaQueryProperty()
		{
			Routing.RegisterRoute("product/{sku}", typeof(ProductPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			await shell.GoToAsync("//main/products/product/seed-tomato");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as ProductPage;
			Assert.NotNull(page);
			Assert.Equal("seed-tomato", page.Sku);
		}

		[Fact]
		public async Task MultiSegmentTemplate_ChildInheritsParentPathParameter()
		{
			// Register as separate routes — Shell iteratively matches each
			// segment via ExpandOutGlobalRoutes.
			Routing.RegisterRoute("product/{sku}", typeof(ProductPage));
			Routing.RegisterRoute("review", typeof(ReviewPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			await shell.GoToAsync("//main/products/product/seed-tomato/review");

			var stack = shell.Navigation.NavigationStack;
			ReviewPage review = null;
			foreach (var p in stack)
			{
				if (p is ReviewPage rp)
					review = rp;
			}
			Assert.NotNull(review);
			// The last page in the navigation receives all unprefixed params,
			// including the path parameter captured from the parent template.
			Assert.Equal("seed-tomato", review.Sku);
		}

		[Fact]
		public async Task LiteralRouteWinsOverTemplateRoute()
		{
			Routing.RegisterRoute("product/{sku}", typeof(ProductPage));
			Routing.RegisterRoute("product/special", typeof(ReviewPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			await shell.GoToAsync("//main/products/product/special");

			var top = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1];
			Assert.IsType<ReviewPage>(top);
		}

		[Fact]
		public async Task PathParameterOverridesQueryStringWithSameName()
		{
			Routing.RegisterRoute("product/{sku}", typeof(ProductPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			// Path provides "seed-tomato"; query provides "ignored". Path must win.
			await shell.GoToAsync("//main/products/product/seed-tomato?sku=ignored");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as ProductPage;
			Assert.NotNull(page);
			Assert.Equal("seed-tomato", page.Sku);
		}

		[Fact]
		public async Task PathParameter_MixedWithUnrelatedQueryString()
		{
			Routing.RegisterRoute("product/{sku}", typeof(ProductPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			await shell.GoToAsync("//main/products/product/seed-tomato?source=catalog");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as ProductPage;
			Assert.NotNull(page);
			Assert.Equal("seed-tomato", page.Sku);
		}

		[Fact]
		public void LiteralRouteUnchanged_RegistersAsLiteral()
		{
			Routing.RegisterRoute("plain-product", typeof(ProductPage));

			Assert.False(Routing.IsTemplateRoute("plain-product"));
			Assert.False(Routing.TryGetRouteTemplate("plain-product", out _));
		}

		[Fact]
		public void Routing_Clear_AlsoClearsTemplates()
		{
			Routing.RegisterRoute("product/{sku}", typeof(ProductPage));
			Assert.True(Routing.IsTemplateRoute("product/{sku}"));

			Routing.Clear();

			Assert.False(Routing.IsTemplateRoute("product/{sku}"));
		}

		[Fact]
		public async Task CurrentStateLocation_ShowsResolvedValues()
		{
			Routing.RegisterRoute("product/{sku}", typeof(ProductPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			await shell.GoToAsync("//main/products/product/seed-tomato");

			var location = shell.CurrentState.Location.ToString();
			Assert.Contains("seed-tomato", location, StringComparison.Ordinal);
			Assert.DoesNotContain("{sku}", location, StringComparison.Ordinal);
		}

		[Fact]
		public async Task RelativeNavigation_WithTemplateRoute()
		{
			Routing.RegisterRoute("product/{sku}", typeof(ProductPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			// Known v1 limitation: relative navigation with template routes
			// goes through SearchForGlobalRoutes which doesn't yet recognize
			// template segments in the relative URI. Use absolute URIs for now.
			// This test documents the limitation — when fixed, change to Assert.Equal.
			await Assert.ThrowsAsync<ArgumentException>(() =>
				shell.GoToAsync("product/seed-tomato"));
		}

		[Fact]
		public async Task UrlEncodedPathParameter_IsDecoded()
		{
			Routing.RegisterRoute("product/{sku}", typeof(ProductPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			await shell.GoToAsync("//main/products/product/hello%20world");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as ProductPage;
			Assert.NotNull(page);
			Assert.Equal("hello world", page.Sku);
		}

		[Fact]
		public async Task SecondNavigation_DifferentValue_DeliveredCorrectly()
		{
			Routing.RegisterRoute("product/{sku}", typeof(ProductPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			await shell.GoToAsync("//main/products/product/apple");
			var page1 = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as ProductPage;
			Assert.Equal("apple", page1.Sku);

			await shell.GoToAsync("//main/products/product/banana");
			var page2 = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as ProductPage;
			Assert.Equal("banana", page2.Sku);
		}

		[Fact]
		public void RegisterRoute_RejectsOptionalTemplateSyntax()
		{
			Assert.Throws<ArgumentException>(() =>
				Routing.RegisterRoute("product/{sku?}", typeof(ProductPage)));
		}

		[Fact]
		public void RegisterRoute_RejectsCatchAllTemplateSyntax()
		{
			Assert.Throws<ArgumentException>(() =>
				Routing.RegisterRoute("files/{*rest}", typeof(ProductPage)));
		}

		[Fact]
		public void RegisterRoute_RejectsDuplicateParameters()
		{
			Assert.Throws<ArgumentException>(() =>
				Routing.RegisterRoute("product/{id}/{id}", typeof(ProductPage)));
		}

		public class QueryAttributablePage : ContentPage, IQueryAttributable
		{
			public IDictionary<string, object> ReceivedQuery { get; private set; }

			public void ApplyQueryAttributes(IDictionary<string, object> query)
			{
				ReceivedQuery = new Dictionary<string, object>(query);
			}
		}

		[Fact]
		public async Task PathParameter_DeliveredViaIQueryAttributable()
		{
			Routing.RegisterRoute("product/{sku}", typeof(QueryAttributablePage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			await shell.GoToAsync("//main/products/product/seed-tomato");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as QueryAttributablePage;
			Assert.NotNull(page);
			Assert.NotNull(page.ReceivedQuery);
			Assert.True(page.ReceivedQuery.ContainsKey("sku"));
			Assert.Equal("seed-tomato", page.ReceivedQuery["sku"]);
		}

		[Fact]
		public async Task TemplateOnlyRoute_AmbiguousRouteIsDocumentedLimitation()
		{
			Routing.RegisterRoute("{category}", typeof(ProductPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			// Known v1 limitation: an all-template route like "{category}" can
			// match the same URI segment in both passes of the two-pass algorithm,
			// producing ambiguous route errors. Template routes should include at
			// least one literal segment prefix (e.g. "browse/{category}").
			await Assert.ThrowsAsync<ArgumentException>(() =>
				shell.GoToAsync("//main/products/vegetables"));
		}
	}
}
