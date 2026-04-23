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
		public void RegisterRoute_AcceptsOptionalTemplateSyntax()
		{
			Routing.RegisterRoute("product/{sku?}", typeof(ProductPage));
			Assert.True(Routing.IsTemplateRoute("product/{sku?}"));
			Assert.True(Routing.TryGetRouteTemplate("product/{sku?}", out var template));
			Assert.True(template.Segments[1].IsOptional);
		}

		[Fact]
		public void RegisterRoute_AcceptsCatchAllTemplateSyntax()
		{
			Routing.RegisterRoute("files/{*rest}", typeof(ProductPage));
			Assert.True(Routing.IsTemplateRoute("files/{*rest}"));
			Assert.True(Routing.TryGetRouteTemplate("files/{*rest}", out var template));
			Assert.True(template.Segments[1].IsCatchAll);
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

			await Assert.ThrowsAsync<ArgumentException>(() =>
				shell.GoToAsync("//main/products/vegetables"));
		}

		// ===== Optional Parameters =====

		[Fact]
		public async Task OptionalParameter_PresentInUri_DeliveredToPage()
		{
			Routing.RegisterRoute("product/{sku?}", typeof(ProductPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			await shell.GoToAsync("//main/products/product/seed-tomato");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as ProductPage;
			Assert.NotNull(page);
			Assert.Equal("seed-tomato", page.Sku);
		}

		[Fact]
		public async Task OptionalParameter_AbsentInUri_NavigationSucceeds()
		{
			Routing.RegisterRoute("product/{sku?}", typeof(ProductPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			// Navigate to "product" without providing the optional sku
			await shell.GoToAsync("//main/products/product");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as ProductPage;
			Assert.NotNull(page);
			Assert.Null(page.Sku); // No value was provided
		}

		// ===== Default Values =====

		[QueryProperty(nameof(Stars), "stars")]
		public class DefaultStarsPage : ContentPage
		{
			public string Stars { get; set; }
		}

		[Fact]
		public async Task DefaultValue_AbsentInUri_DefaultDelivered()
		{
			Routing.RegisterRoute("review/{stars=5}", typeof(DefaultStarsPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			// Navigate without the stars segment — should get default "5"
			await shell.GoToAsync("//main/products/review");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as DefaultStarsPage;
			Assert.NotNull(page);
			Assert.Equal("5", page.Stars);
		}

		[Fact]
		public async Task DefaultValue_PresentInUri_OverridesDefault()
		{
			Routing.RegisterRoute("review/{stars=5}", typeof(DefaultStarsPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			await shell.GoToAsync("//main/products/review/3");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as DefaultStarsPage;
			Assert.NotNull(page);
			Assert.Equal("3", page.Stars);
		}

		// ===== Catch-All Parameters =====

		[QueryProperty(nameof(FilePath), "path")]
		public class FileBrowserPage : ContentPage
		{
			public string FilePath { get; set; }
		}

		[Fact]
		public async Task CatchAll_CapturesAllRemainingSegments()
		{
			Routing.RegisterRoute("files/{*path}", typeof(FileBrowserPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "browse"));

			await shell.GoToAsync("//main/browse/files/docs/reports/2024/summary.pdf");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as FileBrowserPage;
			Assert.NotNull(page);
			Assert.Equal("docs/reports/2024/summary.pdf", page.FilePath);
		}

		[Fact]
		public async Task CatchAll_SingleSegment()
		{
			Routing.RegisterRoute("files/{*path}", typeof(FileBrowserPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "browse"));

			await shell.GoToAsync("//main/browse/files/readme.txt");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as FileBrowserPage;
			Assert.NotNull(page);
			Assert.Equal("readme.txt", page.FilePath);
		}

		[Fact]
		public void CatchAll_MustBeLastSegment()
		{
			Assert.Throws<ArgumentException>(() =>
				Routing.RegisterRoute("{*path}/suffix", typeof(FileBrowserPage)));
		}

		// ===== Constraints =====

		[QueryProperty(nameof(OrderId), "id")]
		public class OrderDetailPage : ContentPage
		{
			public string OrderId { get; set; }
		}

		[Fact]
		public async Task Constraint_Int_MatchesNumericValue()
		{
			Routing.RegisterRoute("order/{id:int}", typeof(OrderDetailPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "orders"));

			await shell.GoToAsync("//main/orders/order/42");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as OrderDetailPage;
			Assert.NotNull(page);
			Assert.Equal("42", page.OrderId);
		}

		[Fact]
		public async Task Constraint_Int_RejectsNonNumeric()
		{
			Routing.RegisterRoute("order/{id:int}", typeof(OrderDetailPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "orders"));

			// "abc" doesn't satisfy :int, so navigation should fail
			await Assert.ThrowsAsync<ArgumentException>(() =>
				shell.GoToAsync("//main/orders/order/abc"));
		}

		[Fact]
		public async Task Constraint_Alpha_MatchesAlphaOnly()
		{
			Routing.RegisterRoute("category/{name:alpha}", typeof(ProductPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "browse"));

			await shell.GoToAsync("//main/browse/category/vegetables");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as ProductPage;
			Assert.NotNull(page);
		}

		[Fact]
		public async Task Constraint_Alpha_RejectsNumeric()
		{
			Routing.RegisterRoute("category/{name:alpha}", typeof(ProductPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "browse"));

			await Assert.ThrowsAsync<ArgumentException>(() =>
				shell.GoToAsync("//main/browse/category/123"));
		}

		[Fact]
		public async Task Constraint_Guid_MatchesValidGuid()
		{
			Routing.RegisterRoute("item/{id:guid}", typeof(OrderDetailPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "items"));

			await shell.GoToAsync("//main/items/item/550e8400-e29b-41d4-a716-446655440000");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as OrderDetailPage;
			Assert.NotNull(page);
			Assert.Equal("550e8400-e29b-41d4-a716-446655440000", page.OrderId);
		}

		[Fact]
		public void Constraint_UnknownType_RejectedAtRegistration()
		{
			Assert.Throws<ArgumentException>(() =>
				Routing.RegisterRoute("order/{id:regex}", typeof(OrderDetailPage)));
		}

		// ===== Mixed Segments =====

		[Fact]
		public async Task MixedSegment_PrefixAndParameter()
		{
			Routing.RegisterRoute("product-{sku}", typeof(ProductPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			await shell.GoToAsync("//main/products/product-seed-tomato");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as ProductPage;
			Assert.NotNull(page);
			Assert.Equal("seed-tomato", page.Sku);
		}

		[Fact]
		public async Task MixedSegment_SuffixAndParameter()
		{
			Routing.RegisterRoute("{name}.html", typeof(ProductPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "pages"));

			await shell.GoToAsync("//main/pages/about.html");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as ProductPage;
			Assert.NotNull(page);
		}

		[Fact]
		public async Task MixedSegment_PrefixSuffixAndParameter()
		{
			Routing.RegisterRoute("item_{id}_detail", typeof(OrderDetailPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "items"));

			await shell.GoToAsync("//main/items/item_42_detail");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as OrderDetailPage;
			Assert.NotNull(page);
			Assert.Equal("42", page.OrderId);
		}

		// ===== Combinations =====

		[Fact]
		public async Task ConstraintWithDefault_AbsentUsesDefault()
		{
			Routing.RegisterRoute("review/{stars:int=5}", typeof(DefaultStarsPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			await shell.GoToAsync("//main/products/review");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as DefaultStarsPage;
			Assert.NotNull(page);
			Assert.Equal("5", page.Stars);
		}

		[Fact]
		public async Task ConstraintWithDefault_PresentUsesValue()
		{
			Routing.RegisterRoute("review/{stars:int=5}", typeof(DefaultStarsPage));

			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellSectionRoute: "main", shellContentRoute: "products"));

			await shell.GoToAsync("//main/products/review/3");

			var page = shell.Navigation.NavigationStack[shell.Navigation.NavigationStack.Count - 1] as DefaultStarsPage;
			Assert.NotNull(page);
			Assert.Equal("3", page.Stars);
		}

		// ===== RouteTemplate.Parse unit tests =====

		[Fact]
		public void Parse_OptionalParameter()
		{
			var t = RouteTemplate.Parse("product/{sku?}", out var error);
			Assert.Null(error);
			Assert.NotNull(t);
			Assert.True(t.Segments[1].IsOptional);
			Assert.False(t.Segments[1].IsCatchAll);
			Assert.Equal("sku", t.Segments[1].Value);
		}

		[Fact]
		public void Parse_CatchAllParameter()
		{
			var t = RouteTemplate.Parse("files/{*path}", out var error);
			Assert.Null(error);
			Assert.True(t.Segments[1].IsCatchAll);
			Assert.Equal("path", t.Segments[1].Value);
		}

		[Fact]
		public void Parse_ConstrainedParameter()
		{
			var t = RouteTemplate.Parse("order/{id:int}", out var error);
			Assert.Null(error);
			Assert.Equal("int", t.Segments[1].Constraint);
			Assert.Equal("id", t.Segments[1].Value);
		}

		[Fact]
		public void Parse_DefaultValueParameter()
		{
			var t = RouteTemplate.Parse("review/{stars=5}", out var error);
			Assert.Null(error);
			Assert.True(t.Segments[1].IsOptional);
			Assert.Equal("5", t.Segments[1].DefaultValue);
			Assert.Equal("stars", t.Segments[1].Value);
		}

		[Fact]
		public void Parse_ConstraintAndDefault()
		{
			var t = RouteTemplate.Parse("page/{num:int=1}", out var error);
			Assert.Null(error);
			Assert.Equal("int", t.Segments[1].Constraint);
			Assert.Equal("1", t.Segments[1].DefaultValue);
			Assert.True(t.Segments[1].IsOptional);
		}

		[Fact]
		public void Parse_MixedSegment()
		{
			var t = RouteTemplate.Parse("product-{sku}", out var error);
			Assert.Null(error);
			Assert.True(t.Segments[0].IsMixed);
			Assert.Equal("product-", t.Segments[0].Prefix);
			Assert.Equal("", t.Segments[0].Suffix);
			Assert.Equal("sku", t.Segments[0].Value);
		}

		[Fact]
		public void Parse_MixedSegmentWithSuffix()
		{
			var t = RouteTemplate.Parse("{name}.html", out var error);
			Assert.Null(error);
			Assert.True(t.Segments[0].IsMixed);
			Assert.Equal("", t.Segments[0].Prefix);
			Assert.Equal(".html", t.Segments[0].Suffix);
			Assert.Equal("name", t.Segments[0].Value);
		}

		[Fact]
		public void Parse_CatchAllNotLast_Rejected()
		{
			var t = RouteTemplate.Parse("{*path}/extra", out var error);
			Assert.NotNull(error);
			Assert.Null(t);
		}

		[Fact]
		public void Parse_UnknownConstraint_Rejected()
		{
			var t = RouteTemplate.Parse("order/{id:regex}", out var error);
			Assert.NotNull(error);
			Assert.Null(t);
		}

		[Fact]
		public void SatisfiesConstraint_Int()
		{
			Assert.True(RouteTemplate.SatisfiesConstraint("int", "42"));
			Assert.True(RouteTemplate.SatisfiesConstraint("int", "-7"));
			Assert.False(RouteTemplate.SatisfiesConstraint("int", "abc"));
			Assert.False(RouteTemplate.SatisfiesConstraint("int", "3.14"));
		}

		[Fact]
		public void SatisfiesConstraint_Bool()
		{
			Assert.True(RouteTemplate.SatisfiesConstraint("bool", "true"));
			Assert.True(RouteTemplate.SatisfiesConstraint("bool", "False"));
			Assert.False(RouteTemplate.SatisfiesConstraint("bool", "yes"));
		}

		[Fact]
		public void SatisfiesConstraint_Alpha()
		{
			Assert.True(RouteTemplate.SatisfiesConstraint("alpha", "hello"));
			Assert.False(RouteTemplate.SatisfiesConstraint("alpha", "hello123"));
			Assert.False(RouteTemplate.SatisfiesConstraint("alpha", ""));
		}

		[Fact]
		public void SatisfiesConstraint_Guid()
		{
			Assert.True(RouteTemplate.SatisfiesConstraint("guid", "550e8400-e29b-41d4-a716-446655440000"));
			Assert.False(RouteTemplate.SatisfiesConstraint("guid", "not-a-guid"));
		}
	}
}
