using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Xamarin.Forms.Platform.WPF.Controls
{
	public class FormsCarouselPage : FormsMultiPage
	{
		public static RoutedUICommand NextCommand = new RoutedUICommand("Next", "Next", typeof(FormsCarouselPage));
		public static RoutedUICommand PreviousCommand = new RoutedUICommand("Previous", "Previous", typeof(FormsCarouselPage));


		static FormsCarouselPage()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(FormsCarouselPage), new FrameworkPropertyMetadata(typeof(FormsCarouselPage)));
			SelectedIndexProperty.OverrideMetadata(typeof(FormsCarouselPage), new FrameworkPropertyMetadata(-1, OnSelectedIndexChanged));
		}

		public FormsCarouselPage()
		{
			this.CommandBindings.Add(new CommandBinding(NextCommand, this.OnNextExecuted, this.OnNextCanExecute));
			this.CommandBindings.Add(new CommandBinding(PreviousCommand, this.OnPreviousExecuted, this.OnPreviousCanExecute));
		}

		protected override void Appearing()
		{
			base.Appearing();
			this.SelectedIndex = 0;
		}

		private void OnPreviousCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.SelectedIndex > 0;
		}

		private void OnPreviousExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			this.SelectedIndex -= 1;
			FormsContentControl.Transition = TransitionType.Right;
		}

		private void OnNextCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (this.ItemsSource == null) return;
			e.CanExecute = this.SelectedIndex < (this.ItemsSource.Cast<object>().Count() - 1);
		}

		private void OnNextExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			this.SelectedIndex += 1;
			FormsContentControl.Transition = TransitionType.Left;
		}

		private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as FormsCarouselPage;

			control.OnSelectedIndexChanged(e);
		}

		private void OnSelectedIndexChanged(DependencyPropertyChangedEventArgs e)
		{
			if (this.ItemsSource == null) return;
			var items = this.ItemsSource.Cast<object>();

			if ((int)e.NewValue >= 0 && (int)e.NewValue < items.Count())
			{
				this.SelectedItem = items.ElementAt((int)e.NewValue);
			}
		}
	}
}
