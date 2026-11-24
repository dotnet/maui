using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class DataTriggerTests : BaseTestFixture
	{
		class MockElement : VisualElement
		{
		}

		[Fact]
		public void SettersAppliedOnAttachIfConditionIsTrue()
		{
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var datatrigger = new DataTrigger(typeof(VisualElement))
			{
				Binding = new Binding("foo"),
				Value = "foobar",
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				}
			};

			element.SetValue(setterbp, "default");
			element.BindingContext = new { foo = "foobar" };
			Assert.Equal("default", element.GetValue(setterbp));
			element.Triggers.Add(datatrigger);
			Assert.Equal("qux", element.GetValue(setterbp));
		}

		[Fact]
		public void SettersUnappliedOnDetach()
		{
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var datatrigger = new DataTrigger(typeof(VisualElement))
			{
				Binding = new Binding("foo"),
				Value = "foobar",
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				}
			};

			element.SetValue(setterbp, "default");
			element.Triggers.Add(datatrigger);

			Assert.Equal("default", element.GetValue(setterbp));

			element.BindingContext = new { foo = "foobar" };
			Assert.Equal("qux", element.GetValue(setterbp));
			element.Triggers.Remove(datatrigger);
			Assert.Equal("default", element.GetValue(setterbp));
		}

		[Fact]
		public void SettersAppliedOnConditionChanged()
		{
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var trigger = new DataTrigger(typeof(VisualElement))
			{
				Binding = new Binding("foo"),
				Value = "foobar",
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				}
			};

			element.SetValue(setterbp, "default");
			element.Triggers.Add(trigger);

			Assert.Equal("default", element.GetValue(setterbp));
			element.BindingContext = new { foo = "foobar" };
			Assert.Equal("qux", element.GetValue(setterbp));
			element.BindingContext = new { foo = "" };
			Assert.Equal("default", element.GetValue(setterbp));
		}

		[Fact]
		public void TriggersAppliedOnMultipleElements()
		{
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var trigger = new DataTrigger(typeof(VisualElement))
			{
				Binding = new Binding("foo"),
				Value = "foobar",
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				}
			};
			var element0 = new MockElement { Triggers = { trigger } };
			var element1 = new MockElement { Triggers = { trigger } };

			element0.BindingContext = element1.BindingContext = new { foo = "foobar" };
			Assert.Equal("qux", element0.GetValue(setterbp));
			Assert.Equal("qux", element1.GetValue(setterbp));
		}

		[Fact]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=30074
		public void AllTriggersUnappliedBeforeApplying()
		{
			var boxview = new BoxView
			{
				Triggers = {
					new DataTrigger (typeof(BoxView)) {
						Binding = new Binding ("."),
						Value = "Complete",
						Setters = {
							new Setter { Property = BoxView.ColorProperty, Value = Colors.Green },
							new Setter { Property = VisualElement.OpacityProperty, Value = .5 },
						}
					},
					new DataTrigger (typeof(BoxView)) {
						Binding = new Binding ("."),
						Value = "MissingInfo",
						Setters = {
							new Setter { Property = BoxView.ColorProperty, Value = Colors.Yellow },
						}
					},
					new DataTrigger (typeof(BoxView)) {
						Binding = new Binding ("."),
						Value = "Error",
						Setters = {
							new Setter { Property = BoxView.ColorProperty, Value = Colors.Red },
						}
					},
				}
			};

			boxview.BindingContext = "Complete";
			Assert.Equal(Colors.Green, boxview.Color);
			Assert.Equal(.5, boxview.Opacity);

			boxview.BindingContext = "MissingInfo";
			Assert.Equal(Colors.Yellow, boxview.Color);
			Assert.Equal(1, boxview.Opacity);

			boxview.BindingContext = "Error";
			Assert.Equal(Colors.Red, boxview.Color);
			Assert.Equal(1, boxview.Opacity);

			boxview.BindingContext = "Complete";
			Assert.Equal(Colors.Green, boxview.Color);
			Assert.Equal(.5, boxview.Opacity);
		}

		// https://github.com/dotnet/maui/issues/6847
		[Fact]
		public void SettersAppliedOnAttachIfBoundValueIsInitiallyNull()
		{
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var datatrigger = new DataTrigger(typeof(VisualElement))
			{
				Binding = new Binding("foo"),
				Value = null,
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				}
			};

			element.SetValue(setterbp, "default");
			element.BindingContext = new { foo = (string)null };
			element.Triggers.Add(datatrigger);
			Assert.Equal("qux", element.GetValue(setterbp));

			element.BindingContext = new { foo = "foobar" };
			Assert.Equal("default", element.GetValue(setterbp));

			element.BindingContext = new { foo = (string)null };
			Assert.Equal("qux", element.GetValue(setterbp));

			element.Triggers.Remove(datatrigger);
			Assert.Equal("default", element.GetValue(setterbp));
		}

		// https://github.com/dotnet/maui/issues/6847
		[Fact]
		public void SettersNotUnappliedWhenNullValueTriggerNeverMatched()
		{
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var datatrigger = new DataTrigger(typeof(VisualElement))
			{
				Binding = new Binding("foo"),
				Value = "foobar",
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				}
			};

			element.SetValue(setterbp, "default");
			element.BindingContext = new { foo = (string)null };
			element.Triggers.Add(datatrigger);
			Assert.Equal("default", element.GetValue(setterbp));

			element.BindingContext = new { foo = "other" };
			Assert.Equal("default", element.GetValue(setterbp));

			element.BindingContext = new { foo = "foobar" };
			Assert.Equal("qux", element.GetValue(setterbp));
		}

		// https://github.com/dotnet/maui/issues/6847
		[Fact]
		public void MultiTriggerWithNullBindingConditionAppliedOnAttach()
		{
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var multiTrigger = new MultiTrigger(typeof(VisualElement))
			{
				Conditions = {
					new BindingCondition { Binding = new Binding("foo"), Value = null },
					new BindingCondition { Binding = new Binding("baz"), Value = "ok" },
				},
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				}
			};

			element.SetValue(setterbp, "default");
			element.BindingContext = new { foo = (string)null, baz = "ok" };
			element.Triggers.Add(multiTrigger);
			Assert.Equal("qux", element.GetValue(setterbp));
		}

		// https://github.com/dotnet/maui/issues/6847
		[Fact]
		public void RemovingNullValueTriggerWhileBoundValueIsNonNullDoesNotReapply()
		{
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			var enterAction = new CountingTriggerAction();
			var datatrigger = new DataTrigger(typeof(VisualElement))
			{
				Binding = new Binding("foo"),
				Value = null,
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				},
				EnterActions = { enterAction },
			};

			element.SetValue(setterbp, "default");
			element.BindingContext = new { foo = "foobar" };
			element.Triggers.Add(datatrigger);
			Assert.Equal("default", element.GetValue(setterbp));
			Assert.Equal(0, enterAction.InvokeCount);

			// TearDown clears the bound value (non-null -> null), which must not re-activate the removed trigger.
			element.Triggers.Remove(datatrigger);
			Assert.Equal("default", element.GetValue(setterbp));
			Assert.Equal(0, enterAction.InvokeCount);
		}

		class DelegateTriggerAction : TriggerAction<BindableObject>
		{
			readonly Action<BindableObject> _action;

			public DelegateTriggerAction(Action<BindableObject> action) => _action = action;

			protected override void Invoke(BindableObject sender) => _action(sender);
		}

		// https://github.com/dotnet/maui/issues/6847
		[Fact]
		public void ReentrantBindingChangesFromEnterActionKeepFinalState()
		{
			var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
			var element = new MockElement();
			bool fired = false;
			var datatrigger = new DataTrigger(typeof(VisualElement))
			{
				Binding = new Binding("foo"),
				Value = "on",
				Setters = {
					new Setter { Property = setterbp, Value = "qux" },
				},
				EnterActions = {
					new DelegateTriggerAction(sender =>
					{
						if (fired)
							return;
						fired = true;
						var ve = (VisualElement)sender;
						ve.BindingContext = new { foo = "off" };
						ve.BindingContext = new { foo = "on" };
					}),
				},
			};

			element.SetValue(setterbp, "default");
			element.BindingContext = new { foo = "on" };
			element.Triggers.Add(datatrigger);

			Assert.True(fired);
			Assert.Equal("qux", element.GetValue(setterbp));

			element.BindingContext = new { foo = "off" };
			Assert.Equal("default", element.GetValue(setterbp));
		}
	}
}