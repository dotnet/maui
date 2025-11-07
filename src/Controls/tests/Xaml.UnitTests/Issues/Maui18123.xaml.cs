using System;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

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

	public class Test
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void MultiBindingShouldNotThrow(XamlInflator inflator)
		{
			var page = new Maui18123(inflator);
			page.BindingContext = new Maui18123VM();
			page.editBtn.SendClicked();
			Assert.Equal("SUBMIT", page.editBtn.Text);
			Assert.Equal("CANCEL", page.deleteBtn.Text);
			page.deleteBtn.SendClicked();
			Assert.Equal("Edit", page.editBtn.Text);
			Assert.Equal("Delete", page.deleteBtn.Text);
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
