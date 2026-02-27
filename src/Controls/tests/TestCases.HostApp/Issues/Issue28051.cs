using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28051, "ObjectDisposedException Occurs During View Recycling When PlatformBehavior Is Attached on Android", PlatformAffected.Android)]
public class Issue28051 : TestContentPage, INotifyPropertyChanged
{
	ObservableCollection<string> items;

	public ObservableCollection<string> Items
	{
		get => items;
		set
		{
			if (items != value)
			{
				items = value;
				OnPropertyChanged();
			}
		}
	}

	protected override void Init()
	{
		BindingContext = this;
		LoadItems();

		Button refreshItemsButton = new Button
		{
			AutomationId = "RefreshItemsButton",
			Text = "Refresh"
		};
		refreshItemsButton.Clicked += OnReproClicked;

		StackLayout contentLayout = new StackLayout() { Spacing = 10 };

		StackLayout itemContainer = new StackLayout();
		itemContainer.SetBinding(BindableLayout.ItemsSourceProperty, new Binding("Items"));

		BindableLayout.SetItemTemplate(itemContainer, CreateItemTemplate());

		RefreshView refreshView = new RefreshView
		{
			Content = itemContainer
		};

		contentLayout.Children.Add(refreshItemsButton);
		contentLayout.Children.Add(refreshView);

		this.Content = contentLayout;
	}

	DataTemplate CreateItemTemplate()
	{
		DataTemplate itemTemplate = new DataTemplate(() =>
		{
			Button button = new Button
			{
				BackgroundColor = Colors.DarkGreen,
				TextColor = Colors.White
			};
			button.SetBinding(Button.TextProperty, new Binding("."));
			LongPressBehavior longPressBehavior = new LongPressBehavior();
			button.Behaviors.Add(longPressBehavior);
			return button;
		});

		return itemTemplate;
	}

	void OnReproClicked(object sender, EventArgs e)
	{
		Items = null;
		Items = new ObservableCollection<string>
			{
				"Item A",
				"Item B",
				"Item C"
			};
	}

	void LoadItems()
	{
		Items = new ObservableCollection<string>
			{
				"Item 1",
				"Item 2",
				"Item 3"
			};
	}

	public new event PropertyChangedEventHandler PropertyChanged;
	protected new void OnPropertyChanged([CallerMemberName] string propertyName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

#if ANDROID
public class LongPressBehavior : PlatformBehavior<Element, Android.Views.View>
{
    protected override void OnAttachedTo(Element bindable, Android.Views.View platformView)
    {
        BindingContext = bindable.BindingContext;
    }
 
    protected override void OnDetachedFrom(Element bindable, Android.Views.View platformView)
    {
        BindingContext = null;
    }
}
#elif IOS || MACCATALYST
public class LongPressBehavior : PlatformBehavior<Element, UIKit.UIView>
{
	protected override void OnAttachedTo(Element bindable, UIKit.UIView platformView)
	{
		BindingContext = bindable.BindingContext;
	}

	protected override void OnDetachedFrom(Element bindable, UIKit.UIView platformView)
	{
		BindingContext = null;

	}
}
#elif WINDOWS
public class LongPressBehavior : PlatformBehavior<Element, Microsoft.UI.Xaml.FrameworkElement>
{
    protected override void OnAttachedTo(Element bindable, Microsoft.UI.Xaml.FrameworkElement platformView)
    {
        BindingContext = bindable.BindingContext;
    }
 
    protected override void OnDetachedFrom(Element bindable, Microsoft.UI.Xaml.FrameworkElement platformView)
    {
        BindingContext = null;
    }
}
#else
public class LongPressBehavior : PlatformBehavior<Element, object>
{
	protected override void OnAttachedTo(Element bindable, object platformView)
	{
		BindingContext = bindable.BindingContext;
	}

	protected override void OnDetachedFrom(Element bindable, object platformView)
	{
		BindingContext = null;
	}
}
#endif