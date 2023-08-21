// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class TestLogger : ILogger
	{
		readonly List<string> messages = new List<string>();

		public IReadOnlyList<string> Messages => messages;

		public void Log(string message)
		{
			messages.Add(message);
		}

		public void Persist()
		{
			File.WriteAllLines("output.txt", messages);
		}
	}
}
