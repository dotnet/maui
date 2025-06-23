namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Bugzilla, 28001, "[Android] TabbedPage: invisible tabs are not Disposed", PlatformAffected.Android)]
public class Bugzilla28001 : NavigationPage
{
	public Bugzilla28001()
	{
		Navigation.PushAsync(new MainPage());
	}

	public class MainPage : ContentPage
	{
		static int s_disposeCount;
		static Label s_lbl;

		void HandleDispose(object sender, EventArgs e)
		{
			s_disposeCount++;
			s_lbl.Text = string.Format("Dispose {0} pages", s_disposeCount);
		}

		public MainPage()
		{
			s_disposeCount = 0;
			s_lbl = new Label { AutomationId = "lblDisposedCount" };
			var tab1 = new DisposePage { Title = "Tab1", AutomationId = "Tab1" };
			var tab2 = new DisposePage { Title = "Tab2", AutomationId = "Tab2" };
			tab1.RendererDisposed += HandleDispose;
			tab2.RendererDisposed += HandleDispose;

			tab2.PopAction = tab1.PopAction = async () => await Navigation.PopAsync();

			var tabbedPage = new TabbedPage { Children = { tab1, tab2 } };
			var btm = new Button { Text = "Push", AutomationId = "Push" };

			btm.Clicked += async (object sender, EventArgs e) =>
			{
				await Navigation.PushAsync(tabbedPage);
			};

			Content = new StackLayout { Children = { btm, s_lbl } };
		}
	}

	public class DisposePage : ContentPage
	{
		public event EventHandler RendererDisposed;

		public void SendRendererDisposed()
		{
			var handler = RendererDisposed;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		public int DisposedLabelCount { get; private set; }

		public Action PopAction { get; set; }
		public DisposePage()
		{
			var popButton = new Button { Text = "Pop", AutomationId = "Pop" };
			popButton.Clicked += (sender, args) => PopAction();

			var disposeLabel1 = new DisposeLabel { Text = "Label 1" };
			var disposeLabel2 = new DisposeLabel { Text = "Label 2" };
			var disposeLabel3 = new DisposeLabel { Text = "Label 3" };
			var disposeLabel4 = new DisposeLabel { Text = "Label 4" };
			var disposeLabel5 = new DisposeLabel { Text = "Label 5" };

			EventHandler disposeHandler = (sender, args) => DisposedLabelCount++;
			disposeLabel1.RendererDisposed += disposeHandler;
			disposeLabel2.RendererDisposed += disposeHandler;
			disposeLabel3.RendererDisposed += disposeHandler;
			disposeLabel4.RendererDisposed += disposeHandler;
			disposeLabel5.RendererDisposed += disposeHandler;

			Content = new StackLayout
			{
				Children = {
					popButton,
					disposeLabel1,
					disposeLabel2,
					disposeLabel3,
					disposeLabel4,
					new StackLayout {
						Children = {
							disposeLabel5,
						}
					}
				}
			};
		}
	}

	public class DisposeLabel : Label
	{
		public event EventHandler RendererDisposed;

		public void SendRendererDisposed()
		{
			var handler = RendererDisposed;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}
	}
}
