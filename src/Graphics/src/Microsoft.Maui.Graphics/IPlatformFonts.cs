#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Microsoft.Maui.Graphics
{
	public interface IPlatformFonts
	{
		IFont Default { get; }
		IFont DefaultBold { get; }

		void Register(string alias, params FontSource[] sources);

		object Get(IFont font);

		object Get(string alias, int weight = FontWeights.Normal, FontStyleType fontStyleType = FontStyleType.Normal);
	}
}
