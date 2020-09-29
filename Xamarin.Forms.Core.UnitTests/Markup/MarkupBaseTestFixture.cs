using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using Xamarin.Forms.Core.UnitTests;
using static Xamarin.Forms.Core.Markup.Markup;

namespace Xamarin.Forms.Markup.UnitTests
{
	public class MarkupBaseTestFixture<TBindable> : MarkupBaseTestFixture where TBindable : BindableObject, new()
	{
		public MarkupBaseTestFixture(bool withExperimentalFlag) : base(withExperimentalFlag) { }

		protected TBindable Bindable { get; private set; }

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Bindable = new TBindable();
		}

		[TearDown]
		public override void TearDown()
		{
			Bindable = null;
			base.TearDown();
		}

		protected void TestPropertiesSet<TPropertyValue>(
			Action<TBindable> modify,
			params (BindableProperty property, TPropertyValue beforeValue, TPropertyValue expectedValue)[] propertyChanges
		) => TestPropertiesSet(Bindable, modify, propertyChanges);

		protected void TestPropertiesSet(
			Action<TBindable> modify,
			params (BindableProperty property, object beforeValue, object expectedValue)[] propertyChanges
		) => TestPropertiesSet(Bindable, modify, propertyChanges);
	}

	public class MarkupBaseTestFixture : BaseTestFixture
	{
		IReadOnlyList<string> originalFlags;
		readonly protected bool withExperimentalFlag;

		public MarkupBaseTestFixture(bool withExperimentalFlag) { this.withExperimentalFlag = withExperimentalFlag; }

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			originalFlags = Device.Flags;

			var flags = Device.Flags;
			var markupFlag = ExperimentalFlags.MarkupExperimental;

			ClearExperimental();
			if (withExperimentalFlag)
			{
				if (!(flags?.Contains(markupFlag) == true))
				{
					var newFlags = flags == null ? new List<string>() : new List<string>(flags);
					newFlags.Add(markupFlag);
					Device.SetFlags(newFlags);
				}
			}
			else
			{
				if (flags?.Contains(markupFlag) == true)
				{
					var newFlags = new List<string>(flags);
					newFlags.Remove(markupFlag);
					Device.SetFlags(newFlags);
				}
			}
		}

		[TearDown]
		public override void TearDown()
		{
			Device.SetFlags(originalFlags);
			base.TearDown();
		}

		protected void AssertExperimental(TestDelegate test)
		{
			if (withExperimentalFlag)
				test();
			else
				StringAssert.EndsWith(
					"part of Markup; to use it, you must opt-in by calling Forms.SetFlags(\"Markup_Experimental\") before calling Forms.Init().",
					Assert.Throws<InvalidOperationException>(test).Message
				);
		}

		protected void TestPropertiesSet<TBindable, TPropertyValue>(
			TBindable bindable,
			Action<TBindable> modify,
			params (BindableProperty property, TPropertyValue beforeValue, TPropertyValue expectedValue)[] propertyChanges
		) where TBindable : BindableObject => AssertExperimental(() =>
		{
			foreach (var change in propertyChanges)
			{
				bindable.SetValue(change.property, change.beforeValue);
				Assume.That(bindable.GetPropertyIfSet(change.property, change.expectedValue), Is.Not.EqualTo(change.expectedValue));
			}

			modify(bindable);

			foreach (var change in propertyChanges)
				Assert.That(bindable.GetPropertyIfSet(change.property, change.beforeValue), Is.EqualTo(change.expectedValue));
		});
	}
}