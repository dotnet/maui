using Foundation;
using UIKit;
using System.Threading.Tasks;
using MauiApp1.Services;

[assembly: Dependency(typeof(MauiApp1.SemanticScreenReaderAsyncImplementation))]
namespace MauiApp1;

public class SemanticScreenReaderAsyncImplementation : IAsyncAnnouncement
{
    static NSObject? Token;
    private TaskCompletionSource<bool>? announcementCompletionSource;

    public void AddNotification()
    {
        if (Token != null)
            return;

        Token = NSNotificationCenter.DefaultCenter.AddObserver(
            new NSString("UIAccessibilityAnnouncementDidFinishNotification"),
            AnnouncementDidFinish);
    }

    public void RemoveNotification()
    {
        if (Token == null)
            return;

        NSNotificationCenter.DefaultCenter.RemoveObserver(Token);
        Token = null;
        announcementCompletionSource = null;
    }

    private void AnnouncementDidFinish(NSNotification notification)
    {
        announcementCompletionSource?.TrySetResult(true);
    }

    public void Announce(string text)
    {
        if (!UIAccessibility.IsVoiceOverRunning)
            return;

        UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, new NSString(text));
    }

    public async Task AnnounceAsync(string text)
    {
        AddNotification();

        if (!UIAccessibility.IsVoiceOverRunning)
            return;

        announcementCompletionSource = new TaskCompletionSource<bool>();

        await Task.Delay(100);
        UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, new NSString(text));

        // Wait until the announcement is finished
        await announcementCompletionSource.Task;

        RemoveNotification();
    }
}
