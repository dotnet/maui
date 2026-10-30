using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting.Internal;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class MauiServiceCollectionTests
	{
		interface IService { }

		class First : IService { }

		class Second : IService { }

		[Fact]
		public void ReplacingADescriptorByIndexUpdatesTryGetService()
		{
			var collection = new MauiServiceCollection();
			collection.AddTransient<IService, First>();

			collection[0] = ServiceDescriptor.Transient<IService, Second>();

			Assert.True(collection.TryGetService(typeof(IService), out var descriptor));
			Assert.Equal(typeof(Second), descriptor!.ImplementationType);
			Assert.Equal(typeof(Second), collection[0].ImplementationType);
		}

		[Fact]
		public void ReplacingADescriptorWithADifferentServiceTypeMovesTheLookup()
		{
			var collection = new MauiServiceCollection();
			collection.AddTransient<IService, First>();

			collection[0] = ServiceDescriptor.Transient<First, First>();

			Assert.False(collection.TryGetService(typeof(IService), out _));
			Assert.True(collection.TryGetService(typeof(First), out var descriptor));
			Assert.Equal(typeof(First), descriptor!.ImplementationType);
		}

		[Fact]
		public void ReplacingAnEarlierDescriptorKeepsTheLastRegistrationInTheLookup()
		{
			var collection = new MauiServiceCollection();
			collection.AddTransient<IService, First>();
			collection.AddTransient<IService, Second>();

			collection[0] = ServiceDescriptor.Transient<IService, First>();

			Assert.True(collection.TryGetService(typeof(IService), out var descriptor));
			Assert.Equal(typeof(Second), descriptor!.ImplementationType);
		}
	}
}
