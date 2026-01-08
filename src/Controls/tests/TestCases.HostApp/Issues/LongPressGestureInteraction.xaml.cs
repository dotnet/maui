using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.None, 0, "LongPress Gesture Interaction Tests", PlatformAffected.All)]
	public partial class LongPressGestureInteraction : ContentPage
	{
		int _tapCount = 0;
		int _longPressCount = 0;
		int _longPress2Count = 0;
		int _longPress3Count = 0;
		int _longPress4Count = 0;
		int _longPress5Count = 0;
		int _swipeCount = 0;

		public LongPressGestureInteraction()
		{
			InitializeComponent();
		}

		void OnTapped(object sender, TappedEventArgs e)
		{
			_tapCount++;
			var label = this.FindByName<Label>("TapLabel");
			if (label != null)
				label.Text = $"Tap Count: {_tapCount}";
		}

		void OnLongPressed(object sender, LongPressedEventArgs e)
		{
			_longPressCount++;
			var label = this.FindByName<Label>("LongPressLabel");
			if (label != null)
				label.Text = $"Long Press Count: {_longPressCount}";
		}

		void OnLongPressed2(object sender, LongPressedEventArgs e)
		{
			_longPress2Count++;
			var label = this.FindByName<Label>("LongPress2Label");
			if (label != null)
				label.Text = $"Long Press Count: {_longPress2Count}";
		}

		void OnLongPressed3(object sender, LongPressedEventArgs e)
		{
			_longPress3Count++;
			var label = this.FindByName<Label>("LongPress3Label");
			if (label != null)
				label.Text = $"Long Press Count: {_longPress3Count}";
		}

		void OnLongPressed4(object sender, LongPressedEventArgs e)
		{
			_longPress4Count++;
			var label = this.FindByName<Label>("LongPress4Label");
			if (label != null)
				label.Text = $"LongPress1 Count: {_longPress4Count}";
		}

		void OnLongPressed5(object sender, LongPressedEventArgs e)
		{
			_longPress5Count++;
			var label = this.FindByName<Label>("LongPress5Label");
			if (label != null)
				label.Text = $"LongPress2 Count: {_longPress5Count}";
		}

		void OnSwiped(object sender, SwipedEventArgs e)
		{
			_swipeCount++;
			var label = this.FindByName<Label>("SwipeLabel");
			if (label != null)
				label.Text = $"Swipe Count: {_swipeCount}";
		}
	}
}
