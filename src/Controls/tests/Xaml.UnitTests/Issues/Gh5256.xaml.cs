using System.Windows.Input;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh5256Entry : Entry
{
	public Gh5256Entry() => base.Completed += (o, e) => this.Completed?.Execute(o);
	public static readonly BindableProperty CompletedProperty =
		BindableProperty.Create(nameof(Completed), typeof(ICommand), typeof(Gh5256Entry), default(ICommand));

	public new ICommand Completed
	{
		get => (ICommand)GetValue(CompletedProperty);
		set => SetValue(CompletedProperty, value);
	}
}

public partial class Gh5256 : ContentPage
{
	public Gh5256() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void EventOverriding(XamlInflator inflator)
		{
			bool commandExecuted = false;
			var layout = new Gh5256(inflator) { BindingContext = new { CompletedCommand = new Command(() => commandExecuted = true) } };
			layout.entry.SendCompleted();
			Assert.True(commandExecuted, "CompletedCommand was not executed");
		}
	}

}
