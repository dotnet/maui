using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace System.Maui.Material.iOS
{
	public interface IMaterialEntryRenderer
	{
		Color TextColor { get; }
		Color PlaceholderColor { get; }
		Color BackgroundColor { get; }
		string Placeholder { get; }
	}
}