using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25947_2
{
	public Maui25947_2()
	{
		InitializeComponent();
	}

	public Maui25947_2(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
		}

		[Test]
		public void TestCustomTemplatedView([Values(false, true)] bool useCompiledXaml)
		{
			var page = new Maui25947_2(useCompiledXaml);

			var stackLayout = page.CustomView.Children[0];
			Assert.IsInstanceOf<StackLayout>(stackLayout);
			Assert.AreEqual(1, ((StackLayout)stackLayout).Children.Count);

			var contentPresenter = ((StackLayout)stackLayout).Children[0];
			Assert.IsInstanceOf<ContentPresenter>(contentPresenter);
			Assert.AreEqual(1, ((ContentPresenter)contentPresenter).Children.Count);

			var label = ((ContentPresenter)contentPresenter).Children[0];
			Assert.IsInstanceOf<Label>(label);
			Assert.AreEqual("Hello, Issue 25947.", ((Label)label).Text);
		}
	}
}

[ContentProperty(nameof(Content))]
public sealed class CustomTemplatedView25947 : TemplatedView
{
	public CustomTemplatedView25947()
	{
		ControlTemplate = new ControlTemplate(() => new StackLayout { new ContentPresenter() });
	}

	public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(CustomTemplatedView25947), null);

	public View Content
	{
		get => (View)GetValue(ContentProperty);
		set => SetValue(ContentProperty, value);
	}
}
