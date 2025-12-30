using System;
using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

//https://github.com/dotnet/roslyn/pull/78316
public static class IncrementalValueProviderExtensions
{
	public static IncrementalValuesProvider<(TProvider1 provider1, TProvider2 provider2, TProvider3 provider3)> Combine<TProvider1, TProvider2, TProvider3>(this IncrementalValuesProvider<TProvider1> provider1, IncrementalValueProvider<TProvider2> provider2, IncrementalValueProvider<TProvider3> provider3) => provider1.Combine(provider2).Combine(provider3).Select(static (t, _) => (t.Left.Left, t.Left.Right, t.Right));

	public static IncrementalValuesProvider<(TProvider1 provider1, TProvider2 provider2, TProvider3 provider3, TProvider4 provider4)> Combine<TProvider1, TProvider2, TProvider3, TProvider4>(this IncrementalValuesProvider<TProvider1> provider1, IncrementalValueProvider<TProvider2> provider2, IncrementalValueProvider<TProvider3> provider3, IncrementalValueProvider<TProvider4> provider4) => provider1.Combine(provider2).Combine(provider3).Combine(provider4).Select(static (t, _) => (t.Left.Left.Left, t.Left.Left.Right, t.Left.Right, t.Right));

	public static IncrementalValuesProvider<(TProvider1 provider1, TProvider2 provider2, TProvider3 provider3, TProvider4 provider4, TProvider5 provider5)> Combine<TProvider1, TProvider2, TProvider3, TProvider4, TProvider5>(this IncrementalValuesProvider<TProvider1> provider1, IncrementalValueProvider<TProvider2> provider2, IncrementalValueProvider<TProvider3> provider3, IncrementalValueProvider<TProvider4> provider4, IncrementalValueProvider<TProvider5> provider5) => provider1.Combine(provider2).Combine(provider3).Combine(provider4).Combine(provider5).Select(static (t, _) => (t.Left.Left.Left.Left, t.Left.Left.Left.Right, t.Left.Left.Right, t.Left.Right, t.Right));

	public static IncrementalValueProvider<(TProvider1 provider1, TProvider2 provider2, TProvider3 provider3)> Combine<TProvider1, TProvider2, TProvider3>(this IncrementalValueProvider<TProvider1> provider1, IncrementalValueProvider<TProvider2> provider2, IncrementalValueProvider<TProvider3> provider3) => provider1.Combine(provider2).Combine(provider3).Select(static (t, _) => (t.Left.Left, t.Left.Right, t.Right));

	public static IncrementalValueProvider<(TProvider1 provider1, TProvider2 provider2, TProvider3 provider3, TProvider4 provider4)> Combine<TProvider1, TProvider2, TProvider3, TProvider4>(this IncrementalValueProvider<TProvider1> provider1, IncrementalValueProvider<TProvider2> provider2, IncrementalValueProvider<TProvider3> provider3, IncrementalValueProvider<TProvider4> provider4) => provider1.Combine(provider2).Combine(provider3).Combine(provider4).Select(static (t, _) => (t.Left.Left.Left, t.Left.Left.Right, t.Left.Right, t.Right));

	public static IncrementalValueProvider<(TProvider1 provider1, TProvider2 provider2, TProvider3 provider3, TProvider4 provider4, TProvider5 provider5)> Combine<TProvider1, TProvider2, TProvider3, TProvider4, TProvider5>(this IncrementalValueProvider<TProvider1> provider1, IncrementalValueProvider<TProvider2> provider2, IncrementalValueProvider<TProvider3> provider3, IncrementalValueProvider<TProvider4> provider4, IncrementalValueProvider<TProvider5> provider5) => provider1.Combine(provider2).Combine(provider3).Combine(provider4).Combine(provider5).Select(static (t, _) => (t.Left.Left.Left.Left, t.Left.Left.Left.Right, t.Left.Left.Right, t.Left.Right, t.Right));

}
