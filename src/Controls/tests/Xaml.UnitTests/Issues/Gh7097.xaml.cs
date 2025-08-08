using System;
using System.Windows.Input;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh7097 : Gh7097Base
{
	public Gh7097() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void CanXReferenceRoot([Values] XamlInflator inflator)
		{
			var layout = new Gh7097(inflator)
			{
				BindingContext = new
				{
					Button1Command = new MockCommand(),
					Button2Command = new MockCommand(),
				}
			};
			var cv = layout.Content as CollectionView;
			var content = cv.ItemTemplate.CreateContent() as StackLayout;
			var btn1 = content.Children[0] as Button;
			Assert.That(btn1.Command, Is.TypeOf<MockCommand>());
		}

		[Test]
		//this was later reported as https://github.com/xamarin/Microsoft.Maui.Controls/issues/7286
		public void RegisteringXNameOnSubPages([Values] XamlInflator inflator)
		{
			var layout = new Gh7097(inflator);
			var s = layout.FindByName("self");
			Assert.That(layout.self, Is.Not.Null);
			Assert.That(layout.collectionview, Is.Not.Null);
		}

		class MockCommand : ICommand
		{
#pragma warning disable 0067
			public event EventHandler CanExecuteChanged;
#pragma warning restore 0067
			public bool CanExecute(object parameter) => true;
			public void Execute(object parameter) => throw new NotImplementedException();
		}
	}
}
