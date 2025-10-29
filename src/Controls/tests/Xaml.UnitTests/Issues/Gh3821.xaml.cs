using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	//[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh3821 : ContentPage
	{
		public Gh3821()
		{
			InitializeComponent();
		}

		public Gh3821(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		string _text;
		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				OnPropertyChanged();
			}
		}		class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[InlineData(true), InlineData(false)]
			public void NoConflictsInNamescopes(bool useCompiledXaml)
			{
				var layout = new Gh3821(useCompiledXaml) { Text = "root" };
				var view = ((Gh3821View)((StackLayout)layout.Content).Children[0]);
				var label0 = ((Label)((Gh3821View)((StackLayout)layout.Content).Children[0]).Content);
				Assert.Equal("root", label0.Text);
			}
		}
	}
}