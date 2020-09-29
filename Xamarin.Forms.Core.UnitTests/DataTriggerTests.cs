using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class DataTriggerTests : BaseTestFixture
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
			Assert.AreEqual("default", element.GetValue(setterbp));
			element.Triggers.Add(datatrigger);
			Assert.AreEqual("qux", element.GetValue(setterbp));
		}

		[Test]
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

			Assert.AreEqual("default", element.GetValue(setterbp));
			element.BindingContext = new { foo = "foobar" };
			Assert.AreEqual("qux", element.GetValue(setterbp));
			element.Triggers.Remove(datatrigger);
			Assert.AreEqual("default", element.GetValue(setterbp));
		}

		[Test]
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

			Assert.AreEqual("default", element.GetValue(setterbp));
			element.BindingContext = new { foo = "foobar" };
			Assert.AreEqual("qux", element.GetValue(setterbp));
			element.BindingContext = new { foo = "" };
			Assert.AreEqual("default", element.GetValue(setterbp));
		}

		[Test]
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
			Assert.AreEqual("qux", element0.GetValue(setterbp));
			Assert.AreEqual("qux", element1.GetValue(setterbp));
		}

		[Test]
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
							new Setter { Property = BoxView.ColorProperty, Value = Color.Green },
							new Setter { Property = VisualElement.OpacityProperty, Value = .5 },
						}
					},
					new DataTrigger (typeof(BoxView)) {
						Binding = new Binding ("."),
						Value = "MissingInfo",
						Setters = {
							new Setter { Property = BoxView.ColorProperty, Value = Color.Yellow },
						}
					},
					new DataTrigger (typeof(BoxView)) {
						Binding = new Binding ("."),
						Value = "Error",
						Setters = {
							new Setter { Property = BoxView.ColorProperty, Value = Color.Red },
						}
					},
				}
			};

			boxview.BindingContext = "Complete";
			Assert.AreEqual(Color.Green, boxview.Color);
			Assert.AreEqual(.5, boxview.Opacity);

			boxview.BindingContext = "MissingInfo";
			Assert.AreEqual(Color.Yellow, boxview.Color);
			Assert.AreEqual(1, boxview.Opacity);

			boxview.BindingContext = "Error";
			Assert.AreEqual(Color.Red, boxview.Color);
			Assert.AreEqual(1, boxview.Opacity);

			boxview.BindingContext = "Complete";
			Assert.AreEqual(Color.Green, boxview.Color);
			Assert.AreEqual(.5, boxview.Opacity);
		}
	}
}