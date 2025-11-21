using System.ComponentModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28497, "[iOS] Fix Crash When Using ShellContent with DataTemplate and Binding", PlatformAffected.iOS)]
public partial class Issue28497 : Shell
{
	FlyoutpageViewModel model;
	public Issue28497()
	{
		InitializeComponent();
		model = new FlyoutpageViewModel();
		BindingContext = model;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(FlyoutIsPresented) && FlyoutIsPresented)
                {
                    model.Activate();
                }
            };
	}
}

public class FlyoutpageViewModel : INotifyPropertyChanged
    {
		int myVar;
		public int Counter
		{
			get { return myVar; }
			set 
            { 
				myVar = value; 
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Counter"));
            }
		}

        public event PropertyChangedEventHandler PropertyChanged;

        public void Activate()
        {
            Counter = Counter + 1;
        }
    }
