using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting.Internal;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core)]
	public partial class MauiContextTests
	{
		[Fact]
		public void CloneIncludeSameServices()
		{
			var obj = new TestThing();

			var collection = new MauiServiceCollection();
			collection.AddSingleton(obj);
			var services = new MauiFactory(collection);

			var first = new MauiContext(services);
			var second = new MauiContext(first.Services);

			Assert.Same(obj, second.Services.GetService<TestThing>());
		}

		[Fact]
		public void AddSpecificInstanceOverridesBase()
		{
			var baseObj = new TestThing();

			var collection = new MauiServiceCollection();
			collection.AddSingleton(baseObj);
			var services = new MauiFactory(collection);

			var specificObj = new TestThing();
			var context = new MauiContext(services);
			context.AddSpecific<TestThing>(specificObj);

			Assert.Same(specificObj, context.Services.GetService<TestThing>());
		}

		[Fact]
		public void AddSpecificIsNotWeak()
		{
			var collection = new MauiServiceCollection();
			var services = new MauiFactory(collection);
			var context = new MauiContext(services);

			DoAdd(context);

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.NotNull(context.Services.GetService<TestThing>());

			static void DoAdd(MauiContext ctx)
			{
				var specificObj = new TestThing();
				ctx.AddSpecific<TestThing>(specificObj);
			}
		}

		[Fact]
		public void AddWeakSpecificIsWeak()
		{
			var collection = new MauiServiceCollection();
			var services = new MauiFactory(collection);
			var context = new MauiContext(services);

			DoAdd(context);

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.Null(context.Services.GetService<TestThing>());

			static void DoAdd(MauiContext ctx)
			{
				var specificObj = new TestThing();
				ctx.AddWeakSpecific<TestThing>(specificObj);
			}
		}

		[Fact]
		public void CloneCanOverrideIncludeService()
		{
			var obj = new TestThing();
			var obj2 = new TestThing();

			var collection = new MauiServiceCollection();
			collection.AddSingleton(obj);
			var services = new MauiFactory(collection);

			var first = new MauiContext(services);

			var second = new MauiContext(first.Services);
			second.AddSpecific<TestThing>(obj2);

			Assert.Same(obj2, second.Services.GetService<TestThing>());
		}

		class TestThing
		{
		}
	}
}