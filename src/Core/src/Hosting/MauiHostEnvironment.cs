using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Hosting;

public class MauiHostEnvironment : IHostEnvironment
{
	public string EnvironmentName
	{ 
		get => IsOptimized(Assembly.GetEntryAssembly()) ? "Production" : "Development";
		set => throw new System.NotImplementedException();
	}

	public string ApplicationName
	{
		get => AppInfo.Current.Name;
		set => throw new System.NotImplementedException();
	}

	public string ContentRootPath
	{ 
		get => throw new System.NotImplementedException();
		set => throw new System.NotImplementedException();
	}

	public IFileProvider ContentRootFileProvider
	{ 
		get => throw new System.NotImplementedException();
		set => throw new System.NotImplementedException();
	}

	static bool IsOptimized(Assembly? asm)
	{
		var att = asm?.GetCustomAttribute<DebuggableAttribute>();
		return att is null || att.IsJITOptimizerDisabled == false;
	}
}
