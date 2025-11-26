using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh5510VM : INotifyPropertyChanged
{
	private string name = "Bill";
	private Dictionary<string, string> errors;

	public Gh5510VM()
	{
		errors = new Dictionary<string, string>
		{
			{ nameof(Name), "An error" }
		};
	}

	public string Name
	{
		get => name;
		set => SetProperty(ref name, value);
	}

	public Dictionary<string, string> Errors
	{
		get => errors;
		private set => SetProperty(ref errors, value);
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public void ClearErrorForPerson() => Errors = new Dictionary<string, string>();

	protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
	{
		if (Equals(field, value))
			return false;
		field = value;
		RaisePropertyChanged(propertyName);
		return true;
	}

	protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);
}

public partial class Gh5510 : ContentPage
{
	public Gh5510() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void CompileBindingWithIndexer([Values] XamlInflator inflator)
		{
			var vm = new Gh5510VM();
			var layout = new Gh5510(inflator) { BindingContext = vm };
			Assert.That(layout.entry.TextColor, Is.EqualTo(Colors.Red));
			vm.ClearErrorForPerson();
			Assert.That(layout.entry.TextColor, Is.EqualTo(Colors.Black));
		}
	}
}
