using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 13558, "Picker values are not displaying when loaded within a ListView", PlatformAffected.Android)]

public partial class Issue13558 : ContentPage
{
	public ObservableCollection<List<string>> Items { get; set; }

	MainViewModel _mainViewModel;
	public Issue13558()
	{
		InitializeComponent();
		_mainViewModel = new MainViewModel();
		BindingContext = _mainViewModel;
	}
}

public class BloodworkStatus : INotifyPropertyChanged
{
	public int Id { get; set; }
	public string Name { get; set; }

	public event PropertyChangedEventHandler PropertyChanged;

	public override string ToString()
	{
		return Name!.ToString();
	}

	private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

public class MainViewModel : INotifyPropertyChanged
{
	public ObservableCollection<Patient> Patients { get; set; }
	public ObservableCollection<BloodworkStatus> BloodworkStatuses { get; set; }

	public MainViewModel()
	{
		Patients = new ObservableCollection<Patient>();
		BloodworkStatuses = new ObservableCollection<BloodworkStatus>();

		BloodworkStatus status = new BloodworkStatus();
		status.Id = 1;
		status.Name = "Not started";
		BloodworkStatuses.Add(status);

		status = new BloodworkStatus();
		status.Id = 2;
		status.Name = "In Progress";
		BloodworkStatuses.Add(status);

		status = new BloodworkStatus();
		status.Id = 3;
		status.Name = "Completed";
		BloodworkStatuses.Add(status);

		var selectedstatus = BloodworkStatuses[1];

		Patient patient = new Patient();
		patient.Id = 1;
		patient.Name = "John Nameless";
		patient.MedicalHistory = "Loves to develop MAUI :/ ";
		patient.BloodworkStatus = BloodworkStatuses[2];
		Patients.Add(patient);

		patient = new Patient();
		patient.Id = 2;
		patient.Name = "Patrick Schlover";
		patient.MedicalHistory = "Hail Microsoft";
		patient.BloodworkStatus = BloodworkStatuses[0];
		Patients.Add(patient);

		patient = new Patient();
		patient.Id = 2;
		patient.Name = "Layla McKanzi";
		patient.MedicalHistory = "Seeking for a Mecnun";
		patient.BloodworkStatus = BloodworkStatuses[0];
		Patients.Add(patient);
	}

	public event PropertyChangedEventHandler PropertyChanged;

	private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

public class Patient : INotifyPropertyChanged
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string MedicalHistory { get; set; }
	public BloodworkStatus BloodworkStatus { get; set; }

	public event PropertyChangedEventHandler PropertyChanged;

	private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}