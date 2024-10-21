using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class DependencyResolutionTests : BaseTestFixture
	{
		class MockElement { }

		class MockElementRenderer : IRegisterable { }

		class MockRendererWithParam : IRegisterable
		{
			readonly object _aParameter;

			public MockRendererWithParam(object aParameter)
			{
				_aParameter = aParameter ?? throw new ArgumentNullException(nameof(aParameter));
			}
		}

		class MockEffect : Effect
		{
			protected override void OnAttached()
			{
				throw new NotImplementedException();
			}

			protected override void OnDetached()
			{
				throw new NotImplementedException();
			}
		}

		interface IMockService { }

		class MockServiceImpl : IMockService { }

		// ReSharper disable once ClassNeverInstantiated.Local 
		// (Just testing the type registration, don't need an instance)
		class MockServiceImpl2 : IMockService { }

		class TypeWhichWillNotResolveCorrectly
		{
		}

		class IncorrectType
		{
		}

		class MockContainer
		{
			readonly Dictionary<Type, object> _services;
			readonly Dictionary<Type, Func<object, object>> _factories;

			public MockContainer()
			{
				_services = new Dictionary<Type, object>();
				_factories = new Dictionary<Type, Func<object, object>>();
			}

			public void Register(Type type, object result)
			{
				_services.Add(type, result);
			}

			public void Register(Type type, Func<object, object> factory)
			{
				_factories.Add(type, factory);
			}

			public object Resolve(Type type, params object[] args)
			{
				if (_services.ContainsKey(type))
				{
					return _services[type];
				}

				if (_factories.ContainsKey(type))
				{
					return _factories[type].Invoke(args[0]);
				}

				return null;
			}
		}

		MockContainer _container;


		public DependencyResolutionTests()
		{
			_container = new MockContainer();
			object Resolver(Type type, object[] args) => _container.Resolve(type, args);

			DependencyResolver.ResolveUsing(Resolver);
		}

		[Fact]
		public void ThrowsIfResolverReturnsWrongType()
		{
			_container = new MockContainer();
			_container.Register(typeof(TypeWhichWillNotResolveCorrectly), new IncorrectType());
			DependencyService.Register<TypeWhichWillNotResolveCorrectly>();

			Assert.Throws<InvalidCastException>(() => DependencyService.Resolve<TypeWhichWillNotResolveCorrectly>());
		}

		[Fact]
		public void GetHandlerFromContainer()
		{
			Internals.Registrar.Registered.Register(typeof(MockElement), typeof(MockElementRenderer));
			var renderer = new MockElementRenderer();
			_container.Register(typeof(MockElementRenderer), renderer);
			var result = Internals.Registrar.Registered.GetHandler(typeof(MockElement));
			var typedRenderer = (MockElementRenderer)result;

			Assert.Same(typedRenderer, renderer);
		}

		[Fact]
		public void GetEffectFromContainer()
		{
			string effectName = "anEffect";
			Internals.Registrar.Effects[effectName] = new Registrar.EffectType(typeof(MockEffect));
			var effect = new MockEffect();
			_container.Register(typeof(MockEffect), effect);
			var result = Effect.Resolve(effectName);

			Assert.Same(result, effect);
		}

		[Fact]
		public void GetServiceFromContainer()
		{
			MockServiceImpl impl = new MockServiceImpl();
			_container.Register(typeof(IMockService), impl);
			DependencyService.Register<MockServiceImpl>();
			var result = DependencyService.Resolve<IMockService>();

			Assert.Same(result, impl);
		}

		[Fact]
		public void PreferServiceTypeFromContainer()
		{
			MockServiceImpl impl = new MockServiceImpl();
			_container.Register(typeof(IMockService), impl);
			DependencyService.Register<IMockService, MockServiceImpl2>();
			var result = DependencyService.Resolve<IMockService>();

			Assert.Same(result, impl);
		}

		[Fact]
		public void FallbackOnDependencyServiceIfNotInContainer()
		{
			DependencyService.Register<MockServiceImpl>();
			var result = DependencyService.Resolve<IMockService>();

			Assert.NotNull(result);
		}

		[Fact]
		public void HandlerWithParameter()
		{
			Internals.Registrar.Registered.Register(typeof(MockElement), typeof(MockRendererWithParam));

			_container.Register(typeof(MockRendererWithParam), (o) => new MockRendererWithParam(o));

			var context = "Pretend this is an Android context";

			var result = Internals.Registrar.Registered.GetHandler(typeof(MockElement), null, null, context);
			var typedRenderer = (MockRendererWithParam)result;

			Assert.IsType<MockRendererWithParam>(typedRenderer);
		}
	}
}