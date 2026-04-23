using Xunit;
using ShellRouteTemplates;

namespace ShellRouteTemplates.Tests;

// Stand-in page types — like ProductDetailPage, ReviewPage etc. in the Garden sample.
file class ProductDetailPage { }
file class ProductReviewPage { }
file class OrderDetailPage { }
file class CatalogPage { }

public class RouteTemplateParsingTests
{
    [Fact]
    public void Parses_LiteralOnlyRoute()
    {
        var t = RouteTemplate.Parse("product");
        Assert.False(t.HasParameters);
        Assert.Single(t.Segments);
        Assert.Equal("product", t.Segments[0].Literal);
    }

    [Fact]
    public void Parses_SingleParameterRoute()
    {
        var t = RouteTemplate.Parse("product/{sku}");
        Assert.True(t.HasParameters);
        Assert.Equal(2, t.Segments.Count);
        Assert.Equal("product", t.Segments[0].Literal);
        Assert.Equal("sku", t.Segments[1].ParameterName);
    }

    [Fact]
    public void Parses_NestedParameters()
    {
        var t = RouteTemplate.Parse("order/{orderId}/line/{lineId}");
        Assert.Equal(4, t.Segments.Count);
        Assert.Equal("orderId", t.Segments[1].ParameterName);
        Assert.Equal("lineId", t.Segments[3].ParameterName);
    }

    [Fact]
    public void Rejects_DuplicateParameterNames()
    {
        Assert.Throws<ArgumentException>(() => RouteTemplate.Parse("a/{x}/b/{x}"));
    }

    [Fact]
    public void Rejects_EmptyParameter()
    {
        Assert.Throws<ArgumentException>(() => RouteTemplate.Parse("product/{}"));
    }

    [Fact]
    public void Rejects_MixedSegment_v1()
    {
        // Documented limitation: prototype rejects "product-{sku}". ASP.NET Core supports it.
        Assert.Throws<ArgumentException>(() => RouteTemplate.Parse("a/product-{sku}"));
    }

    [Theory]
    [InlineData("product", 2)]                  // 1 literal × 2
    [InlineData("product/{sku}", 3)]            // literal(2) + param(1)
    [InlineData("product/{sku}/review", 5)]     // literal(2) + param(1) + literal(2)
    public void Specificity_FavorsLiteralSegments(string template, int expected)
    {
        Assert.Equal(expected, RouteTemplate.Parse(template).Specificity);
    }
}

public class RouteTemplateMatchingTests
{
    [Fact]
    public void Matches_LiteralRoute_ExtractsNoParams()
    {
        var t = RouteTemplate.Parse("product");
        Assert.True(t.TryMatch(new[] { "product" }, 0, out var consumed, out var p));
        Assert.Equal(1, consumed);
        Assert.Empty(p);
    }

    [Fact]
    public void Matches_TemplateRoute_ExtractsParam()
    {
        var t = RouteTemplate.Parse("product/{sku}");
        Assert.True(t.TryMatch(new[] { "product", "seed-tomato" }, 0, out var consumed, out var p));
        Assert.Equal(2, consumed);
        Assert.Equal("seed-tomato", p["sku"]);
    }

    [Fact]
    public void DoesNotMatch_WrongLiteral()
    {
        var t = RouteTemplate.Parse("product/{sku}");
        Assert.False(t.TryMatch(new[] { "review", "seed-tomato" }, 0, out _, out _));
    }

    [Fact]
    public void DoesNotMatch_NotEnoughSegments()
    {
        var t = RouteTemplate.Parse("product/{sku}");
        Assert.False(t.TryMatch(new[] { "product" }, 0, out _, out _));
    }

    [Fact]
    public void Matches_AtNonZeroOffset()
    {
        var t = RouteTemplate.Parse("product/{sku}");
        // "products" (a shell item) precedes the template
        var uri = new[] { "main", "products", "product", "seed-tomato" };
        Assert.True(t.TryMatch(uri, 2, out var consumed, out var p));
        Assert.Equal(2, consumed);
        Assert.Equal("seed-tomato", p["sku"]);
    }

