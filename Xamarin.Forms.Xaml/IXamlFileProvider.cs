using System;

namespace Xamarin.Forms.Xaml
{
	public interface IXamlFileProvider
	{
		string GetXamlFor(Type type);
	}
}