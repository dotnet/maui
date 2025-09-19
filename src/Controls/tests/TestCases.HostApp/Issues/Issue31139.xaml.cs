using System.ComponentModel;
using System.Diagnostics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31139, "Binding updates from background thread should marshal to UI thread", PlatformAffected.All)]
public partial class Issue31139 : ContentPage
{
	public Issue31139()
	{
		InitializeComponent();
		BindingContext = new Issue31139ViewModel();
	}
}

public class Issue31139ViewModel : INotifyPropertyChanged
{
	bool flag = true;
	public Issue31139ViewModel()
	{
		Thread t = new Thread(() =>
		{
			var stopwatch = new Stopwatch();
			stopwatch.Restart();

			while (true)
			{
				try
				{
					if (flag)
					{
						Status = "Test";
						flag = false;
					}
					else
					{
						Status = "Success";
						break;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}

				Thread.Sleep(500);
			}
		});

		t.IsBackground = true;
		t.Start();
	}

	private string _status = string.Empty;

	public string Status
	{
		get => _status;
		set
		{
			if (_status != value)
			{
				_status = value;
				OnPropertyChanged(nameof(Status));
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}