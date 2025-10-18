using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

[assembly: TestHandler(typeof(Button), typeof(ButtonTarget))]
[assembly: TestHandler(typeof(Slider), typeof(SliderTarget))]
[assembly: TestHandler(typeof(ButtonChild), typeof(ButtonChildTarget))]
[assembly: TestHandler(typeof(Button), typeof(VisualButtonTarget), new[] { typeof(VisualMarkerUnitTests) })]
[assembly: TestHandler(typeof(Slider), typeof(VisualSliderTarget), new[] { typeof(VisualMarkerUnitTests) })]
[assembly: TestHandler(typeof(ButtonPriority), typeof(ButtonHigherPriorityTarget), Priority = 1)]
[assembly: TestHandlerLowerPriority(typeof(ButtonPriority), typeof(ButtonLowerPriorityTarget), Priority = 0)]

namespace Microsoft.Maui.Controls.Core.UnitTests
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


	public class PriorityRegistrarTests : BaseTestFixture
	{

		public PriorityRegistrarTests()
		{

			Internals.Registrar.RegisterAll(new[] {
				typeof (TestHandlerAttribute),
				typeof (TestHandlerLowerPriority)
			});

		}

		[Fact]
		public void BasicTest()
		{
			IRegisterable renderWithTarget = Internals.Registrar.Registered.GetHandler(typeof(ButtonPriority));
			Assert.Equal(typeof(ButtonHigherPriorityTarget), renderWithTarget.GetType());
		}
	}


	public class VisualRegistrarTests : BaseTestFixture
	{

		public VisualRegistrarTests()
		{

			Internals.Registrar.RegisterAll(new[] {
				typeof (TestHandlerAttribute)
			});

		}


		[Fact]
		public void RegisteringANewDefaultShouldReplaceRenderWithAttributeForFallbackVisual()
		{
			Internals.Registrar.Registered.Register(typeof(RenderWith), typeof(RenderWithSetAsNewDefault));
			var renderWithTarget = Internals.Registrar.Registered.GetHandler(typeof(RenderWith), typeof(VisualMarkerUnitTests));

			Assert.IsType<RenderWithSetAsNewDefault>(renderWithTarget);
		}


		[Fact]
		public void EnsureDefaultChildRendererTrumpsParentRenderWith()
		{
			Microsoft.Maui.Controls.Internals.Registrar.Registered.Register(typeof(RenderWithChild), typeof(RenderWithChildTarget));

			IRegisterable renderWithTarget;

			renderWithTarget = Internals.Registrar.Registered.GetHandler(typeof(RenderWithChild));
			Assert.NotNull(renderWithTarget);
			Assert.IsType<RenderWithChildTarget>(renderWithTarget);

			renderWithTarget = Internals.Registrar.Registered.GetHandler(typeof(RenderWithChild), typeof(RegisteredWithNobodyMarker));
			Assert.NotNull(renderWithTarget);
			Assert.IsType<RenderWithChildTarget>(renderWithTarget);

			renderWithTarget = Internals.Registrar.Registered.GetHandler(typeof(RenderWithChild), typeof(VisualMarkerUnitTests));
			Assert.NotNull(renderWithTarget);
			Assert.IsType<RenderWithChildTarget>(renderWithTarget);
		}

		[Fact]
		public void GetButtonChildHandler()
		{
			var buttonTarget = Internals.Registrar.Registered.GetHandler(typeof(ButtonChild), typeof(RegisteredWithNobodyMarker));
			Assert.NotNull(buttonTarget);
			Assert.IsType<ButtonChildTarget>(buttonTarget);

			buttonTarget = Internals.Registrar.Registered.GetHandler(typeof(ButtonChild), typeof(VisualMarkerUnitTests));
			Assert.NotNull(buttonTarget);
			Assert.IsType<ButtonChildTarget>(buttonTarget);
		}

		[Fact]
		public void GetButtonHandler()
		{
			var buttonTarget = Internals.Registrar.Registered.GetHandler(typeof(Button), typeof(VisualMarkerUnitTests));
			Assert.NotNull(buttonTarget);
			Assert.IsType<VisualButtonTarget>(buttonTarget);

			buttonTarget = Internals.Registrar.Registered.GetHandler(typeof(Button));
			Assert.NotNull(buttonTarget);
			Assert.IsType<ButtonTarget>(buttonTarget);


			Button button = new Button();
			object someObject = new object();
			buttonTarget = Internals.Registrar.Registered.GetHandler(typeof(Button), button, new VisualMarkerUnitTests(), someObject);
			Assert.NotNull(buttonTarget);
			Assert.IsType<VisualButtonTarget>(buttonTarget);

			var visualButtonTarget = (VisualButtonTarget)buttonTarget;
			Assert.Equal(visualButtonTarget.Param1, someObject);
			Assert.Equal(visualButtonTarget.Param2, button);

		}

		[Fact]
		public void GetSliderHandler()
		{
			var sliderTarget = Internals.Registrar.Registered.GetHandler(typeof(Slider), typeof(VisualMarkerUnitTests));
			Assert.NotNull(sliderTarget);
			Assert.IsType<VisualSliderTarget>(sliderTarget);

			sliderTarget = Internals.Registrar.Registered.GetHandler<SliderTarget>(typeof(Slider));
			Assert.NotNull(sliderTarget);
			Assert.IsType<SliderTarget>(sliderTarget);
		}
	}


	public class RegistrarTests : BaseTestFixture
	{

		public RegistrarTests()
		{

			Internals.Registrar.RegisterAll(new[] { typeof(TestHandlerAttribute) });
		}

		[Fact]
		public void GetButtonHandler()
		{
			var buttonTarget = Internals.Registrar.Registered.GetHandler<ButtonTarget>(typeof(Button));
			Assert.NotNull(buttonTarget);
			Assert.IsType<ButtonTarget>(buttonTarget);
		}

		[Fact]
		public void GetSliderHandler()
		{
			var sliderTarget = Internals.Registrar.Registered.GetHandler<SliderTarget>(typeof(Slider));
			Assert.NotNull(sliderTarget);
			Assert.IsType<SliderTarget>(sliderTarget);
		}
	}


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



		public SimpleRegistrarUnitTests()
		{
			VisualElement.SetDefaultVisual(VisualMarker.Default);
		}

		[Fact]
		public void TestConstructor()
		{
			var registrar = new Internals.Registrar<MockRenderer>();

			var renderer = registrar.GetHandler(typeof(Button));

			Assert.Null(renderer);
		}

		[Fact]
		public void TestGetRendererForKnownClass()
		{
			var registrar = new Internals.Registrar<MockRenderer>();

			registrar.Register(typeof(View), typeof(MockRenderer));

			var renderer = registrar.GetHandler(typeof(View));

			Assert.IsType<MockRenderer>(renderer);
		}

		[Fact]
		public void TestGetRendererForUnknownSubclass()
		{
			var registrar = new Internals.Registrar<MockRenderer>();

			registrar.Register(typeof(View), typeof(MockRenderer));

			var renderer = registrar.GetHandler(typeof(Button));

			Assert.IsType<MockRenderer>(renderer);
		}

		[Fact]
		public void TestGetRendererWithRegisteredSubclass()
		{
			var registrar = new Internals.Registrar<MockRenderer>();

			registrar.Register(typeof(View), typeof(MockRenderer));
			registrar.Register(typeof(Button), typeof(ButtonMockRenderer));

			var buttonRenderer = registrar.GetHandler(typeof(Button));
			var viewRenderer = registrar.GetHandler(typeof(View));

			Assert.IsType<ButtonMockRenderer>(buttonRenderer);
			Assert.IsNotType<ButtonMockRenderer>(viewRenderer);
			Assert.IsType<MockRenderer>(viewRenderer);
		}

		[Fact]
		public void TestReplaceRenderer()
		{
			var registrar = new Internals.Registrar<MockRenderer>();

			registrar.Register(typeof(View), typeof(MockRenderer));
			registrar.Register(typeof(Button), typeof(ButtonMockRenderer));
			registrar.Register(typeof(Button), typeof(ShinyButtonMockRenderer));

			var buttonRenderer = registrar.GetHandler(typeof(Button));

			Assert.IsType<ShinyButtonMockRenderer>(buttonRenderer);
		}

		[Fact]
		public void GetHandlerType()
		{
			var registrar = new Internals.Registrar<MockRenderer>();
			registrar.Register(typeof(View), typeof(MockRenderer));

			Assert.Equal(typeof(MockRenderer), registrar.GetHandlerType(typeof(View)));
		}

		[Fact]
		public void GetHandlerTypeForObject()
		{
			var registrar = new Internals.Registrar<MockRenderer>();
			registrar.Register(typeof(View), typeof(MockRenderer));
			registrar.Register(typeof(Button), typeof(ButtonMockRenderer));

			Assert.Equal(typeof(ButtonMockRenderer), registrar.GetHandlerTypeForObject(new Button()));
		}

		[Fact]
		public void GetHandlerForObject()
		{
			var registrar = new Internals.Registrar<MockRenderer>();
			registrar.Register(typeof(View), typeof(MockRenderer));
			registrar.Register(typeof(Button), typeof(ButtonMockRenderer));

			var buttonRenderer = registrar.GetHandlerForObject<MockRenderer>(new Button());
			Assert.IsType<ButtonMockRenderer>(buttonRenderer);
		}

		[Fact]
		public void TestGetRendererNullViewRenderer()
		{
			var registrar = new Internals.Registrar<MockRenderer>();

			//let's say that we are now registering the view of a viewcell
			registrar.Register(typeof(View), typeof(MockRenderer));
			//later we had a view that was registered with null because there's
			//no default renderer for View
			registrar.Register(typeof(View), null);

			var renderer = registrar.GetHandler(typeof(View));

			Assert.IsType<MockRenderer>(renderer);
		}

	}
}