    [Fact]
    public void DecodesPercentEncodedValues()
    {
        var t = RouteTemplate.Parse("file/{name}");
        Assert.True(t.TryMatch(new[] { "file", "hello%20world" }, 0, out _, out var p));
        Assert.Equal("hello world", p["name"]);
    }
}

public class RouteTablePrecedenceTests
{
    [Fact]
    public void LiteralBeatsTemplate_WhenBothMatch()
    {
        // Per ASP.NET Core: "product" wins over "{anything}" for the URI "product".
        var table = new RouteTable();
        table.Register("product", typeof(CatalogPage));
        table.Register("{anything}", typeof(ProductDetailPage));

        var (segs, _) = UriParser.Parse("product");
        var result = table.MatchPath(segs);

        Assert.NotNull(result);
        Assert.Single(result!.Matches);
        Assert.Equal(typeof(CatalogPage), result.Matches[0].PageType);
    }

    [Fact]
    public void Template_Wins_WhenLiteralDoesNotApply()
    {
        var table = new RouteTable();
        table.Register("product", typeof(CatalogPage));
        table.Register("product/{sku}", typeof(ProductDetailPage));

        var (segs, _) = UriParser.Parse("product/seed-tomato");
        var result = table.MatchPath(segs);

        Assert.NotNull(result);
        Assert.Single(result!.Matches);
        Assert.Equal(typeof(ProductDetailPage), result.Matches[0].PageType);
        Assert.Equal("seed-tomato", result.AllParameters["sku"]);
    }

    [Fact]
    public void LongerLiteralChain_WinsOverShorterTemplate()
    {
        var table = new RouteTable();
        table.Register("{a}/{b}", typeof(ProductDetailPage));
        table.Register("product/seed-tomato", typeof(CatalogPage)); // hypothetical literal pin

        var (segs, _) = UriParser.Parse("product/seed-tomato");
        var result = table.MatchPath(segs);

        Assert.NotNull(result);
        Assert.Equal(typeof(CatalogPage), result!.Matches[0].PageType);
    }
}

public class RouteTableMultiSegmentTests
{
    [Fact]
    public void ChainsTwoTemplates_AndExtractsParamsForBoth()
    {
        // Garden sample: //main/products/product/seed-tomato/review
        // After the shell-element prefix "main/products" is consumed by the Shell
        // matcher, what's left for global routes is: product/seed-tomato/review.
        var table = new RouteTable();
        table.Register("product/{sku}", typeof(ProductDetailPage));
        table.Register("review", typeof(ProductReviewPage));

        var (segs, _) = UriParser.Parse("product/seed-tomato/review");
        var result = table.MatchPath(segs);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Matches.Count);
        Assert.Equal(typeof(ProductDetailPage), result.Matches[0].PageType);
        Assert.Equal(typeof(ProductReviewPage), result.Matches[1].PageType);

