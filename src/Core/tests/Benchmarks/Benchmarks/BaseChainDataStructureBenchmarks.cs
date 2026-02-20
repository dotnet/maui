using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace Microsoft.Maui.Handlers.Benchmarks;

/// <summary>
/// Base-chain allocation benchmarks focused on one-operation scenarios.
/// These benchmarks intentionally avoid manual throughput loops so BenchmarkDotNet
/// reports meaningful per-operation numbers.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared)]
public class BaseChainDataStructureBenchmarks
{
public enum FeatureToggleScenario
{
GestureRecognizer,
Effect,
DynamicResource,
StyleClass,
NavigationTouch
}

[Params(
FeatureToggleScenario.GestureRecognizer,
FeatureToggleScenario.Effect,
FeatureToggleScenario.DynamicResource,
FeatureToggleScenario.StyleClass,
FeatureToggleScenario.NavigationTouch)]
public FeatureToggleScenario Scenario { get; set; }

[Benchmark(Description = "B0_NewObject")]
public object B0_NewObject() => new object();

[Benchmark(Description = "B1_NewLabel")]
public Label B1_NewLabel() => new Label();

[Benchmark(Description = "B2_NewLabelWithBasicProperties")]
public Label B2_NewLabelWithBasicProperties()
{
var label = new Label
{
Text = "bench",
IsVisible = true
};

return label;
}

[Benchmark(Description = "B3_NewLabelWithFeatureToggle")]
public Label B3_NewLabelWithFeatureToggle()
{
var label = new Label();

switch (Scenario)
{
case FeatureToggleScenario.GestureRecognizer:
label.GestureRecognizers.Add(new TapGestureRecognizer());
break;
case FeatureToggleScenario.Effect:
label.Effects.Add(new NoopRoutingEffect());
break;
case FeatureToggleScenario.DynamicResource:
label.SetDynamicResource(Label.TextColorProperty, "PrimaryTextColor");
break;
			case FeatureToggleScenario.StyleClass:
				label.StyleClass = new List<string> { "headline" };
				break;
case FeatureToggleScenario.NavigationTouch:
_ = label.Navigation.NavigationStack.Count;
break;
}

return label;
}

[Benchmark(Description = "B4_ResourceDictionaryStyleMaterialization")]
public Label B4_ResourceDictionaryStyleMaterialization()
{
var app = CreateMockApplicationWithStyles();
var label = new Label
{
Style = (Style)app.Resources["ExplicitLabelStyle"]
};

var page = new ContentPage { Content = label };
Application.SetCurrentApplication(app);
#pragma warning disable CS0618 // MainPage used in benchmark emulation only
app.MainPage = page;
#pragma warning restore CS0618

_ = label.TextColor;
return label;
}

private static Application CreateMockApplicationWithStyles()
{
var app = new Application();
var resources = new ResourceDictionary
{
["PrimaryTextColor"] = Colors.DarkSlateGray,
};

var implicitLabelStyle = new Style(typeof(Label))
{
Setters =
{
new Setter { Property = Label.TextColorProperty, Value = Colors.DimGray },
new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap },
}
};

resources.Add(implicitLabelStyle);

resources["ExplicitLabelStyle"] = new Style(typeof(Label))
{
Setters =
{
new Setter { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold },
new Setter { Property = Label.TextColorProperty, Value = Colors.MediumPurple },
}
};

app.Resources = resources;
return app;
}

private sealed class NoopRoutingEffect : RoutingEffect
{
public NoopRoutingEffect() : base("Benchmarks.Noop")
{
}
}
}
