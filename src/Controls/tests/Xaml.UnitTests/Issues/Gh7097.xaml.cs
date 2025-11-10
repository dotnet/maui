using System;
using System.Windows.Input;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh7097 : Gh7097Base
{
	public Gh7097() => InitializeComponent();


	public class Tests : IDisposable
	{
		public Tests()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[Values]
		public void CanXReferenceRoot(XamlInflator inflator)
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
			Assert.IsType<MockCommand>(btn1.Command);
		}

		[Theory]
		[Values]
		//this was later reported as https://github.com/xamarin/Microsoft.Maui.Controls/issues/7286
		public void RegisteringXNameOnSubPages(XamlInflator inflator)
		{
			var layout = new Gh7097(inflator);
			var s = layout.FindByName("self");
			Assert.NotNull(layout.self);
			Assert.NotNull(layout.collectionview);
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
