// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using Mono.Cecil;
using NUnit.Framework;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class MockAssemblyResolver : BaseAssemblyResolver
	{
		public override AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			AssemblyDefinition assembly;
			var localPath = IOPath.GetFullPath(IOPath.Combine(TestContext.CurrentContext.TestDirectory, $"{name.Name}.dll"));
			if (File.Exists(localPath))
				assembly = AssemblyDefinition.ReadAssembly(localPath);
			else
				assembly = base.Resolve(name);
			return assembly;
		}
	}
}