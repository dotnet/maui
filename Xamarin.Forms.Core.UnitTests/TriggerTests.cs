using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TriggerTests : BaseTestFixture
	{
		class MockElement : VisualElement
		{
		}

		[Test]
		public void SettersAppliedOnConditionChanged()
		{
			var conditionbp = BindableProperty.Create("foo", typeof(string), typeof(BindableObject), null);
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var trigger = new Trigger(typeof(VisualElement))
			{
				Property = conditionbp,
				Value = "foobar",
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				}
			};

			element.SetValue(setterbp, "default");
			element.Triggers.Add(trigger);

			Assert.AreEqual("default", element.GetValue(setterbp));
			element.SetValue(conditionbp, "foobar");
			Assert.AreEqual("qux", element.GetValue(setterbp));
			element.SetValue(conditionbp, "");
			Assert.AreEqual("default", element.GetValue(setterbp));
		}

		[Test]
		public void SettersAppliedOnAttachIfConditionIsTrue()
		{
			var conditionbp = BindableProperty.Create("foo", typeof(string), typeof(BindableObject), null);
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var trigger = new Trigger(typeof(VisualElement))
			{
				Property = conditionbp,
				Value = "foobar",
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				}
			};

			element.SetValue(setterbp, "default");
			element.SetValue(conditionbp, "foobar");
			Assert.AreEqual("default", element.GetValue(setterbp));
			element.Triggers.Add(trigger);
			Assert.AreEqual("qux", element.GetValue(setterbp));
		}

		[Test]
		public void SettersUnappliedOnDetach()
		{
			var conditionbp = BindableProperty.Create("foo", typeof(string), typeof(BindableObject), null);
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var trigger = new Trigger(typeof(VisualElement))
			{
				Property = conditionbp,
				Value = "foobar",
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				}
			};

			element.SetValue(setterbp, "default");
			element.Triggers.Add(trigger);

			Assert.AreEqual("default", element.GetValue(setterbp));
			element.SetValue(conditionbp, "foobar");
			Assert.AreEqual("qux", element.GetValue(setterbp));
			element.Triggers.Remove(trigger);
			Assert.AreEqual("default", element.GetValue(setterbp));
		}

		[Test]
		public void EnterAndExitActionsTriggered()
		{
			var conditionbp = BindableProperty.Create("foo", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var enteraction = new MockTriggerAction();
			var exitaction = new MockTriggerAction();
			var trigger = new Trigger(typeof(VisualElement))
			{
				Property = conditionbp,
				Value = "foobar",
				EnterActions = {
					enteraction
				},
				ExitActions = {
					exitaction
				}
			};

			Assert.False(enteraction.Invoked);
			Assert.False(exitaction.Invoked);
			element.Triggers.Add(trigger);
			Assert.False(enteraction.Invoked);
			Assert.False(exitaction.Invoked);
			element.SetValue(conditionbp, "foobar");
			Assert.True(enteraction.Invoked);
			Assert.False(exitaction.Invoked);

			enteraction.Invoked = exitaction.Invoked = false;
			Assert.False(enteraction.Invoked);
			Assert.False(exitaction.Invoked);
			element.SetValue(conditionbp, "");
			Assert.False(enteraction.Invoked);
			Assert.True(exitaction.Invoked);
		}

		[Test]
		// https://bugzilla.xamarin.com/show_bug.cgi?id=32896
		public void SettersWithBindingsUnappliedIfConditionIsFalse()
		{
			var conditionbp = BindableProperty.Create("foo", typeof(string), typeof(BindableObject), null);
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var trigger = new Trigger(typeof(VisualElement))
			{
				Property = conditionbp,
				Value = "foobar",
				Setters = {
					new Setter { Property = setterbp, Value = new Binding(".", source: "Qux") },
				}
			};

			element.SetValue(setterbp, "default");
			element.Triggers.Add(trigger);
			Assume.That(element.GetValue(setterbp), Is.EqualTo("default"));

			//sets the condition to true
			element.SetValue(conditionbp, "foobar");
			Assume.That(element.GetValue(setterbp), Is.EqualTo("Qux"));

			//unsets the condition
			element.SetValue(conditionbp, "baz");
			Assert.That(element.GetValue(setterbp), Is.EqualTo("default"));
		}
	}
}