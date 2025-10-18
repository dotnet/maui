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
	}
}