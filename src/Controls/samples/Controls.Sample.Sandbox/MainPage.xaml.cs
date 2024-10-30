using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
#if IOS
using UIKit;
#endif
#if WINDOWS
using Microsoft.UI.Xaml;
#endif


namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        // Items = new ObservableCollection<string>
        // {
        //     "https://api.time.com/wp-content/uploads/2022/01/TIM220117_Shonda.Cover_.FINALv2.jpg?quality=85&w=840",
        //     "https://api.time.com/wp-content/uploads/2022/01/TIM220131_Biden.Cover_.FINAL_.jpg?quality=85&w=840",
        //     "https://api.time.com/wp-content/uploads/2022/02/TIM220214_Covid.Cover_.FINAL_.jpg?quality=85&w=840",
        //     "https://api.time.com/wp-content/uploads/2022/02/TIM220228_KOTY.Cover_.FINAL_.jpg?quality=85&w=840",
        //     "https://api.time.com/wp-content/uploads/2022/03/TIM220314-Amal-Clooney-Cover-women-of-the-year.jpg?quality=85&w=840",
        //     "https://api.time.com/wp-content/uploads/2022/03/TIM220411-Kaling-Cover.FINAL_.jpg?quality=85&w=840",
        //     "https://api.time.com/wp-content/uploads/2022/04/TIM220425-Ohtani-Cover.FINAL2_.jpg?quality=85&w=840",
        //     "https://api.time.com/wp-content/uploads/2022/04/Musk.Cover_.Final_.jpg?quality=85&w=840",
        //     "https://api.time.com/wp-content/uploads/2022/05/TIM220523_Arctic-Cover-FINAL.jpg?quality=85&w=840",
        //     "https://api.time.com/wp-content/uploads/2022/05/TIM220606_Zendaya-Cover.jpg?quality=85&w=840",
        //     "https://api.time.com/wp-content/uploads/2022/05/Enough.Cover_.Final_.jpg?quality=85&w=840",
        //     "https://api.time.com/wp-content/uploads/2022/06/TIM220704_OceansCover.FINAL_.jpg?quality=85&w=840",
        //     "https://api.time.com/wp-content/uploads/2022/07/TIM220725_WGP-Cover-Final.jpg?quality=85&w=840",
        //     "https://api.time.com/wp-content/uploads/2022/06/TIM220725_Abortion-Cover.jpg?quality=85&w=840"
        // };

        // GoToCommand = new Command(()=> {
        // 	Navigation.PushAsync(new ContentPage());
        // });

        BindingContext = new MainPageViewModel();
    }

    // public ObservableCollection<string> Items { get; }

    //public Command GoToCommand {get; set; }
}

public class ItemModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime SelectedAt { get; set; }
}

public partial class MainPageViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private List<ItemModel>? _items;
    public List<ItemModel>? Items
    {
        get => _items;
        set
        {
            _items = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));
        }
    }

    private ItemModel? _selectedItem;
    public ItemModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
        }
    }

    //public ICommand SelectCommand { get; set; }

    public MainPageViewModel()
    {
        Items = new List<ItemModel>()
            {
                new ItemModel {Id = 1, Name = "One"},
                new ItemModel {Id = 2, Name = "Two"},
                new ItemModel {Id = 3, Name = "Three"},
                new ItemModel {Id = 4, Name = "Four"}
            };

        //SelectCommand = new Command<object>(SelectCurrentItem);
    }

    [RelayCommand]
    private void SelectCurrentItem(object item)
    {
        if (item is ItemModel itemModel)
        {
            itemModel.SelectedAt = DateTime.UtcNow;
            SelectedItem = itemModel;
        }
        else
        {
            SelectedItem = new ItemModel { Id = -1, Name = "Unknown!", SelectedAt = DateTime.UtcNow };
        }
    }
}

public class PressedRoutingEffect : RoutingEffect
{
    public static readonly BindableProperty ClickCommandProperty = BindableProperty.CreateAttached("ClickCommand", typeof(ICommand), typeof(PressedRoutingEffect), null);

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.CreateAttached("CommandParameter", typeof(object), typeof(PressedRoutingEffect), null);

    public static ICommand? GetClickCommand(BindableObject view)
    {
        var val = view.GetValue(ClickCommandProperty);

        return val as ICommand;
    }

    public static void SetClickCommand(BindableObject view, ICommand value)
    {
        view.SetValue(ClickCommandProperty, value);
    }

    public static object GetCommandParameter(BindableObject view)
    {
        return view.GetValue(CommandParameterProperty);
    }

    public static void SetCommandParameter(BindableObject view, object value)
    {
        view.SetValue(CommandParameterProperty, value);
    }
}

#if WINDOWS
    public class PressedPlatformEffect : Microsoft.Maui.Controls.Platform.PlatformEffect
    {
        public PressedPlatformEffect() : base()
        {
        }

        private bool _attached;

        /// <summary>
        /// Apply the handler
        /// </summary>
        protected override void OnAttached()
        {
            if (!_attached)
            {
                Container.Tapped += Container_Tapped;
                _attached = true;
            }
        }

        private void Container_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var command = PressedRoutingEffect.GetClickCommand(Element);
            command?.Execute(PressedRoutingEffect.GetCommandParameter(Element));
        }

        /// <summary>
        /// Clean the event handler on detach
        /// </summary>
        protected override void OnDetached()
        {
            if (_attached)
            {
                Container.Tapped -= Container_Tapped;
                _attached = false;
            }
        }
    }
#elif ANDROID
public class PressedPlatformEffect : Microsoft.Maui.Controls.Platform.PlatformEffect
{
    private bool _attached;

    public PressedPlatformEffect() : base()
    {
    }

    protected override void OnAttached()
    {
        if (!_attached)
        {
            if (Control != null)
            {
                Control.Click += Control_Click;
            }
            else
            {
                Control.Click += Control_Click;
            }
            _attached = true;
        }
    }

    private void Control_Click(object sender, EventArgs e)
    {
        var command = PressedRoutingEffect.GetClickCommand(Element);
        command?.Execute(PressedRoutingEffect.GetCommandParameter(Element));
    }

    protected override void OnDetached()
    {
        if (_attached)
        {
            if (Control != null)
            {
                Control.Click -= Control_Click;

            }
            else
            {
                Control.Click -= Control_Click;
            }
            _attached = false;
        }
    }
}
#elif IOS
    public class PressedPlatformEffect : Microsoft.Maui.Controls.Platform.PlatformEffect
    {
        private bool _attached;
        private readonly UITapGestureRecognizer tapGestureRecognizer;

        public PressedPlatformEffect() : base()
        {
            tapGestureRecognizer = new UITapGestureRecognizer(Control_Click);
        }

        protected override void OnAttached()
        {
            //because an effect can be detached immediately after attached (happens in listview), only attach the handler one time
            if (!_attached)
            {
                Container.AddGestureRecognizer(tapGestureRecognizer);
                _attached = true;
            }
        }

        private void Control_Click()
        {
            var command = PressedRoutingEffect.GetClickCommand(Element);
            command?.Execute(PressedRoutingEffect.GetCommandParameter(Element));
        }

        protected override void OnDetached()
        {
            if (_attached)
            {
                Container.RemoveGestureRecognizer(tapGestureRecognizer);
                _attached = false;
            }
        }
    }
#endif
