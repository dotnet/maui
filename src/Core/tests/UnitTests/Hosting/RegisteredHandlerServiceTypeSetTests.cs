using System;
using System.Collections.Generic;
using Microsoft.Maui.Hosting.Internal;
using Microsoft.Maui.Platform;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.UnitTesting.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class RegisteredHandlerServiceTypeSetTests
	{
		[Fact]
		public void ResolveCorrespondingHandlerServiceType()
		{
			var registeredTypes = new RegisteredHandlerServiceTypeSet();
			registeredTypes.Add(typeof(ViewStub));

			var type = registeredTypes.ResolveVirtualViewToRegisteredHandlerServiceType(typeof(ViewStub));

			Assert.Equal(typeof(ViewStub), type);
		}

		[Fact]
		public void ThrowsWhenNoMatchingHandlerServiceTypeIsRegistered()
		{
			var registeredTypes = new RegisteredHandlerServiceTypeSet();

			Assert.Throws<HandlerNotFoundException>(() => registeredTypes.ResolveVirtualViewToRegisteredHandlerServiceType(typeof(ViewStub)));
		}

		[Theory]
		[InlineData(typeof(IViewStub))]
		[InlineData(typeof(IMyBaseViewStub))]
		[InlineData(typeof(IMyDerivedViewStub))]
		public void ResolvesBaseInterfaceServiceType(Type baseInterfaceType)
		{
			var registeredTypes = new RegisteredHandlerServiceTypeSet();
			registeredTypes.Add(baseInterfaceType);

			var resolvedServiceType = registeredTypes.ResolveVirtualViewToRegisteredHandlerServiceType(typeof(MyDerivedViewStub));

			Assert.NotNull(resolvedServiceType);
			Assert.Same(baseInterfaceType, resolvedServiceType);
		}

		public static IEnumerable<object[]> RegisteredTypePermutations =>
			new List<object[]>
			{
				new object[] { new Type[] { typeof(IMyDerivedViewStub), typeof(IMyBaseViewStub), typeof(IViewStub) } },
				new object[] { new Type[] { typeof(IMyDerivedViewStub), typeof(IViewStub), typeof(IMyBaseViewStub) } },
				new object[] { new Type[] { typeof(IMyBaseViewStub), typeof(IMyDerivedViewStub), typeof(IViewStub) } },
				new object[] { new Type[] { typeof(IMyBaseViewStub), typeof(IViewStub), typeof(IMyDerivedViewStub) } },
				new object[] { new Type[] { typeof(IViewStub), typeof(IMyDerivedViewStub), typeof(IMyBaseViewStub) } },
				new object[] { new Type[] { typeof(IViewStub), typeof(IMyBaseViewStub), typeof(IMyDerivedViewStub) } },
			};

		[Theory]
		[MemberData(nameof(RegisteredTypePermutations))]
		public void ResolvesToMostDerivedBaseInterfaceServiceType(Type[] registeredTypePermutations)
		{
			var registeredTypes = new RegisteredHandlerServiceTypeSet();

			foreach (Type registeredType in registeredTypePermutations)
				registeredTypes.Add(registeredType);

			var resolvedServiceType = registeredTypes.ResolveVirtualViewToRegisteredHandlerServiceType(typeof(MyDerivedViewStub));

			Assert.Equal(typeof(IMyDerivedViewStub), resolvedServiceType);
		}

		[Fact]
		public void ResolvesToConcreteTypeOverInterfaceType()
		{
			var registeredTypes = new RegisteredHandlerServiceTypeSet();
			registeredTypes.Add(typeof(ViewStub));
			registeredTypes.Add(typeof(IMyDerivedViewStub));
			registeredTypes.Add(typeof(IMyBaseViewStub));
			registeredTypes.Add(typeof(IViewStub));

			var resolvedServiceType = registeredTypes.ResolveVirtualViewToRegisteredHandlerServiceType(typeof(MyDerivedViewStub));

			Assert.Equal(typeof(ViewStub), resolvedServiceType);
		}

		[Fact]
		public void DoesNotResolveMoreDerivedTypes()
		{
			var registeredTypes = new RegisteredHandlerServiceTypeSet();
			registeredTypes.Add(typeof(MyBaseViewStub));
			registeredTypes.Add(typeof(MyDerivedViewStub));

			Assert.Throws<HandlerNotFoundException>(() => registeredTypes.ResolveVirtualViewToRegisteredHandlerServiceType(typeof(ViewStub)));
		}

		[Theory]
		[InlineData(typeof(IViewStub), typeof(IViewStub))]
		[InlineData(typeof(ViewStub), typeof(IViewStub))]
		[InlineData(typeof(MyBaseViewStub), typeof(MyBaseViewStub))]
		[InlineData(typeof(MyDerivedViewStub), typeof(MyBaseViewStub))]
		public void ResolveClosestApplicableServiceType(Type type, Type expectedImageSourceServiceType)
		{
			var registeredTypes = new RegisteredHandlerServiceTypeSet();
			registeredTypes.Add(typeof(IViewStub));
			registeredTypes.Add(typeof(MyBaseViewStub));

			var resolvedServiceType = registeredTypes.ResolveVirtualViewToRegisteredHandlerServiceType(type);

			Assert.Equal(expectedImageSourceServiceType, resolvedServiceType);
		}

		interface IMyBaseViewStub : IViewStub { }
		interface IMyDerivedViewStub : IMyBaseViewStub { }
		class MyBaseViewStub : ViewStub, IMyBaseViewStub { }
		class MyDerivedViewStub : MyBaseViewStub, IMyDerivedViewStub { }


		[Fact]
		public void ThrowsWhenOnlyInterfacesRelatedByInheritanceAreRegistered()
		{
			var registeredTypes = new RegisteredHandlerServiceTypeSet();
			registeredTypes.Add(typeof(IParentAViewStub));
			registeredTypes.Add(typeof(IParentBViewStub));

			Assert.Throws<InvalidOperationException>(() => registeredTypes.ResolveVirtualViewToRegisteredHandlerServiceType(typeof(ChildViewStub)));
		}

		[Fact]
		public void ResolvesToHandlerRegisteredUnderConcreteTypeDespiteAmbiguousInterfacesRegistered()
		{
			var registeredTypes = new RegisteredHandlerServiceTypeSet();
			registeredTypes.Add(typeof(ViewStub));
			registeredTypes.Add(typeof(IParentAViewStub));
			registeredTypes.Add(typeof(IParentBViewStub));

			var resolvedServiceType = registeredTypes.ResolveVirtualViewToRegisteredHandlerServiceType(typeof(ChildViewStub));

			Assert.Equal(typeof(ViewStub), resolvedServiceType);
		}

		class ChildViewStub : ViewStub, IParentAViewStub, IParentBViewStub { }
		interface IParentAViewStub : IViewStub { }
		interface IParentBViewStub : IViewStub { }
	}
}