using System.Windows.Input;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh5256Entry : Entry
	{
		public Gh5256Entry()
		{
			base.Completed += (o, e) => this.Completed?.Execute(o);
		}
		public static readonly BindableProperty CompletedProperty =
			BindableProperty.Create("Completed", typeof(ICommand), typeof(Gh5256Entry), default(ICommand));

		public new ICommand Completed
		{
			get => (ICommand)GetValue(CompletedProperty);
			set => SetValue(CompletedProperty, value);
		}
	}

	//[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh5256 : ContentPage
	{
		public Gh5256() => InitializeComponent();
		public Gh5256(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[Theory]
			public void Method(bool useCompiledXaml)
			{
				var layout = new Gh5256(useCompiledXaml) { BindingContext = new { CompletedCommand = new Command(() => Assert.Pass()) } };
				layout.entry.SendCompleted();
				throw new Xunit.Sdk.XunitException();
			}
		}

	}
}
