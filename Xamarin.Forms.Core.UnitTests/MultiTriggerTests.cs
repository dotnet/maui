using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class MultiTriggerTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			Device.PlatformServices = new MockPlatformServices();
			base.Setup();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		class MockElement : VisualElement
		{
		}

		[Test]
		public void SettersAppliedOnAttachIfConditionIsTrue()
		{
			var conditionbp = BindableProperty.Create("foo", typeof(string), typeof(BindableObject), null);
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var multiTrigger = new MultiTrigger(typeof(VisualElement))
			{
				Conditions = {
					new PropertyCondition { Property = conditionbp, Value = "foobar" },
					new BindingCondition { Binding = new Binding ("baz"), Value = "foobaz" },
				},
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				}
			};

			element.SetValue(setterbp, "default");
			element.SetValue(conditionbp, "foobar");
			element.BindingContext = new { baz = "foobaz" };
			Assert.AreEqual("default", element.GetValue(setterbp));
			element.Triggers.Add(multiTrigger);
			Assert.AreEqual("qux", element.GetValue(setterbp));
		}

		[Test]
		public void SettersNotAppliedOnAttachIfOneConditionIsFalse()
		{
			var conditionbp = BindableProperty.Create("foo", typeof(string), typeof(BindableObject), null);
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var multiTrigger = new MultiTrigger(typeof(VisualElement))
			{
				Conditions = {
					new PropertyCondition { Property = conditionbp, Value = "foobar" },
					new BindingCondition { Binding = new Binding ("baz"), Value = "foobaz" },
				},
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				}
			};

			element.SetValue(setterbp, "default");
			element.SetValue(conditionbp, "foobar");
			element.BindingContext = new { baz = "foobazXX" };
			Assert.AreEqual("default", element.GetValue(setterbp));
			element.Triggers.Add(multiTrigger);
			Assert.AreEqual("default", element.GetValue(setterbp));
		}

		[Test]
		public void SettersUnappliedOnDetach()
		{
			var conditionbp = BindableProperty.Create("foo", typeof(string), typeof(BindableObject), null);
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var multiTrigger = new MultiTrigger(typeof(VisualElement))
			{
				Conditions = {
					new PropertyCondition { Property = conditionbp, Value = "foobar" },
					new BindingCondition { Binding = new Binding ("baz"), Value = "foobaz" },
				},
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				}
			};

			element.SetValue(setterbp, "default");
			element.BindingContext = new { baz = "" };
			element.Triggers.Add(multiTrigger);
			Assert.AreEqual("default", element.GetValue(setterbp)); //both conditions false

			element.SetValue(conditionbp, "foobar");
			Assert.AreEqual("default", element.GetValue(setterbp)); //one condition false

			element.BindingContext = new { baz = "foobaz" };
			Assert.AreEqual("qux", element.GetValue(setterbp)); //both condition true
			element.Triggers.Remove(multiTrigger);
			Assert.AreEqual("default", element.GetValue(setterbp));
		}

		[Test]
		public void SettersAppliedAndUnappliedOnConditionsChange()
		{
			var conditionbp = BindableProperty.Create("foo", typeof(string), typeof(BindableObject), null);
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var multiTrigger = new MultiTrigger(typeof(VisualElement))
			{
				Conditions = {
					new PropertyCondition { Property = conditionbp, Value = "foobar" },
					new BindingCondition { Binding = new Binding ("baz"), Value = "foobaz" },
				},
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				}
			};

			element.SetValue(setterbp, "default");
			element.BindingContext = new { baz = "" };
			element.Triggers.Add(multiTrigger);
			Assert.AreEqual("default", element.GetValue(setterbp)); //both conditions false

			element.SetValue(conditionbp, "foobar");
			Assert.AreEqual("default", element.GetValue(setterbp)); //one condition false

			element.BindingContext = new { baz = "foobaz" };
			Assert.AreEqual("qux", element.GetValue(setterbp)); //both condition true

			element.BindingContext = new { baz = "" };
			Assert.AreEqual("default", element.GetValue(setterbp)); //one condition false

			element.BindingContext = new { baz = "foobaz" };
			Assert.AreEqual("qux", element.GetValue(setterbp)); //both condition true

			element.SetValue(conditionbp, "");
			Assert.AreEqual("default", element.GetValue(setterbp)); //one condition false

			element.SetValue(conditionbp, "foobar");
			Assert.AreEqual("qux", element.GetValue(setterbp)); //both condition true

			element.SetValue(conditionbp, "");
			element.BindingContext = new { baz = "foobaz" };
			Assert.AreEqual("default", element.GetValue(setterbp)); //both conditions false
		}
	}
}