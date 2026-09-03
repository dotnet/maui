using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class WindowsCollapseStyleChangerPage : ContentPage
	{
		public static readonly BindableProperty ParentPageProperty = BindableProperty.Create("ParentPage", typeof(Microsoft.Maui.Controls.FlyoutPage), typeof(WindowsCollapseStyleChangerPage), null, propertyChanged: OnParentPagePropertyChanged);

		public Microsoft.Maui.Controls.FlyoutPage ParentPage
		{
			get { return (Microsoft.Maui.Controls.FlyoutPage)GetValue(ParentPageProperty); }
			set { SetValue(ParentPageProperty, value); }
		}

		public WindowsCollapseStyleChangerPage()
		{
			InitializeComponent();
			PopulatePicker();
		}

		void PopulatePicker()
		{
			var enumType = typeof(CollapseStyle);
			var collapseOptions = Enum.GetNames(enumType);
			foreach (string option in collapseOptions)
			{
				picker.Items.Add(option);
			}
		}

		void OnPickerSelectedIndexChanged(object sender, EventArgs e)
		{
			ParentPage.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetCollapseStyle((CollapseStyle)Enum.Parse(typeof(CollapseStyle), picker.Items[picker.SelectedIndex]));
		}

		static void OnParentPagePropertyChanged(BindableObject element, object oldValue, object newValue)
		{
			if (newValue != null)
			{
				var enumType = typeof(CollapseStyle);
				var instance = (WindowsCollapseStyleChangerPage)element;
				instance.picker.SelectedIndex = Array.IndexOf(Enum.GetNames(enumType), Enum.GetName(enumType, instance.ParentPage.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().GetCollapseStyle()));
			}
		}
	}
}
