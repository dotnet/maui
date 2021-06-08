using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class HandlerLifeCycleTests : BaseTestFixture
	{
		[Test]
		public void VirtualViewSet()
		{
			Button button = new Button();
			HandlerStub handlerStub = new HandlerStub();

			button.Handler = handlerStub;

			Assert.IsNotNull(handlerStub.VirtualView);
		}

		[Test]
		public void BasicAttachEvents()
		{
			Button button = new Button();
			bool attaching = false;
			bool attached = false;

			button.AttachingHandler += (_, __) => attaching = true;
			button.AttachedHandler += (_, __) =>
			{
				if (!attaching)
					Assert.Fail("Attached fired before attaching");

				attached = true;
			};

			button.Handler = new HandlerStub();

			Assert.IsTrue(attaching);
			Assert.IsTrue(attached);
		}


		[TestCase(null)]
		[TestCase(typeof(HandlerStub))]
		public void BasicDetachEvents(Type handlerType)
		{
			Button button = new Button();
			bool detaching = false;
			bool detached = false;

			button.DetachingHandler += (_, __) => detaching = true;
			button.DetachedHandler += (_, __) =>
			{
				if (!detaching)
					Assert.Fail("Detached fired before detaching");

				detached = true;
			};

			button.Handler = new HandlerStub();

			Assert.IsFalse(detaching);
			Assert.IsFalse(detached);

			if (handlerType != null)
				button.Handler = (IViewHandler)Activator.CreateInstance(handlerType);
			else
				button.Handler = null;

			Assert.IsTrue(detaching);
			Assert.IsTrue(detached);
		}


		[Test]
		public void BasicAttachMethods()
		{
			LifeCycleButton button = new LifeCycleButton();
			button.Handler = new HandlerStub();

			Assert.AreEqual(1, button.attaching);
			Assert.AreEqual(1, button.attached);
		}


		[TestCase(null)]
		[TestCase(typeof(HandlerStub))]
		public void BasicDetachMethods(Type handlerType)
		{
			LifeCycleButton button = new LifeCycleButton();

			button.Handler = new HandlerStub();

			Assert.Zero(button.detaching);
			Assert.Zero(button.detached);

			if (handlerType != null)
				button.Handler = (IViewHandler)Activator.CreateInstance(handlerType);
			else
				button.Handler = null;

			Assert.AreEqual(1, button.detached);
			Assert.AreEqual(1, button.detaching);
		}

		public class LifeCycleButton : Button
		{
			public int attaching = 0;
			public int attached = 0;
			public int detaching = 0;
			public int detached = 0;

			public override void OnAttachedHandler()
			{
				attached++;

				if (attached != attaching)
					Assert.Fail("Attaching/Attached fire mismatch");
			}

			public override void OnAttachingHandler() => attaching++;
			public override void OnDetachingHandler() => detaching++;
			public override void OnDetachedHandler()
			{
				detached++;

				if (detached != detaching)
					Assert.Fail("Attaching/Attached fire mismatch");
			}
		}
	}
}
