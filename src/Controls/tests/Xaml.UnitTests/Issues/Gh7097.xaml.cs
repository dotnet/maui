using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh7097 : Gh7097Base
	{
		public Gh7097() => InitializeComponent();
		public Gh7097(bool useCompiledXaml) : base(useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
			// [SetUp]
			[Xunit.Fact]
			public void Setup()
			{
				DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			}

			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
			// [TearDown]
			[Xunit.Fact]
			public void TearDown()
			{
				DispatcherProvider.SetCurrent(null);
			}

			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void Method(bool useCompiledXaml)
			{
				var layout = new Gh7097(useCompiledXaml)
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

			[Fact]
			//this was later reported as https://github.com/xamarin/Microsoft.Maui.Controls/issues/7286
			public void RegisteringXNameOnSubPages(bool useCompiledXaml)
			{
				var layout = new Gh7097(useCompiledXaml);
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
}
