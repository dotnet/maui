using System;

using NUnit.Framework;

using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.StyleSheets.UnitTests
{
	[TestFixture]
	public class IStylableTest
	{
		[SetUp]
		public void SetUp()
		{
			Device.PlatformServices = new MockPlatformServices();
			Internals.Registrar.RegisterAll(new Type[0]);
		}

		[TestCase]
		public void GetPropertyDefinedOnParent()
		{
			var label = new Label();
			var bp = ((IStylable)label).GetProperty("background-color", false);
			Assert.AreSame(VisualElement.BackgroundColorProperty, bp);
		}

		[TestCase]
		public void GetPropertyDefinedOnType()
		{
			var label = new Label();
			var bp = ((IStylable)label).GetProperty("color", false);
			Assert.AreSame(Label.TextColorProperty, bp);
		}

		[TestCase]
		public void GetPropertyDefinedOnType2()
		{
			var entry = new Entry();
			var bp = ((IStylable)entry).GetProperty("color", false);
			Assert.AreSame(Entry.TextColorProperty, bp);
		}

		[TestCase]
		public void GetInvalidPropertyForType()
		{
			var grid = new Grid();
			var bp = ((IStylable)grid).GetProperty("color", false);
			Assert.Null(bp);
		}

		[TestCase]
		public void GetPropertyDefinedOnPropertyOwnerType()
		{
			var frame = new Frame();
			var bp = ((IStylable)frame).GetProperty("padding-left", false);
			Assert.That(bp, Is.SameAs(PaddingElement.PaddingLeftProperty));
		}

		[TestCase]
		public void GetNonPublicProperty()
		{
			var label = new Label();
			var bp = ((IStylable)label).GetProperty("margin-right", false);
			Assert.That(bp, Is.SameAs(View.MarginRightProperty));
		}

	}
}