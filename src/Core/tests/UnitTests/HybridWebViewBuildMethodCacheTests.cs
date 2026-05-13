#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.UnitTests
{

public partial class HybridWebViewBuildMethodCacheTests
{
	[Fact]
	public void ValidMethods_AreCached()
	{
		var cache = HybridWebViewHandler.BuildMethodCache(typeof(ValidTarget), ValidTargetJsonContext.Default);

		Assert.True(cache.ContainsKey("Add"));
		Assert.True(cache.ContainsKey("GetNameAsync"));
		Assert.True(cache.ContainsKey("DoWork"));
		Assert.Equal(3, cache.Count);
	}

	[Fact]
	public void OverloadedMethods_ThrowAtRegistration()
	{
		var ex = Assert.Throws<InvalidOperationException>(
			() => HybridWebViewHandler.BuildMethodCache(typeof(OverloadedTarget), OverloadedTargetJsonContext.Default));

		Assert.Contains("multiple methods named", ex.Message, StringComparison.OrdinalIgnoreCase);
		Assert.Contains("Compute", ex.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void GenericMethods_AreSkipped()
	{
		var cache = HybridWebViewHandler.BuildMethodCache(typeof(GenericMethodTarget), GenericMethodJsonContext.Default);

		Assert.False(cache.ContainsKey("Echo"));
		Assert.True(cache.ContainsKey("NonGeneric"));
		Assert.Single(cache);
	}

	[Fact]
	public void SpecialNameMethods_AreSkipped()
	{
		var cache = HybridWebViewHandler.BuildMethodCache(typeof(PropertyTarget), PropertyTargetJsonContext.Default);

		// get_Value and set_Value should be skipped (IsSpecialName)
		Assert.False(cache.ContainsKey("get_Value"));
		Assert.False(cache.ContainsKey("set_Value"));
		Assert.True(cache.ContainsKey("DoSomething"));
		Assert.Single(cache);
	}

	[Fact]
	public void RefParameter_ThrowsAtRegistration()
	{
		var ex = Assert.Throws<InvalidOperationException>(
			() => HybridWebViewHandler.BuildMethodCache(typeof(RefParamTarget), RefParamJsonContext.Default));

		Assert.Contains("ref/pointer", ex.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void MissingParamJsonTypeInfo_ThrowsAtRegistration()
	{
		// EmptyJsonContext has no type registrations at all
		var ex = Assert.Throws<InvalidOperationException>(
			() => HybridWebViewHandler.BuildMethodCache(typeof(ValidTarget), EmptyJsonContext.Default));

		Assert.Contains("does not contain metadata", ex.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void MissingReturnJsonTypeInfo_ThrowsAtRegistration()
	{
		// ParamOnlyJsonContext has int but NOT string — GetNameAsync returns Task<string>
		var ex = Assert.Throws<InvalidOperationException>(
			() => HybridWebViewHandler.BuildMethodCache(typeof(ValidTarget), ParamOnlyJsonContext.Default));

		Assert.Contains("does not contain metadata", ex.Message, StringComparison.OrdinalIgnoreCase);
		Assert.Contains("GetNameAsync", ex.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task CachedDelegate_InvokesCorrectly_VoidReturn()
	{
		var cache = HybridWebViewHandler.BuildMethodCache(typeof(ValidTarget), ValidTargetJsonContext.Default);
		var target = new ValidTarget();

		var handler = (HybridWebViewHandler.DotNetMethodDelegate)cache["DoWork"];
		var result = await handler(target, null);

		Assert.Null(result);
		Assert.True(target.WorkDone);
	}

	[Fact]
	public async Task CachedDelegate_InvokesCorrectly_SyncReturn()
	{
		var cache = HybridWebViewHandler.BuildMethodCache(typeof(ValidTarget), ValidTargetJsonContext.Default);
		var target = new ValidTarget();

		var handler = (HybridWebViewHandler.DotNetMethodDelegate)cache["Add"];
		var result = await handler(target, new[] { "3", "4" });

		Assert.Equal("7", result);
	}

	[Fact]
	public async Task CachedDelegate_InvokesCorrectly_AsyncReturn()
	{
		var cache = HybridWebViewHandler.BuildMethodCache(typeof(ValidTarget), ValidTargetJsonContext.Default);
		var target = new ValidTarget();

		var handler = (HybridWebViewHandler.DotNetMethodDelegate)cache["GetNameAsync"];
		var result = await handler(target, null);

		Assert.Equal("\"test\"", result);
	}

	[Fact]
	public async Task CachedDelegate_WrongParamCount_Throws()
	{
		var cache = HybridWebViewHandler.BuildMethodCache(typeof(ValidTarget), ValidTargetJsonContext.Default);
		var target = new ValidTarget();

		var handler = (HybridWebViewHandler.DotNetMethodDelegate)cache["Add"];
		await Assert.ThrowsAsync<InvalidOperationException>(
			() => handler(target, new[] { "1" })); // expects 2 params, got 1
	}

	// --- Target types ---

	public class ValidTarget
	{
		public bool WorkDone { get; private set; }

		public int Add(int a, int b) => a + b;
		public async Task<string> GetNameAsync()
		{
			await Task.Yield();
			return "test";
		}
		public void DoWork() => WorkDone = true;
	}

	public class OverloadedTarget
	{
		public int Compute(int a) => a;
		public int Compute(int a, int b) => a + b;
	}

	public class GenericMethodTarget
	{
		public T Echo<T>(T value) => value;
		public int NonGeneric() => 42;
	}

	public class PropertyTarget
	{
		public int Value { get; set; }
		public void DoSomething() { }
	}

	public class RefParamTarget
	{
		public void Modify(ref int x) => x = 42;
	}

	// --- JSON contexts ---

	[JsonSerializable(typeof(int))]
	[JsonSerializable(typeof(string))]
	internal partial class ValidTargetJsonContext : JsonSerializerContext { }

	[JsonSerializable(typeof(int))]
	internal partial class OverloadedTargetJsonContext : JsonSerializerContext { }

	[JsonSerializable(typeof(int))]
	internal partial class GenericMethodJsonContext : JsonSerializerContext { }

	[JsonSerializable(typeof(int))]
	internal partial class PropertyTargetJsonContext : JsonSerializerContext { }

	[JsonSerializable(typeof(int))]
	internal partial class RefParamJsonContext : JsonSerializerContext { }

	[JsonSerializable(typeof(bool))]
	internal partial class EmptyJsonContext : JsonSerializerContext { }

	[JsonSerializable(typeof(int))]
	internal partial class ParamOnlyJsonContext : JsonSerializerContext { }
}
}
