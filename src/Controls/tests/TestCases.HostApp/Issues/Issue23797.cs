using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23797, "Binding context in ContentPresenter", PlatformAffected.All)]
public class Issue23797 : ContentPage
{
    public static Issue23797_ViewModel Issue23797_ViewModel { get; } = new Issue23797_ViewModel();

    public Issue23797()
    {
        Title = "Issue23797";

        var stackLayout = new StackLayout
        {
            Spacing = 20,
            Padding = 20
        };

        var label = new Label
        {
            FontSize = 16,
            BackgroundColor = Colors.LightGray,
            Padding = 10,
            AutomationId = "CurrentMessageLabel"
        };
        label.SetBinding(Label.TextProperty, new Binding("Message", source: Issue23797_ViewModel));
        stackLayout.Children.Add(label);

        var customControl = new CustomControlWithCustomContent
        {
            BindingContext = Issue23797_ViewModel
        };

        var button = new Button
        {
            Text = "Click to change to 'success'",
            BackgroundColor = Colors.LightBlue,
            AutomationId = "Issue23797Btn"
        };
        button.SetBinding(Button.CommandProperty, new Binding("TestCommand"));

        customControl.MyContent = button;
        stackLayout.Children.Add(customControl);

        Content = stackLayout;
    }
}

public class CustomControlWithCustomContent : ContentView
{
    public static readonly BindableProperty MyContentProperty = BindableProperty.Create(
        nameof(MyContent),
        typeof(View),
        typeof(CustomControlWithCustomContent),
        null);

    public View MyContent
    {
        get => (View)GetValue(MyContentProperty);
        set => SetValue(MyContentProperty, value);
    }

    public CustomControlWithCustomContent()
    {
        ControlTemplate = new ControlTemplate(() =>
        {
            var border = new Border
            {
                Stroke = Colors.Red,
                StrokeThickness = 2,
                Padding = 10
            };

            var contentPresenter = new ContentPresenter();
            contentPresenter.SetBinding(ContentPresenter.ContentProperty, new Binding(nameof(MyContent), source: RelativeBindingSource.TemplatedParent));

            border.Content = contentPresenter;
            return border;
        });
    }
}

public class Issue23797_ViewModel : INotifyPropertyChanged
{
    private string _message = "failure";

    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            OnPropertyChanged();
        }
    }

    public ICommand TestCommand { get; }

    public Issue23797_ViewModel()
    {
        TestCommand = new Command(() =>
        {
            Message = "success";
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}