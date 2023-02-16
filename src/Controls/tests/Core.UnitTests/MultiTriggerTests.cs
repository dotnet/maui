using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class MultiTriggerTests : BaseTestFixture
	{
		class MockElement : VisualElement
		{
		}

		[Fact]
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
			Assert.Equal("default", element.GetValue(setterbp));
			element.Triggers.Add(multiTrigger);
			Assert.Equal("qux", element.GetValue(setterbp));
		}

		[Fact]
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
			Assert.Equal("default", element.GetValue(setterbp));
			element.Triggers.Add(multiTrigger);
			Assert.Equal("default", element.GetValue(setterbp));
		}

		[Fact]
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
			Assert.Equal("default", element.GetValue(setterbp)); //both conditions false

			element.SetValue(conditionbp, "foobar");
			Assert.Equal("default", element.GetValue(setterbp)); //one condition false

			element.BindingContext = new { baz = "foobaz" };
			Assert.Equal("qux", element.GetValue(setterbp)); //both condition true
			element.Triggers.Remove(multiTrigger);
			Assert.Equal("default", element.GetValue(setterbp));
		}

		[Fact]
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
			Assert.Equal("default", element.GetValue(setterbp)); //both conditions false

			element.SetValue(conditionbp, "foobar");
			Assert.Equal("default", element.GetValue(setterbp)); //one condition false

			element.BindingContext = new { baz = "foobaz" };
			Assert.Equal("qux", element.GetValue(setterbp)); //both condition true

			element.BindingContext = new { baz = "" };
			Assert.Equal("default", element.GetValue(setterbp)); //one condition false

			element.BindingContext = new { baz = "foobaz" };
			Assert.Equal("qux", element.GetValue(setterbp)); //both condition true

			element.SetValue(conditionbp, "");
			Assert.Equal("default", element.GetValue(setterbp)); //one condition false

			element.SetValue(conditionbp, "foobar");
			Assert.Equal("qux", element.GetValue(setterbp)); //both condition true

			element.SetValue(conditionbp, "");
			element.BindingContext = new { baz = "foobaz" };
			Assert.Equal("default", element.GetValue(setterbp)); //both conditions false
		}
	}
}