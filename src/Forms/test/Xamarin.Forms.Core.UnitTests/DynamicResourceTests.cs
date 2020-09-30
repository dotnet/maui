using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class DynamicResourceTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
			Application.Current = new MockApplication();
		}

		[TearDown]
		public override void TearDown()
		{
			Application.Current = null;
		}

		[Test]
		public void TestDynamicResourceOverride()
		{
			Application.Current.Resources = new ResourceDictionary();
			Application.Current.Resources.Add("GreenColor", Color.Green);
			Application.Current.Resources.Add("RedColor", Color.Red);

			var setter = new Setter()
			{
				Property = Label.TextColorProperty,
				Value = new DynamicResource("RedColor")
			};
			var style = new Style(typeof(Label));
			style.Setters.Add(setter);
			Application.Current.Resources.Add(style);

			var label = new Label()
			{
				Text = "Green = :)"
			};
			label.SetDynamicResource(Label.TextColorProperty, "GreenColor");

			Application.Current.MainPage = new ContentPage
			{
				Content = new StackLayout
				{
					Children = { label }
				}
			};

			Assert.AreEqual(Color.Green, label.TextColor);
		}

		[Test]
		public void TestDynamicResource()
		{
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			var layout = new StackLayout
			{
				Children = {
					label
				}
			};

			Assert.AreEqual(Label.TextProperty.DefaultValue, label.Text);

			layout.Resources = new ResourceDictionary {
				{ "foo", "FOO" }
			};
			Assert.AreEqual("FOO", label.Text);
		}

		[Test]
		public void SetResourceTriggerSetValue()
		{
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.AreEqual(Label.TextProperty.DefaultValue, label.Text);
			label.Resources = new ResourceDictionary {
				{"foo", "FOO"}
			};
			Assert.AreEqual("FOO", label.Text);
		}

		[Test]
		public void SetResourceOnParentTriggerSetValue()
		{
			var label = new Label();
			var layout = new StackLayout { Children = { label } };
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.AreEqual(Label.TextProperty.DefaultValue, label.Text);
			layout.Resources = new ResourceDictionary {
				{"foo", "FOO"}
			};
			Assert.AreEqual("FOO", label.Text);
		}

		[Test]
		public void SettingResourceTriggersValueChanged()
		{
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.AreEqual(Label.TextProperty.DefaultValue, label.Text);
			label.Resources = new ResourceDictionary();
			label.Resources.Add("foo", "FOO");
			Assert.AreEqual("FOO", label.Text);
		}

		[Test]
		public void AddingAResourceDictionaryTriggersValueChangedForExistingValues()
		{
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.AreEqual(Label.TextProperty.DefaultValue, label.Text);
			var rd = new ResourceDictionary { { "foo", "FOO" } };
			label.Resources = rd;
			Assert.AreEqual("FOO", label.Text);
		}

		[Test]
		public void ValueChangedTriggeredOnSubscribeIfKeyAlreadyExists()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary { { "foo", "FOO" } };
			Assert.AreEqual(Label.TextProperty.DefaultValue, label.Text);
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.AreEqual("FOO", label.Text);
		}

		[Test]
		public void RemoveDynamicResourceStopsUpdating()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary { { "foo", "FOO" } };
			Assert.AreEqual(Label.TextProperty.DefaultValue, label.Text);
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.AreEqual("FOO", label.Text);
			label.RemoveDynamicResource(Label.TextProperty);
			label.Resources["foo"] = "BAR";
			Assert.AreEqual("FOO", label.Text);
		}

		[Test]
		public void ReparentResubscribe()
		{
			var layout0 = new ContentView { Resources = new ResourceDictionary { { "foo", "FOO" } } };
			var layout1 = new ContentView { Resources = new ResourceDictionary { { "foo", "BAR" } } };

			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.AreEqual(Label.TextProperty.DefaultValue, label.Text);

			layout0.Content = label;
			Assert.AreEqual("FOO", label.Text);

			layout0.Content = null;
			layout1.Content = label;
			Assert.AreEqual("BAR", label.Text);
		}

		[Test]
		public void ClearedResourcesDoesNotClearValues()
		{
			var layout0 = new ContentView { Resources = new ResourceDictionary { { "foo", "FOO" } } };
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			layout0.Content = label;

			Assert.AreEqual("FOO", label.Text);

			layout0.Resources.Clear();
			Assert.AreEqual("FOO", label.Text);
		}

		[Test]
		//Issue 2608
		public void ResourcesCanBeChanged()
		{
			var label = new Label();
			label.BindingContext = new MockViewModel();
			label.SetBinding(Label.TextProperty, "Text", BindingMode.TwoWay);
			label.SetDynamicResource(Label.TextProperty, "foo");
			label.Resources = new ResourceDictionary { { "foo", "FOO" } };

			Assume.That(label.Text, Is.EqualTo("FOO"));

			label.Resources["foo"] = "BAR";

			Assert.AreEqual("BAR", label.Text);
		}

		[Test]
		public void FallbackToApplicationCurrent()
		{
			Application.Current.Resources = new ResourceDictionary { { "foo", "FOO" } };

			var label = new Label();
			label.BindingContext = new MockViewModel();
			label.SetBinding(Label.TextProperty, "Text", BindingMode.TwoWay);
			label.SetDynamicResource(Label.TextProperty, "foo");

			Assert.That(label.Text, Is.EqualTo("FOO"));
		}
	}
}