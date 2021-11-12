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
			var services = new MauiFactory(collection, false);

			var first = new MauiContext(services);
			var second = new MauiContext(first);

			Assert.Same(obj, second.Services.GetService<TestThing>());
		}

		[Fact]
		public void AddSpecificInstanceOverridesBase()
		{
			var baseObj = new TestThing();

			var collection = new MauiServiceCollection();
			collection.AddSingleton(baseObj);
			var services = new MauiFactory(collection, false);

			var specificObj = new TestThing();
			var context = new MauiContext(services);
			context.AddSpecific(specificObj);

			Assert.Same(specificObj, context.Services.GetService<TestThing>());
		}

		[Fact]
		public void AddSpecificFactoryOverridesBase()
		{
			var baseObj = new TestThing();

			var collection = new MauiServiceCollection();
			collection.AddSingleton(baseObj);
			var services = new MauiFactory(collection, false);

			var specificObj = new TestThing();
			var context = new MauiContext(services);
			context.AddSpecific(() => specificObj);

			Assert.Same(specificObj, context.Services.GetService<TestThing>());
		}

		[Fact]
		public void AddSpecificFactoryIsLazy()
		{
			var count = 0;
			var specificObj = new TestThing();

			var collection = new MauiServiceCollection();
			var services = new MauiFactory(collection, false);
			var context = new MauiContext(services);
			context.AddSpecific(() => { count++; return specificObj; });

			Assert.Equal(0, count);
			Assert.Same(specificObj, context.Services.GetService<TestThing>());
			Assert.Equal(1, count);
			Assert.Same(specificObj, context.Services.GetService<TestThing>());
			Assert.Equal(1, count);
		}

		[Fact]
		public void AddSpecificIsNotWeak()
		{
			var collection = new MauiServiceCollection();
			var services = new MauiFactory(collection, false);
			var context = new MauiContext(services);

			DoAdd(context);

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.NotNull(context.Services.GetService<TestThing>());

			static void DoAdd(MauiContext ctx)
			{
				var specificObj = new TestThing();
				ctx.AddSpecific(specificObj);
			}
		}

		[Fact]
		public void AddWeakSpecificIsWeak()
		{
			var collection = new MauiServiceCollection();
			var services = new MauiFactory(collection, false);
			var context = new MauiContext(services);

			DoAdd(context);

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.Null(context.Services.GetService<TestThing>());

			static void DoAdd(MauiContext ctx)
			{
				var specificObj = new TestThing();
				ctx.AddWeakSpecific(specificObj);
			}
		}

		[Fact]
		public void CloneCanOverrideIncludeService()
		{
			var obj = new TestThing();
			var obj2 = new TestThing();

			var collection = new MauiServiceCollection();
			collection.AddSingleton(obj);
			var services = new MauiFactory(collection, false);

			var first = new MauiContext(services);

			var second = new MauiContext(first);
			second.AddSpecific(obj2);

			Assert.Same(obj2, second.Services.GetService<TestThing>());
		}

		class TestThing
		{
		}
	}
}