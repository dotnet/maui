using System.Windows.Controls;
using WSelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;

namespace System.Maui.Platform
{
	public partial class PickerRenderer : AbstractViewRenderer<IPicker, ComboBox>
	{
		const string TextBoxTemplate = "PART_EditableTextBox";
		protected override ComboBox CreateView()
		{
			var combox = new ComboBox();
			combox.IsEditable = true;
			combox.SelectionChanged += OnControlSelectionChanged;
			combox.Loaded += OnControlLoaded;

			combox.ItemsSource = ((LockableObservableListWrapper)VirtualView.Items)._list;
			return combox;
		}

		protected override void DisposeView(ComboBox nativeView)
		{
			nativeView.SelectionChanged -= OnControlSelectionChanged;
			nativeView.Loaded -= OnControlLoaded;

			base.DisposeView(nativeView);
		}

		public static void MapPropertyTitle(IViewRenderer renderer, IPicker picker) => (renderer as PickerRenderer)?.UpdateTitle();
		public static void MapPropertyTitleColor(IViewRenderer renderer, IPicker picker) => (renderer as PickerRenderer)?.UpdateTitleColor();
		public static void MapPropertyTextColor(IViewRenderer renderer, IPicker picker) => (renderer as PickerRenderer)?.UpdateTextColor();
		public static void MapPropertySelectedIndex(IViewRenderer renderer, IPicker picker) => (renderer as PickerRenderer)?.UpdateSelectedIndex();

		public virtual void UpdateBackgroundColor()
		{
			var textbox = (TextBox)TypedNativeView.Template.FindName(TextBoxTemplate, TypedNativeView);
			if (textbox != null)
			{
				var parent = (Border)textbox.Parent;
				parent.Background = VirtualView.BackgroundColor.ToBrush();
			}
		}

		public virtual void UpdateSelectedIndex()
		{
			TypedNativeView.SelectedIndex = VirtualView.SelectedIndex;
		}

		public virtual void UpdateTitle()
		{
			//TODO: Create full size combobox
		}

		public virtual void UpdateTitleColor()
		{
			//TODO: Create full size combobox
		}

		public virtual void UpdateTextColor()
		{
			TypedNativeView.UpdateDependencyColor(ComboBox.ForegroundProperty, VirtualView.TextColor);
		}

		void OnControlSelectionChanged(object sender, WSelectionChangedEventArgs e)
		{
			VirtualView.SelectedIndex = TypedNativeView.SelectedIndex;
		}

		void OnControlLoaded(object sender, System.Windows.RoutedEventArgs e)
		{
			UpdateBackgroundColor();
		}
	}
}
