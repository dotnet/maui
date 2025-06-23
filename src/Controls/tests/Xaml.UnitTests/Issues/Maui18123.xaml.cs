using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Maui18123MultiValueConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		=> values?.ToArray();

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}

public partial class Maui18123 : ContentPage
{

	public Maui18123() => InitializeComponent();

	public Maui18123(bool useCompiledXaml)
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

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void MultiBindingShouldNotThrow([Values(false, true)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Maui18123)));

			var page = new Maui18123(useCompiledXaml);
			page.BindingContext = new Maui18123VM();
			page.editBtn.SendClicked();
			Assert.That(page.editBtn.Text, Is.EqualTo("SUBMIT"));
			Assert.That(page.deleteBtn.Text, Is.EqualTo("CANCEL"));
			page.deleteBtn.SendClicked();
			Assert.That(page.editBtn.Text, Is.EqualTo("Edit"));
			Assert.That(page.deleteBtn.Text, Is.EqualTo("Delete"));
		}
	}
}

public partial class Maui18123VM : BindableObject
{
	string _mode = "";
	public string Mode
	{
		get { return _mode; }
		set
		{
			_mode = value;
			OnPropertyChanged();
		}
	}

	Command testCommand;
	public Command TestCommand => testCommand ??= new Command(Test);

	void Test(object parameter)
	{
		var value = string.Empty;

		if (parameter is object[] parameters)
			value = parameters.LastOrDefault()?.ToString();
		else
			value = parameter.ToString();

		Mode = value;
	}
}