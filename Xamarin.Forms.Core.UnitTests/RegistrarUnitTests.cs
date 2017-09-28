using System;
using System.Reflection;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

[assembly: TestHandler (typeof (Button), typeof (ButtonTarget))]
[assembly: TestHandler (typeof (Slider), typeof (SliderTarget))]

namespace Xamarin.Forms.Core.UnitTests
{
	internal class TestHandlerAttribute : HandlerAttribute
	{
		public TestHandlerAttribute (Type handler, Type target) : base (handler, target)
		{
			
		}
	}

	internal class ButtonTarget : IRegisterable {}

	internal class SliderTarget : IRegisterable {}

	[TestFixture]
	public class RegistrarTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup ()
		{
			base.Setup ();
			Device.PlatformServices = new MockPlatformServices ();
			Internals.Registrar.RegisterAll (new [] {
				typeof (TestHandlerAttribute)
			});

		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown ();
			Device.PlatformServices = null;
		}

		[Test]
		public void GetButtonHandler ()
		{
			var buttonTarget = Internals.Registrar.Registered.GetHandler<ButtonTarget> (typeof (Button));
			Assert.IsNotNull (buttonTarget);
			Assert.That (buttonTarget, Is.InstanceOf<ButtonTarget>());
		}

		[Test]
		public void GetSliderHandler()
		{
			var sliderTarget = Internals.Registrar.Registered.GetHandler<SliderTarget> (typeof (Slider));
			Assert.IsNotNull (sliderTarget);
			Assert.That (sliderTarget, Is.InstanceOf<SliderTarget> ());
		}
	}

	[TestFixture]
	public class SimpleRegistrarUnitTests
	{
		class MockRenderer {}
		class ButtonMockRenderer : MockRenderer {}
		class ShinyButtonMockRenderer : MockRenderer {}
		class CrashMockRenderer : MockRenderer
		{
			public CrashMockRenderer ()
			{
				throw new NotImplementedException();
			}
		}

		[Test]
		public void TestConstructor ()
		{
			var registrar = new Internals.Registrar<MockRenderer> ();

			var renderer = registrar.GetHandler (typeof (Button));

			Assert.Null (renderer);
		}

		[Test]
		public void TestGetRendererForKnownClass ()
		{
			var registrar = new Internals.Registrar<MockRenderer> ();

			registrar.Register (typeof(View), typeof(MockRenderer));

			var renderer = registrar.GetHandler (typeof (View));

			Assert.That (renderer, Is.InstanceOf<MockRenderer>());
		}

		[Test]
		public void TestGetRendererForUnknownSubclass ()
		{
			var registrar = new Internals.Registrar<MockRenderer> ();

			registrar.Register (typeof (View), typeof (MockRenderer));

			var renderer = registrar.GetHandler (typeof (Button));

			Assert.That (renderer, Is.InstanceOf<MockRenderer>());
		}

		[Test]
		public void TestGetRendererWithRegisteredSubclass ()
		{
			var registrar = new Internals.Registrar<MockRenderer> ();

			registrar.Register (typeof (View), typeof (MockRenderer));
			registrar.Register (typeof (Button), typeof (ButtonMockRenderer));

			var buttonRenderer = registrar.GetHandler (typeof (Button));
			var viewRenderer = registrar.GetHandler (typeof (View));

			Assert.That (buttonRenderer, Is.InstanceOf<ButtonMockRenderer>());
			Assert.That (viewRenderer, Is.Not.InstanceOf<ButtonMockRenderer>());
			Assert.That (viewRenderer, Is.InstanceOf<MockRenderer>());
		}

		[Test]
		public void TestReplaceRenderer ()
		{
			var registrar = new Internals.Registrar<MockRenderer> ();

			registrar.Register (typeof (View), typeof (MockRenderer));
			registrar.Register (typeof (Button), typeof (ButtonMockRenderer));
			registrar.Register (typeof (Button), typeof (ShinyButtonMockRenderer));

			var buttonRenderer = registrar.GetHandler (typeof (Button));

			Assert.That (buttonRenderer, Is.InstanceOf<ShinyButtonMockRenderer>());
		}

		[Test]
		public void GetHandlerType()
		{
			var registrar = new Internals.Registrar<MockRenderer>();
			registrar.Register (typeof (View), typeof (MockRenderer));

			Assert.AreEqual (typeof (MockRenderer), registrar.GetHandlerType (typeof (View)));
		}

		[Test]
		public void GetHandlerTypeForObject()
		{
			var registrar = new Internals.Registrar<MockRenderer>();
			registrar.Register (typeof (View), typeof (MockRenderer));
			registrar.Register (typeof (Button), typeof (ButtonMockRenderer));

			Assert.AreEqual (typeof (ButtonMockRenderer), registrar.GetHandlerTypeForObject (new Button ()));
		}

		[Test]
		public void GetHandlerForObject()
		{
			var registrar = new Internals.Registrar<MockRenderer>();
			registrar.Register (typeof (View), typeof (MockRenderer));
			registrar.Register (typeof (Button), typeof (ButtonMockRenderer));

            var buttonRenderer = registrar.GetHandlerForObject<MockRenderer> (new Button ());
            Assert.That (buttonRenderer, Is.InstanceOf<ButtonMockRenderer> ());
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
