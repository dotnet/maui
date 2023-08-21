// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
	public interface IPictureWriter
	{
		void Save(IPicture picture, Stream stream);
		Task SaveAsync(IPicture picture, Stream stream);
	}
}
