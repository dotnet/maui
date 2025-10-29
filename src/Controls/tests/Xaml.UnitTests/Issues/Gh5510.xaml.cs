using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
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
		public Gh5510(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Theory]
			public void Method(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh5510)));

				var vm = new Gh5510VM();
				var layout = new Gh5510(useCompiledXaml) { BindingContext = vm };
				Assert.Equal(Colors.Red, layout.entry.TextColor);
				vm.ClearErrorForPerson();
				Assert.Equal(Colors.Black, layout.entry.TextColor);
			}
		}
	}
}
