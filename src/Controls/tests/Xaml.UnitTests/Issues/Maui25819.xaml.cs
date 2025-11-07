using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25819 : ContentPage
{
	public Maui25819() => InitializeComponent();

	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void DoesntThrow(XamlInflator inflator)
		{
			MockCompiler.Compile(typeof(Maui25819));
			var layout = new Maui25819(inflator);
			layout.BindingContext = new Maui25819UserDataViewModel();
			Assert.Equal("2023", layout.label0.Text);
		}
	}
}

public partial class Maui25819UserDataViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public Maui25819UsersDataModel UsersData { get; set; }
	public Maui25819UserDataViewModel()
	{
		UsersData = new Maui25819UsersDataModel
		{
			InstalmentPlans = new Maui25819InstalmentPlans
			{
				InstalmentPlan = new[]
				{
					new Maui25819InstalmentPlan { FinancialYear = "2023" }
				}
			}
		};
	}
}

public class Maui25819UsersDataModel
{
	public Maui25819InstalmentPlans InstalmentPlans { get; set; } = new();
}

public class Maui25819InstalmentPlans
{
	public Maui25819InstalmentPlan[] InstalmentPlan { get; set; } = [];
}

public class Maui25819InstalmentPlan
{
	public string FinancialYear { get; set; } = string.Empty;
}
