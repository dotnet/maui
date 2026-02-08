using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Benchmark : ContentPage
{
	public Benchmark(string inflator)
	{
		switch (inflator)
		{
			case "Runtime":
				InitializeComponentRuntime();
				break;
			case "XamlC":
				InitializeComponentXamlC();
				break;
			case "SourceGen":
				InitializeComponentSourceGen();
				break;
			default:
				throw new NotSupportedException($"no code for {inflator} generated. check the [XamlProcessing] attribute.");
		}
	}

	[Collection("Xaml Inflation")]
	public class Tests
	{
#if DEBUG
		private readonly ITestOutputHelper _output;

		public Tests(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public void ResourceDictionaryDiagnostics_SourceGen()
		{
			var page = new Benchmark("SourceGen");
			var diag = page.Resources.GetDiagnostics();

			_output.WriteLine($"=== ResourceDictionary Diagnostics (SourceGen with LazyRD) ===");
			_output.WriteLine($"Total resources: {diag.TotalCount}");
			_output.WriteLine($"  Eager (value types): {diag.EagerCount}");
			_output.WriteLine($"  Lazy (ref types): {diag.LazyCount}");
			_output.WriteLine($"    - Resolved: {diag.ResolvedLazyCount}");
			_output.WriteLine($"    - Unresolved: {diag.LazyCount - diag.ResolvedLazyCount}");
			_output.WriteLine("");
			
			_output.WriteLine($"Eager keys ({diag.EagerKeys.Count}):");
			foreach (var key in diag.EagerKeys)
				_output.WriteLine($"  - {key}");
			_output.WriteLine("");

			_output.WriteLine($"Resolved lazy keys ({diag.ResolvedKeys.Count}):");
			foreach (var key in diag.ResolvedKeys)
			{
				diag.InvocationCounts.TryGetValue(key, out var count);
				_output.WriteLine($"  - {key} (invoked {count}x)");
			}
			_output.WriteLine("");

			_output.WriteLine($"Unresolved lazy keys ({diag.UnresolvedKeys.Count}):");
			foreach (var key in diag.UnresolvedKeys)
				_output.WriteLine($"  - {key}");

			// Assertions
			Assert.True(diag.TotalCount > 0, "Should have resources");
			Assert.True(diag.LazyCount > 0, "Should have lazy resources with LazyRD enabled");
			
			// With Label uncommented, we expect the Label style to be resolved
			// but other styles should remain unresolved
			_output.WriteLine("");
			_output.WriteLine($"=== Summary ===");
			_output.WriteLine($"Lazy resources saved from initialization: {diag.LazyCount - diag.ResolvedLazyCount} / {diag.LazyCount}");
		}

		[Fact]
		public void ResourceDictionaryDiagnostics_SourceGen_WithExplicitAccess()
		{
			var page = new Benchmark("SourceGen");
			
			// Before accessing any resource
			var diagBefore = page.Resources.GetDiagnostics();
			_output.WriteLine($"=== Before accessing any resource ===");
			_output.WriteLine($"Resolved: {diagBefore.ResolvedLazyCount} / {diagBefore.LazyCount}");
			
			// Access just one resource - the Label style
			var labelStyle = page.Resources["Microsoft.Maui.Controls.Label"];
			Assert.NotNull(labelStyle);
			
			// After accessing Label style
			var diagAfter = page.Resources.GetDiagnostics();
			_output.WriteLine($"");
			_output.WriteLine($"=== After accessing Label style ===");
			_output.WriteLine($"Resolved: {diagAfter.ResolvedLazyCount} / {diagAfter.LazyCount}");
			_output.WriteLine($"");
			_output.WriteLine($"Resolved keys:");
			foreach (var key in diagAfter.ResolvedKeys)
				_output.WriteLine($"  - {key}");
			_output.WriteLine($"");
			_output.WriteLine($"Unresolved keys ({diagAfter.UnresolvedKeys.Count}):");
			// Just show count to keep output short
			
			// The Label style should be resolved, along with the colors it uses
			Assert.True(diagAfter.ResolvedKeys.Contains("Microsoft.Maui.Controls.Label"), "Label style should be resolved");
		}

		[Fact]
		public void ResourceDictionaryDiagnostics_XamlC()
		{
			var page = new Benchmark("XamlC");
			var diag = page.Resources.GetDiagnostics();

			_output.WriteLine($"=== ResourceDictionary Diagnostics (XamlC - no lazy) ===");
			_output.WriteLine($"Total resources: {diag.TotalCount}");
			_output.WriteLine($"  Eager: {diag.EagerCount}");
			_output.WriteLine($"  Lazy: {diag.LazyCount}");

			// XamlC doesn't use lazy resources
			Assert.Equal(0, diag.LazyCount);
			Assert.Equal(diag.TotalCount, diag.EagerCount);
		}
#endif
	}
}
