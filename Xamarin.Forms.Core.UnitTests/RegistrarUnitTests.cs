using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

[assembly: TestHandler(typeof(Button), typeof(ButtonTarget))]
[assembly: TestHandler(typeof(Slider), typeof(SliderTarget))]
[assembly: TestHandler(typeof(ButtonChild), typeof(ButtonChildTarget))]
[assembly: TestHandler(typeof(Button), typeof(VisualButtonTarget), new[] { typeof(VisualMarkerUnitTests) })]
[assembly: TestHandler(typeof(Slider), typeof(VisualSliderTarget), new[] { typeof(VisualMarkerUnitTests) })]
[assembly: TestHandler(typeof(ButtonPriority), typeof(ButtonHigherPriorityTarget), Priority = 1)]
[assembly: TestHandlerLowerPriority(typeof(ButtonPriority), typeof(ButtonLowerPriorityTarget), Priority = 0)]

namespace Xamarin.Forms.Core.UnitTests
{
	internal class TestHandlerAttribute : HandlerAttribute
	{
		public TestHandlerAttribute(Type handler, Type target, Type[] supportedVisuals = null) : base(handler, target, supportedVisuals)
		{
		}
	}
	internal class TestHandlerLowerPriority : HandlerAttribute
	{
		public TestHandlerLowerPriority(Type handler, Type target, Type[] supportedVisuals = null) : base(handler, target, supportedVisuals)
		{
		}
	}

	public class VisualMarkerUnitTests : IVisual { }
	public class RegisteredWithNobodyMarker : IVisual { }

	internal class RenderWithTarget : IRegisterable { }
	[RenderWith(typeof(RenderWithTarget))]
	internal class RenderWith { }
	internal class RenderWithChild : RenderWith { }
	internal class RenderWithChildTarget : IRegisterable { }
	internal class RenderWithSetAsNewDefault : IRegisterable { }

	internal class VisualButtonTarget : IRegisterable
	{
		public object Param1 { get; }
		public object Param2 { get; }

		public VisualButtonTarget() { }
		public VisualButtonTarget(object param1, object param2)
		{
			Param1 = param1;
			Param2 = param2;
		}
	}

	internal class VisualSliderTarget : IRegisterable { }
	internal class ButtonChildTarget : IRegisterable { }
	internal class ButtonChild : Button, IRegisterable { }

	internal class ButtonTarget : IRegisterable { }

	internal class SliderTarget : IRegisterable { }

	internal class ButtonPriority { }
	internal class ButtonLowerPriorityTarget : IRegisterable { }
	internal class ButtonHigherPriorityTarget : IRegisterable { }

