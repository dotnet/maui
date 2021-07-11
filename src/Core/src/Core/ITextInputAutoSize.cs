using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	public interface ITextInputAutoSize
	{
		/// <summary>
		/// Gets or sets the AutoSize option.
		/// </summary>
		EditorAutoSizeOption AutoSize { get; set; }
	}
}
