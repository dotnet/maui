using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui32398 : ContentPage
{
	public Maui32398() => InitializeComponent();
	
	public readonly BindableProperty NonStaticProperty =
		BindableProperty.Create(nameof(NonStatic), typeof(string), typeof(Maui32398), default(string));
	public string NonStatic
	{
		get;set;
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

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void NonStaticBP([Values] XamlInflator inflator)
		{
			var page = new Maui32398(inflator);
			Assert.AreEqual("foo", page.NonStatic);
			Assert.AreNotEqual("foo", page.GetValue(page.NonStaticProperty));
		}
	}
}