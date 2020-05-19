using System;

namespace Xamarin.Forms.Xaml
{
	internal interface IExpressionParser
	{
		object Parse(string match, ref string expression, IServiceProvider serviceProvider);
	}

	internal interface IExpressionParser<out T> : IExpressionParser
		where T : class
	{
		new T Parse(string match, ref string expression, IServiceProvider serviceProvider);
	}
}