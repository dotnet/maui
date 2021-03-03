using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public partial class XamlNativeViews : ContentPage
	{
		public XamlNativeViews()
		{
			InitializeComponent();
			BindingContext = new VM { NativeText = "Text set to Native view using native binding" };
		}
	}

	[Preserve(AllMembers = true)]
	public class VM
	{
		public string NativeText { get; set; }
	}
}