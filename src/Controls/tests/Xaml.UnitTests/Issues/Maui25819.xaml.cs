using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Mono.Cecil.Cil;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;
public partial class Maui25819 : ContentPage
{
	public Maui25819()
	{
		InitializeComponent();
	}

	public Maui25819(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
		}


		[Test]
		public void DoesntThrow([Values] bool inflator)
		{
			MockCompiler.Compile(typeof(Maui25819));
			var layout = new Maui25819(inflator);
			layout.BindingContext = new Maui25819UserDataViewModel();
			Assert.That(layout.label0.Text, Is.EqualTo("2023"));
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