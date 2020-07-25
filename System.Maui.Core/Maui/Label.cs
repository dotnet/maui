using System;
using System.Collections.Generic;
using System.Text;

namespace System.Maui
{
	public partial class Label : ILabel
	{
		Color IText.Color => TextColor;
	}
}
