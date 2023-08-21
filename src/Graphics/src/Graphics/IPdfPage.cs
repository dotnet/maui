// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
	public interface IPdfPage : IDrawable, IDisposable
	{
		float Width { get; }
		float Height { get; }
		int PageNumber { get; }

		void Save(Stream stream);
		Task SaveAsync(Stream stream);
	}
}