        // Parameter inheritance: sku captured by the parent template flows down to the review page.
        Assert.Equal("seed-tomato", result.AllParameters["sku"]);
    }

    [Fact]
    public void BothPagesSeeSku_ViaShellLikeApplyQueryAttributes()
    {
        // Demonstrates how ShellNavigationManager.ApplyQueryAttributes would deliver params.
        // The simulated apply: every page receives the merged dictionary, then the page's
        // [QueryProperty] attributes pick out what it needs.
        var table = new RouteTable();
        table.Register("product/{sku}", typeof(ProductDetailPage));
        table.Register("review", typeof(ProductReviewPage));

        var (segs, _) = UriParser.Parse("product/seed-tomato/review");
        var result = table.MatchPath(segs)!;

        // Simulate applying attributes to each created page in stack order.
        // (Real Shell calls ApplyQueryAttributes per page in ShellSection.GoToAsync.)
        var deliveredToProduct = new Dictionary<string, string>(result.AllParameters, StringComparer.Ordinal);
        var deliveredToReview = new Dictionary<string, string>(result.AllParameters, StringComparer.Ordinal);

        Assert.Equal("seed-tomato", deliveredToProduct["sku"]);
        Assert.Equal("seed-tomato", deliveredToReview["sku"]);
    }

    [Fact]
    public void OrderId_FromGardenSample()
    {
        var table = new RouteTable();
        table.Register("order/{orderId}", typeof(OrderDetailPage));

        var (segs, _) = UriParser.Parse("order/ORD-00001");
        var result = table.MatchPath(segs);

        Assert.NotNull(result);
        Assert.Equal("ORD-00001", result!.AllParameters["orderId"]);
    }

    [Fact]
    public void NestedParams_FromTwoSeparateTemplates()
    {
        var table = new RouteTable();
        table.Register("order/{orderId}", typeof(OrderDetailPage));
        table.Register("line/{lineId}", typeof(ProductDetailPage));

        var (segs, _) = UriParser.Parse("order/ORD-00001/line/L42");
        var result = table.MatchPath(segs);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Matches.Count);
        Assert.Equal("ORD-00001", result.AllParameters["orderId"]);
        Assert.Equal("L42", result.AllParameters["lineId"]);
    }

    [Fact]
    public void ChildSameParamName_OverridesParent()
    {
        // Documented: when two templates declare the same param name, the deepest match wins.
        // Mirrors ASP.NET Core's "innermost route value wins" rule.
        var table = new RouteTable();
        table.Register("a/{id}", typeof(ProductDetailPage));
        table.Register("b/{id}", typeof(ProductReviewPage));

        var (segs, _) = UriParser.Parse("a/parent-id/b/child-id");
        var result = table.MatchPath(segs);

        Assert.NotNull(result);
        Assert.Equal("child-id", result!.AllParameters["id"]);
    }

    [Fact]
    public void ReturnsNull_WhenAnyUriSegmentIsUnmatched()
    {
        var table = new RouteTable();
        table.Register("product/{sku}", typeof(ProductDetailPage));

        var (segs, _) = UriParser.Parse("product/seed-tomato/unknown-tail");
        Assert.Null(table.MatchPath(segs));
    }
}

public class QueryStringInteropTests
{
    [Fact]
    public void PathParamAndQueryString_Coexist()
    {
        var table = new RouteTable();
        table.Register("product/{sku}", typeof(ProductDetailPage));

        var (segs, query) = UriParser.Parse("product/seed-tomato?highlight=true");
        var result = table.MatchPath(segs)!;

        // Path params and query string params merge into the dictionary that
        // ShellNavigationManager hands to ApplyQueryAttributes. Path wins on key conflicts
        // (same rule as ShellRouteParameters.SetQueryStringParameters today: it only adds
        // keys that aren't already present).
        var merged = new Dictionary<string, string>(result.AllParameters, StringComparer.Ordinal);
        foreach (var kv in query)
            if (!merged.ContainsKey(kv.Key))
                merged[kv.Key] = kv.Value;

        Assert.Equal("seed-tomato", merged["sku"]);
        Assert.Equal("true", merged["highlight"]);
    }

    [Fact]
    public void PathParamWins_OverQueryStringSameKey()
    {
        var table = new RouteTable();
        table.Register("product/{sku}", typeof(ProductDetailPage));

        var (segs, query) = UriParser.Parse("product/seed-tomato?sku=should-be-ignored");
        var result = table.MatchPath(segs)!;

        var merged = new Dictionary<string, string>(result.AllParameters, StringComparer.Ordinal);
        foreach (var kv in query)
            if (!merged.ContainsKey(kv.Key))
                merged[kv.Key] = kv.Value;

        Assert.Equal("seed-tomato", merged["sku"]);
    }
}

public class BackwardsCompatibilityTests
{
    [Fact]
    public void LiteralRouteOnly_BehavesExactlyLikeBefore()
    {
        // Sanity: literal routes must keep working unchanged. This is the "non-breaking" promise.
        var table = new RouteTable();
        table.Register("product", typeof(CatalogPage));
        table.Register("review", typeof(ProductReviewPage));

        var (segs, _) = UriParser.Parse("product/review");
        var result = table.MatchPath(segs);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Matches.Count);
        Assert.Empty(result.AllParameters);
    }

    [Fact]
    public void RouteWithoutBraces_NeverParsedAsTemplate()
    {
        var t = RouteTemplate.Parse("a/b/c");
        Assert.False(t.HasParameters);
    }
}
