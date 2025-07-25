using System.Windows.Input;
using NUnit.Framework;

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

[XamlProcessing(XamlInflator.Default, true)]
public partial class Gh5256 : ContentPage
{
	public Gh5256() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void EventOverriding([Values] XamlInflator inflator)
		{
			var layout = new Gh5256(inflator) { BindingContext = new { CompletedCommand = new Command(() => Assert.Pass()) } };
			layout.entry.SendCompleted();
			Assert.Fail();
		}
	}

}
