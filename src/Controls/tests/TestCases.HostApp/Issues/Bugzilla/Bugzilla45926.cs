namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 45926, "MessagingCenter prevents subscriber from being collected", PlatformAffected.All)]
public class Bugzilla45926 : TestNavigationPage
{
	protected override void Init()
	{
		Button createPage, sendMessage, doGC;

		Label instanceCount = new Label();
		Label messageCount = new Label();

		instanceCount.Text = $"Instances: {_45926SecondPage.InstanceCounter.ToString()}";
		messageCount.Text = $"Messages: {_45926SecondPage.MessageCounter.ToString()}";

		var content = new ContentPage
		{
			Title = "Test",
			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children = {
					(createPage = new Button { Text = "New Page" }),
					(sendMessage = new Button { Text = "Send Message" }),
					(doGC = new Button { Text = "Do GC" }),
					instanceCount, messageCount
				}
			}
		};

		createPage.Clicked += (s, e) =>
		{
			PushAsync(new _45926IntermediatePage());
			PushAsync(new _45926SecondPage());
		};

		sendMessage.Clicked += (s, e) =>
		{
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Send(this, "Test");
#pragma warning restore CS0618 // Type or member is obsolete
		};

		doGC.Clicked += (sender, e) =>
		{
			GarbageCollectionHelper.Collect();
			instanceCount.Text = $"Instances: {_45926SecondPage.InstanceCounter.ToString()}";
			messageCount.Text = $"Messages: {_45926SecondPage.MessageCounter.ToString()}";
		};

		PushAsync(content);
	}
}


public class _45926IntermediatePage : ContentPage
{
	public _45926IntermediatePage()
	{
		Content = new Label { Text = "Intermediate Page" };
	}
}


public class _45926SecondPage : ContentPage
{
	public static int InstanceCounter = 0;
	public static int MessageCounter = 0;

	public _45926SecondPage()
	{
		Interlocked.Increment(ref InstanceCounter);

		Content = new Label
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Text = "Second Page #" + (InstanceCounter)
		};

#pragma warning disable CS0618 // Type or member is obsolete
		MessagingCenter.Subscribe<Bugzilla45926>(this, "Test", OnMessage);
#pragma warning restore CS0618 // Type or member is obsolete
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
	}

	void OnMessage(Bugzilla45926 app)
	{
		System.Diagnostics.Debug.WriteLine("Got Test message!");
		Interlocked.Increment(ref MessageCounter);
	}

	~_45926SecondPage()
	{
		Interlocked.Decrement(ref InstanceCounter);
		System.Diagnostics.Debug.WriteLine("~SecondPage: {0}", GetHashCode());
	}
}