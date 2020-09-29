using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class TriggerTests : ContentPage
	{
		public TriggerTests()
		{
			InitializeComponent();
		}

		public TriggerTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(false)]
			[TestCase(true)]
			public void ValueIsConverted(bool useCompiledXaml)
			{
				var layout = new TriggerTests(useCompiledXaml);
				Entry entry = layout.entry;
				Assert.NotNull(entry);

				var triggers = entry.Triggers;
				Assert.IsNotEmpty(triggers);
				var pwTrigger = triggers[0] as Trigger;
				Assert.AreEqual(Entry.IsPasswordProperty, pwTrigger.Property);
				Assert.AreEqual(true, pwTrigger.Value);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void ValueIsConvertedWithPropertyCondition(bool useCompiledXaml)
			{
				var layout = new TriggerTests(useCompiledXaml);
				Entry entry = layout.entry1;
				Assert.NotNull(entry);

				var triggers = entry.Triggers;
				Assert.IsNotEmpty(triggers);
				var pwTrigger = triggers[0] as MultiTrigger;
				var pwCondition = pwTrigger.Conditions[0] as PropertyCondition;
				Assert.AreEqual(Entry.IsPasswordProperty, pwCondition.Property);
				Assert.AreEqual(true, pwCondition.Value);
			}
		}
	}
}