using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers.Benchmarks;

/// <summary>
/// Startup-like scenarios for base-chain baggage.
/// Each benchmark call represents one startup emulation operation.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared)]
[SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 1, iterationCount: 8)]
public class BaseChainStartupBenchmarks
{
[Params(500, 1000)]
public int ControlCount { get; set; }

[Benchmark(Description = "B5_MockVisualTreeStartup")]
public ContentPage B5_MockVisualTreeStartup()
{
return CreateMockPage(ControlCount, includeStyles: false, resources: null);
}

[Benchmark(Description = "B6_AppLikeStartupEmulation")]
public Application B6_AppLikeStartupEmulation()
{
var app = new Application
{
Resources = CreateStyledResources()
};

var page = CreateMockPage(ControlCount, includeStyles: true, resources: app.Resources);
Application.SetCurrentApplication(app);
#pragma warning disable CS0618 // MainPage used in benchmark emulation only
app.MainPage = page;
#pragma warning restore CS0618

return app;
}

private static ContentPage CreateMockPage(int controlCount, bool includeStyles, ResourceDictionary? resources)
{
var layout = new VerticalStackLayout();

for (var i = 0; i < controlCount; i++)
{
var label = new Label { Text = $"Label {i}" };

if (includeStyles && resources is not null)
{
label.Style = (Style)resources["ExplicitLabelStyle"];
if ((i % 5) == 0)
label.SetDynamicResource(Label.TextColorProperty, "PrimaryTextColor");
}

layout.Children.Add(label);
}

return new ContentPage { Content = new ScrollView { Content = layout } };
}

private static ResourceDictionary CreateStyledResources()
{
var resources = new ResourceDictionary
{
["PrimaryTextColor"] = Colors.DarkSlateBlue,
};

resources.Add(new Style(typeof(Label))
{
Setters =
{
new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap },
new Setter { Property = Label.FontSizeProperty, Value = 14d },
}
});

resources["ExplicitLabelStyle"] = new Style(typeof(Label))
{
Setters =
{
new Setter { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold },
new Setter { Property = Label.TextColorProperty, Value = Colors.DarkViolet },
}
};

return resources;
}
}
