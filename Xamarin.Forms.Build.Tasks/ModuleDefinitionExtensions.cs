using System;
using System.Collections.Generic;

using Mono.Cecil;

namespace Xamarin.Forms.Build.Tasks
{
	static class ModuleDefinitionExtensions
	{
		static readonly Dictionary<Tuple<ModuleDefinition, System.Reflection.MethodBase>, MethodReference> MbMr =
			new Dictionary<Tuple<ModuleDefinition, System.Reflection.MethodBase>, MethodReference>();

		static readonly Dictionary<Tuple<ModuleDefinition, Type>, TypeReference> TTr =
			new Dictionary<Tuple<ModuleDefinition, Type>, TypeReference>();

		public static MethodReference ImportReferenceCached(this ModuleDefinition module, System.Reflection.MethodBase method)
		{
			var key = new Tuple<ModuleDefinition, System.Reflection.MethodBase>(module, method);
			if (MbMr.TryGetValue(key, out var result))
				return result;
			return MbMr[key] = module.ImportReference(method);
		}

		public static TypeReference ImportReferenceCached(this ModuleDefinition module, Type type)
		{
			var key = new Tuple<ModuleDefinition, Type>(module, type);
			if (TTr.TryGetValue(key, out var result))
				return result;
			return TTr[key] = module.ImportReference(type);
		}
	}
}