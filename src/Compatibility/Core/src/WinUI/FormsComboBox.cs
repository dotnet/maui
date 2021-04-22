using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using WVisualState = Microsoft.UI.Xaml.VisualState;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class FormsComboBox : ComboBox
	{
		public FormsComboBox()
		{
			DefaultStyleKey = typeof(FormsComboBox);
			DropDownOpened += FormsComboBoxDropDownOpened;
			SelectionChanged += FormsComboBoxSelectionChanged;
		}

		void FormsComboBoxDropDownOpened(object sender, object e)
		{
			MinWidth = ActualWidth;
		}

		void FormsComboBoxSelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
		{
			MinWidth = 0;
		}

		internal bool IsClosingAnimated { get; private set; }

		internal bool IsFullScreen => Device.Idiom == TargetIdiom.Phone && Items != null && Items.Count > 5;

		internal bool IsOpeningAnimated { get; private set; }

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			
			if (Device.Idiom == TargetIdiom.Phone)
			{
				// If we're running on the phone, we have to give the PickerRenderer hooks
				// into the opening and closing animations so it can handle them smoothly
				// and measure at the appropriate times

				var openedState = GetTemplateChild("Opened") as WVisualState;
				if (openedState != null)
				{
					openedState.Storyboard.Completed += (sender, o) => OnOpenAnimationCompleted();
					IsOpeningAnimated = true;
				}

				var closedState = GetTemplateChild("Closed") as WVisualState;

				// On the phone, this is a dummy animation we've added to the closed state in the VSM
				// Since it finishes immediately, we can use its Completed event to signal that the 
				// closing animation has started 
				var closedSignalAnimation = closedState?.Storyboard.Children[0] as DoubleAnimation;

				if (closedSignalAnimation != null)
				{
					closedSignalAnimation.Completed += (sender, o) => OnClosedAnimationStarted();
					IsClosingAnimated = true;
				}
			}
		}

		protected virtual void OnClosedAnimationStarted()
		{
			ClosedAnimationStarted?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnOpenAnimationCompleted()
		{
			OpenAnimationCompleted?.Invoke(this, EventArgs.Empty);
		}

		internal event EventHandler<EventArgs> ClosedAnimationStarted;

		internal event EventHandler<EventArgs> OpenAnimationCompleted;
	}
}