using System;
using System.Collections.Generic;
using System.Maui;
using System.Text;

namespace Xamarin.Forms
{
	public partial class Label : ILabel
	{
		Color IText.Color => TextColor;
	}
}
