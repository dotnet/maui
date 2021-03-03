using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages
{
	public partial class ControlTemplateXamlPage : ContentPage
	{
		public static readonly BindableProperty AboveTextProperty =
			BindableProperty.Create(nameof(AboveText), typeof(string), typeof(ControlTemplateXamlPage), null);

		public string AboveText
		{
			get { return (string)GetValue(AboveTextProperty); }
			set { SetValue(AboveTextProperty, value); }
		}

		public ControlTemplateXamlPage()
		{
			BindingContext = new
			{
				Text = "Testing 123"
			};
			this.SetBinding(AboveTextProperty, "Text");
			InitializeComponent();
		}
	}
}
