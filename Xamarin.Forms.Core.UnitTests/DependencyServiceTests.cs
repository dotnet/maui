using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

[assembly: Dependency(typeof(DependencyTestImpl))]

namespace Xamarin.Forms.Core.UnitTests
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
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
		public void GetGlobalInstance()
		{
			var global = DependencyService.Get<IDependencyTest>();

			Assert.NotNull(global);

			var secondFetch = DependencyService.Get<IDependencyTest>();

			Assert.True(ReferenceEquals(global, secondFetch));
		}

		[Test]
		public void NewInstanceIsNotGlobalInstance()
		{
			var global = DependencyService.Get<IDependencyTest>();

			Assert.NotNull(global);

			var secondFetch = DependencyService.Get<IDependencyTest>(DependencyFetchTarget.NewInstance);

			Assert.False(ReferenceEquals(global, secondFetch));
		}

		[Test]
		public void NewInstanceIsAlwaysNew()
		{
			var firstFetch = DependencyService.Get<IDependencyTest>(DependencyFetchTarget.NewInstance);

			Assert.NotNull(firstFetch);

			var secondFetch = DependencyService.Get<IDependencyTest>(DependencyFetchTarget.NewInstance);

			Assert.False(ReferenceEquals(firstFetch, secondFetch));
		}

		[Test]
		public void UnsatisfiedReturnsNull()
		{
			Assert.Null(DependencyService.Get<IUnsatisfied>());
		}

		[Test]
		public void RegisterTypeImplementation()
		{
			DependencyService.Register<DependencyTestRegisterImpl>();
			var global = DependencyService.Get<DependencyTestRegisterImpl>();
			Assert.NotNull(global);
		}


		[Test]
		public void RegisterInterfaceAndImplementations()
		{
			DependencyService.Register<IDependencyTestRegister, DependencyTestRegisterImpl2>();
			var global = DependencyService.Get<IDependencyTestRegister>();
			Assert.IsInstanceOf<DependencyTestRegisterImpl2>(global);
		}

		[Test]
		public void RegisterInterfaceAndOverrideImplementations()
		{
			DependencyService.Register<IDependencyTestRegister, DependencyTestRegisterImpl>();
			DependencyService.Register<IDependencyTestRegister, DependencyTestRegisterImpl2>();
			var global = DependencyService.Get<IDependencyTestRegister>();
			Assert.IsInstanceOf<DependencyTestRegisterImpl2>(global);
		}

		[Test]
		public void RegisterSingletonInterface()
		{
			var local = new DependencyTestRegisterImpl();
			DependencyService.RegisterSingleton<IDependencyTestRegister>(local);
			var global = DependencyService.Get<IDependencyTestRegister>();
			Assert.AreEqual(local, global);
		}
	}
}