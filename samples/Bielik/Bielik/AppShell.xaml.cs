using Bielik.Views;

namespace Bielik;

public partial class AppShell : Shell
{
    public AppShell(DiscoverPage discover, ChatPage chat, ModelPage model, SettingsPage settings)
    {
        InitializeComponent();
        DiscoverContent.Content = discover;
        ChatContent.Content = chat;
        ModelContent.Content = model;
        SettingsContent.Content = settings;
    }
}