	[TestFixture]
	public class PriorityRegistrarTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
			Internals.Registrar.RegisterAll(new[] {
				typeof (TestHandlerAttribute),
				typeof (TestHandlerLowerPriority)
			});

		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
		public void BasicTest()
		{
			IRegisterable renderWithTarget = Internals.Registrar.Registered.GetHandler(typeof(ButtonPriority));
			Assert.AreEqual(typeof(ButtonHigherPriorityTarget), renderWithTarget.GetType());
		}
	}

	[TestFixture]
	public class VisualRegistrarTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
			Internals.Registrar.RegisterAll(new[] {
				typeof (TestHandlerAttribute)
			});

		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}


		[Test]
		public void RegisteringANewDefaultShouldReplaceRenderWithAttributeForFallbackVisual()
		{
			Internals.Registrar.Registered.Register(typeof(RenderWith), typeof(RenderWithSetAsNewDefault));
			var renderWithTarget = Internals.Registrar.Registered.GetHandler(typeof(RenderWith), typeof(VisualMarkerUnitTests));

			Assert.That(renderWithTarget, Is.InstanceOf<RenderWithSetAsNewDefault>());
		}


		[Test]
		public void EnsureDefaultChildRendererTrumpsParentRenderWith()
		{
			Xamarin.Forms.Internals.Registrar.Registered.Register(typeof(RenderWithChild), typeof(RenderWithChildTarget));

			IRegisterable renderWithTarget;

			renderWithTarget = Internals.Registrar.Registered.GetHandler(typeof(RenderWithChild));
			Assert.IsNotNull(renderWithTarget);
			Assert.That(renderWithTarget, Is.InstanceOf<RenderWithChildTarget>());

			renderWithTarget = Internals.Registrar.Registered.GetHandler(typeof(RenderWithChild), typeof(RegisteredWithNobodyMarker));
			Assert.IsNotNull(renderWithTarget);
			Assert.That(renderWithTarget, Is.InstanceOf<RenderWithChildTarget>());

			renderWithTarget = Internals.Registrar.Registered.GetHandler(typeof(RenderWithChild), typeof(VisualMarkerUnitTests));
			Assert.IsNotNull(renderWithTarget);
			Assert.That(renderWithTarget, Is.InstanceOf<RenderWithChildTarget>());
		}

		[Test]
		public void GetButtonChildHandler()
		{
			var buttonTarget = Internals.Registrar.Registered.GetHandler(typeof(ButtonChild), typeof(RegisteredWithNobodyMarker));
			Assert.IsNotNull(buttonTarget);
			Assert.That(buttonTarget, Is.InstanceOf<ButtonChildTarget>());

			buttonTarget = Internals.Registrar.Registered.GetHandler(typeof(ButtonChild), typeof(VisualMarkerUnitTests));
			Assert.IsNotNull(buttonTarget);
			Assert.That(buttonTarget, Is.InstanceOf<ButtonChildTarget>());
		}

		[Test]
		public void GetButtonHandler()
		{
			var buttonTarget = Internals.Registrar.Registered.GetHandler(typeof(Button), typeof(VisualMarkerUnitTests));
			Assert.IsNotNull(buttonTarget);
			Assert.That(buttonTarget, Is.InstanceOf<VisualButtonTarget>());

			buttonTarget = Internals.Registrar.Registered.GetHandler(typeof(Button));
			Assert.IsNotNull(buttonTarget);
			Assert.That(buttonTarget, Is.InstanceOf<ButtonTarget>());


			Button button = new Button();
			object someObject = new object();
			buttonTarget = Internals.Registrar.Registered.GetHandler(typeof(Button), button, new VisualMarkerUnitTests(), someObject);
			Assert.IsNotNull(buttonTarget);
			Assert.That(buttonTarget, Is.InstanceOf<VisualButtonTarget>());

			var visualButtonTarget = (VisualButtonTarget)buttonTarget;
			Assert.AreEqual(visualButtonTarget.Param1, someObject);
			Assert.AreEqual(visualButtonTarget.Param2, button);

		}

		[Test]
		public void GetSliderHandler()
		{
			var sliderTarget = Internals.Registrar.Registered.GetHandler(typeof(Slider), typeof(VisualMarkerUnitTests));
			Assert.IsNotNull(sliderTarget);
			Assert.That(sliderTarget, Is.InstanceOf<VisualSliderTarget>());

			sliderTarget = Internals.Registrar.Registered.GetHandler<SliderTarget>(typeof(Slider));
			Assert.IsNotNull(sliderTarget);
			Assert.That(sliderTarget, Is.InstanceOf<SliderTarget>());
		}
	}

	[TestFixture]
	public class RegistrarTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
			Internals.Registrar.RegisterAll(new[] {
				typeof (TestHandlerAttribute)
			});

		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
		public void GetButtonHandler()
		{
			var buttonTarget = Internals.Registrar.Registered.GetHandler<ButtonTarget>(typeof(Button));
			Assert.IsNotNull(buttonTarget);
			Assert.That(buttonTarget, Is.InstanceOf<ButtonTarget>());
		}

		[Test]
		public void GetSliderHandler()
		{
			var sliderTarget = Internals.Registrar.Registered.GetHandler<SliderTarget>(typeof(Slider));
			Assert.IsNotNull(sliderTarget);
			Assert.That(sliderTarget, Is.InstanceOf<SliderTarget>());
		}
	}

	[TestFixture]
	public class SimpleRegistrarUnitTests
	{
		class MockRenderer { }
		class ButtonMockRenderer : MockRenderer { }
		class ShinyButtonMockRenderer : MockRenderer { }
		class CrashMockRenderer : MockRenderer
		{
			public CrashMockRenderer()
			{
				throw new NotImplementedException();
			}
		}


		[SetUp]
		public void Setup()
		{
			VisualElement.SetDefaultVisual(VisualMarker.Default);
		}

		[Test]
		public void TestConstructor()
		{
			var registrar = new Internals.Registrar<MockRenderer>();

			var renderer = registrar.GetHandler(typeof(Button));

			Assert.Null(renderer);
		}

		[Test]
		public void TestGetRendererForKnownClass()
		{
			var registrar = new Internals.Registrar<MockRenderer>();

			registrar.Register(typeof(View), typeof(MockRenderer));

			var renderer = registrar.GetHandler(typeof(View));

			Assert.That(renderer, Is.InstanceOf<MockRenderer>());
		}

		[Test]
		public void TestGetRendererForUnknownSubclass()
		{
			var registrar = new Internals.Registrar<MockRenderer>();

			registrar.Register(typeof(View), typeof(MockRenderer));

			var renderer = registrar.GetHandler(typeof(Button));

			Assert.That(renderer, Is.InstanceOf<MockRenderer>());
		}

		[Test]
		public void TestGetRendererWithRegisteredSubclass()
		{
			var registrar = new Internals.Registrar<MockRenderer>();

			registrar.Register(typeof(View), typeof(MockRenderer));
			registrar.Register(typeof(Button), typeof(ButtonMockRenderer));

			var buttonRenderer = registrar.GetHandler(typeof(Button));
			var viewRenderer = registrar.GetHandler(typeof(View));

			Assert.That(buttonRenderer, Is.InstanceOf<ButtonMockRenderer>());
			Assert.That(viewRenderer, Is.Not.InstanceOf<ButtonMockRenderer>());
			Assert.That(viewRenderer, Is.InstanceOf<MockRenderer>());
		}

		[Test]
		public void TestReplaceRenderer()
		{
			var registrar = new Internals.Registrar<MockRenderer>();

			registrar.Register(typeof(View), typeof(MockRenderer));
			registrar.Register(typeof(Button), typeof(ButtonMockRenderer));
			registrar.Register(typeof(Button), typeof(ShinyButtonMockRenderer));

			var buttonRenderer = registrar.GetHandler(typeof(Button));

			Assert.That(buttonRenderer, Is.InstanceOf<ShinyButtonMockRenderer>());
		}

		[Test]
		public void GetHandlerType()
		{
			var registrar = new Internals.Registrar<MockRenderer>();
			registrar.Register(typeof(View), typeof(MockRenderer));

			Assert.AreEqual(typeof(MockRenderer), registrar.GetHandlerType(typeof(View)));
		}

		[Test]
		public void GetHandlerTypeForObject()
		{
			var registrar = new Internals.Registrar<MockRenderer>();
			registrar.Register(typeof(View), typeof(MockRenderer));
			registrar.Register(typeof(Button), typeof(ButtonMockRenderer));

			Assert.AreEqual(typeof(ButtonMockRenderer), registrar.GetHandlerTypeForObject(new Button()));
		}

		[Test]
		public void GetHandlerForObject()
		{
			var registrar = new Internals.Registrar<MockRenderer>();
			registrar.Register(typeof(View), typeof(MockRenderer));
			registrar.Register(typeof(Button), typeof(ButtonMockRenderer));

			var buttonRenderer = registrar.GetHandlerForObject<MockRenderer>(new Button());
			Assert.That(buttonRenderer, Is.InstanceOf<ButtonMockRenderer>());
		}

		[Test]
		public void TestGetRendererNullViewRenderer()
		{
			var registrar = new Internals.Registrar<MockRenderer>();

			//let's say that we are now registering the view of a viewcell
			registrar.Register(typeof(View), typeof(MockRenderer));
			//later we had a view that was registered with null because there's
			//no default renderer for View
			registrar.Register(typeof(View), null);

			var renderer = registrar.GetHandler(typeof(View));

			Assert.That(renderer, Is.InstanceOf<MockRenderer>());
		}

	}
}