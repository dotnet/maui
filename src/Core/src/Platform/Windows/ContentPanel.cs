using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Platform;

public class ContentPanel : MauiPanel
{
	FrameworkElement? _content;

	internal FrameworkElement? Content
	{
		get => _content;
		set
		{
			if (_content == value)
				return;

			if (_content is not null && Children.Contains(_content))
			{
				Children.Remove(_content);
				_content = null;
			}

			_content = value;

			Children.Add(_content);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public void UpdateBackground(Paint? background)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Microsoft.Maui.Platform.UpdateBorderStroke instead")]
	public void UpdateBorderShape(IShape borderShape)
	{
	}
}
