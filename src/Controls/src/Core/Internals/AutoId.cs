// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class AutoId
	{
		int _current;

		public int Value => _current;

		public int Increment()
		{
			var old = _current;

			_current++;

			return old;
		}
	}
}