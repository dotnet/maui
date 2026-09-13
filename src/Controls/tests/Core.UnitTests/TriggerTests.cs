using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class TriggerTests : BaseTestFixture
	{
		class MockElement : VisualElement
		{
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void TriggerSpecificityIsAllocatedOnAttachAndReused(bool initiallyActive)
		{
			var element = new MockElement { IsEnabled = !initiallyActive };
			var trigger = new Trigger(typeof(VisualElement))
			{
				Property = VisualElement.IsEnabledProperty,
				Value = false,
				Setters = {
					new Setter { Property = VisualElement.ScaleProperty, Value = 2d },
				}
			};

			Assert.Null(element._triggerSpecificity);
			element.Triggers.Add(trigger);
			var specificities = element._triggerSpecificity;
			Assert.NotNull(specificities);
			Assert.Same(trigger, Assert.Single(specificities).Key);
			Assert.Equal(initiallyActive ? 2d : 1d, element.Scale);

			element.IsEnabled = true;
			Assert.Equal(1d, element.Scale);
			element.IsEnabled = false;
			Assert.Equal(2d, element.Scale);
			Assert.Same(specificities, element._triggerSpecificity);

			element.Triggers.Remove(trigger);
			Assert.Empty(specificities);
			Assert.Equal(1d, element.Scale);
			element.IsEnabled = true;
			element.IsEnabled = false;
			Assert.Equal(1d, element.Scale);

			element.Triggers.Add(trigger);
			Assert.Same(specificities, element._triggerSpecificity);
			Assert.Same(trigger, Assert.Single(specificities).Key);
			Assert.Equal(2d, element.Scale);

			element.Triggers.Clear();
			Assert.Same(specificities, element._triggerSpecificity);
			Assert.Empty(specificities);
			Assert.Equal(1d, element.Scale);
		}

		[Fact]
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

			Assert.Equal("default", element.GetValue(setterbp));
			element.SetValue(conditionbp, "foobar");
			Assert.Equal("qux", element.GetValue(setterbp));
			element.SetValue(conditionbp, "");
			Assert.Equal("default", element.GetValue(setterbp));
		}

		[Fact]
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
			Assert.Equal("default", element.GetValue(setterbp));
			element.Triggers.Add(trigger);
			Assert.Equal("qux", element.GetValue(setterbp));
		}

		[Fact]
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

			Assert.Equal("default", element.GetValue(setterbp));
			element.SetValue(conditionbp, "foobar");
			Assert.Equal("qux", element.GetValue(setterbp));
			element.Triggers.Remove(trigger);
			Assert.Equal("default", element.GetValue(setterbp));
		}

		[Fact]
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

		[Fact]
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
			Assert.Equal("default", element.GetValue(setterbp));

			//sets the condition to true
			element.SetValue(conditionbp, "foobar");
			Assert.Equal("Qux", element.GetValue(setterbp));

			//unsets the condition
			element.SetValue(conditionbp, "baz");
			Assert.Equal("default", element.GetValue(setterbp));
		}
	}
}