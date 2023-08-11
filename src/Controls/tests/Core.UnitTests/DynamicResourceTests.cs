using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class DynamicResourceTests : BaseTestFixture
	{
		public DynamicResourceTests()
		{
			Application.Current = new MockApplication();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Application.Current = null;
			}

			base.Dispose(disposing);
		}

		[Fact]
		public void TestDynamicResourceOverride()
		{
			Application.Current.Resources = new ResourceDictionary
			{
				{ "GreenColor", Colors.Green },
				{ "RedColor", Colors.Red }
			};

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

			Application.Current.LoadPage(new ContentPage
			{
				Content = new StackLayout
				{
					Children = { label }
				}
			});

			Assert.Equal(Colors.Green, label.TextColor);
		}

		[Fact]
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

			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);

			layout.Resources = new ResourceDictionary {
				{ "foo", "FOO" }
			};
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void SetResourceTriggerSetValue()
		{
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
			label.Resources = new ResourceDictionary {
				{"foo", "FOO"}
			};
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void SetResourceOnParentTriggerSetValue()
		{
			var label = new Label();
			var layout = new StackLayout { Children = { label } };
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
			layout.Resources = new ResourceDictionary {
				{"foo", "FOO"}
			};
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void SettingResourceTriggersValueChanged()
		{
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
			label.Resources = new ResourceDictionary();
			label.Resources.Add("foo", "FOO");
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void AddingAResourceDictionaryTriggersValueChangedForExistingValues()
		{
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
			var rd = new ResourceDictionary { { "foo", "FOO" } };
			label.Resources = rd;
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void ValueChangedTriggeredOnSubscribeIfKeyAlreadyExists()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary { { "foo", "FOO" } };
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void RemoveDynamicResourceStopsUpdating()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary { { "foo", "FOO" } };
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal("FOO", label.Text);
			label.RemoveDynamicResource(Label.TextProperty);
			label.Resources["foo"] = "BAR";
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void ReparentResubscribe()
		{
			var layout0 = new ContentView { Resources = new ResourceDictionary { { "foo", "FOO" } } };
			var layout1 = new ContentView { Resources = new ResourceDictionary { { "foo", "BAR" } } };

			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);

			layout0.Content = label;
			Assert.Equal("FOO", label.Text);

			layout0.Content = null;
			layout1.Content = label;
			Assert.Equal("BAR", label.Text);
		}

		[Fact]
		public void ClearedResourcesDoesNotClearValues()
		{
			var layout0 = new ContentView { Resources = new ResourceDictionary { { "foo", "FOO" } } };
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			layout0.Content = label;

			Assert.Equal("FOO", label.Text);

			layout0.Resources.Clear();
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		//Issue 2608
		public void ResourcesCanBeChanged()
		{
			var label = new Label();
			label.BindingContext = new MockViewModel();
			label.SetBinding(Label.TextProperty, "Text", BindingMode.TwoWay);
			label.SetDynamicResource(Label.TextProperty, "foo");
			label.Resources = new ResourceDictionary { { "foo", "FOO" } };

			Assert.Equal("FOO", label.Text);

			label.Resources["foo"] = "BAR";

			Assert.Equal("BAR", label.Text);
		}

		[Fact]
		public void FallbackToApplicationCurrent()
		{
			Application.Current.Resources = new ResourceDictionary { { "foo", "FOO" } };

			var label = new Label();
			label.BindingContext = new MockViewModel();
			label.SetBinding(Label.TextProperty, "Text", BindingMode.TwoWay);
			label.SetDynamicResource(Label.TextProperty, "foo");

			Assert.Equal("FOO", label.Text);
		}
	}
}