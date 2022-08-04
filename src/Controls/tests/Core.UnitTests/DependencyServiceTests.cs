using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

[assembly: Dependency(typeof(DependencyTestImpl))]

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public interface IDependencyTest
	{
		bool Works { get; }
	}

	public interface IDependencyTestRegister
	{
		bool Works { get; }
	}

	public interface IUnsatisfied
	{
		bool Broken { get; }
	}

	public class DependencyTestImpl : IDependencyTest
	{
		public bool Works { get { return true; } }
	}

	public class DependencyTestRegisterImpl : IDependencyTestRegister
	{
		public bool Works { get { return true; } }
	}

	public class DependencyTestRegisterImpl2 : IDependencyTestRegister
	{
		public bool Works { get { return false; } }
	}

	public class DependencyServiceTests : BaseTestFixture
	{
		[Fact]
		public void GetGlobalInstance()
		{
			var global = DependencyService.Get<IDependencyTest>();

			Assert.NotNull(global);

			var secondFetch = DependencyService.Get<IDependencyTest>();

			Assert.True(ReferenceEquals(global, secondFetch));
		}

		[Fact]
		public void NewInstanceIsNotGlobalInstance()
		{
			var global = DependencyService.Get<IDependencyTest>();

			Assert.NotNull(global);

			var secondFetch = DependencyService.Get<IDependencyTest>(DependencyFetchTarget.NewInstance);

			Assert.False(ReferenceEquals(global, secondFetch));
		}

		[Fact]
		public void NewInstanceIsAlwaysNew()
		{
			var firstFetch = DependencyService.Get<IDependencyTest>(DependencyFetchTarget.NewInstance);

			Assert.NotNull(firstFetch);

			var secondFetch = DependencyService.Get<IDependencyTest>(DependencyFetchTarget.NewInstance);

			Assert.False(ReferenceEquals(firstFetch, secondFetch));
		}

		[Fact]
		public void UnsatisfiedReturnsNull()
		{
			Assert.Null(DependencyService.Get<IUnsatisfied>());
		}

		[Fact]
		public void RegisterTypeImplementation()
		{
			DependencyService.Register<DependencyTestRegisterImpl>();
			var global = DependencyService.Get<DependencyTestRegisterImpl>();
			Assert.NotNull(global);
		}


		[Fact]
		public void RegisterInterfaceAndImplementations()
		{
			DependencyService.Register<IDependencyTestRegister, DependencyTestRegisterImpl2>();
			var global = DependencyService.Get<IDependencyTestRegister>();
			Assert.IsType<DependencyTestRegisterImpl2>(global);
		}

		[Fact]
		public void RegisterInterfaceAndOverrideImplementations()
		{
			DependencyService.Register<IDependencyTestRegister, DependencyTestRegisterImpl>();
			DependencyService.Register<IDependencyTestRegister, DependencyTestRegisterImpl2>();
			var global = DependencyService.Get<IDependencyTestRegister>();
			Assert.IsType<DependencyTestRegisterImpl2>(global);
		}

		[Fact]
		public void RegisterSingletonInterface()
		{
			var local = new DependencyTestRegisterImpl();
			DependencyService.RegisterSingleton<IDependencyTestRegister>(local);
			var global = DependencyService.Get<IDependencyTestRegister>();
			Assert.Equal(local, global);
		}
	}
}
