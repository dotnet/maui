// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.DeviceTests
{
	public class TextTransformCases : IEnumerable<object[]>
	{
		private readonly List<object[]> _data = new()
		{
			new object[] { "Hello There", TextTransform.None, "Hello There" },
			new object[] { "Hello There", TextTransform.Uppercase, "HELLO THERE" },
			new object[] { "Hello There", TextTransform.Lowercase, "hello there" }
		};

		public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
