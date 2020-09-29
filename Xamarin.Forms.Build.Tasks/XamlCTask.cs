using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xamarin.Forms.Xaml;
using static Microsoft.Build.Framework.MessageImportance;
using static Mono.Cecil.Cil.OpCodes;
using IOPath = System.IO.Path;

namespace Xamarin.Forms.Build.Tasks
{
	public class XamlCTask : XamlTask
	{
		bool hasCompiledXamlResources;
		public bool KeepXamlResources { get; set; }
		public bool OptimizeIL { get; set; }

		[Obsolete("OutputGeneratedILAsCode is obsolete as of version 2.3.4. This option is no longer available.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool OutputGeneratedILAsCode { get; set; }

		public bool CompileByDefault { get; set; }
		public bool ForceCompile { get; set; }

		public IAssemblyResolver DefaultAssemblyResolver { get; set; }

		internal string Type { get; set; }
		internal MethodDefinition InitCompForType { get; private set; }

		/// <summary>
		/// Enable to optimize for shorter build time
		/// e.g. OptimizeIL unused, Debug symbols not loaded, no assemblies written
		/// </summary>
		public bool ValidateOnly { get; set; }

		public override bool Execute(out IList<Exception> thrownExceptions)
		{
			thrownExceptions = null;
			LoggingHelper.LogMessage(Normal, $"{new string(' ', 0)}Compiling Xaml, assembly: {Assembly}");
			var skipassembly = !CompileByDefault;
			bool success = true;

			if (!File.Exists(Assembly))
			{
				LoggingHelper.LogMessage(Normal, $"{new string(' ', 2)}Assembly file not found. Skipping XamlC.");
				return true;
			}

			using (var fallbackResolver = DefaultAssemblyResolver == null ? new XamlCAssemblyResolver() : null)
			{
				var resolver = DefaultAssemblyResolver ?? fallbackResolver;
				if (resolver is XamlCAssemblyResolver xamlCResolver)
				{
					if (ReferencePath != null)
					{
						var paths = ReferencePath.Select(p => IOPath.GetDirectoryName(p.Replace("//", "/"))).Distinct();
						foreach (var searchpath in paths)
						{
							LoggingHelper.LogMessage(Low, $"{new string(' ', 2)}Adding searchpath {searchpath}");
							xamlCResolver.AddSearchDirectory(searchpath);
						}
					}
				}
				else
					LoggingHelper.LogMessage(Low, $"{new string(' ', 2)}Ignoring dependency and reference paths due to an unsupported resolver");

				var debug = DebugSymbols || (!string.IsNullOrEmpty(DebugType) && DebugType.ToLowerInvariant() != "none");

				var readerParameters = new ReaderParameters
				{
					AssemblyResolver = resolver,
					ReadWrite = !ValidateOnly,
					ReadSymbols = debug && !ValidateOnly, // We don't need symbols for ValidateOnly, since we won't be writing
				};

				using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(IOPath.GetFullPath(Assembly), readerParameters))
				{
					CustomAttribute xamlcAttr;
					if (assemblyDefinition.HasCustomAttributes &&
						(xamlcAttr =
							assemblyDefinition.CustomAttributes.FirstOrDefault(
								ca => ca.AttributeType.FullName == "Xamarin.Forms.Xaml.XamlCompilationAttribute")) != null)
					{
						var options = (XamlCompilationOptions)xamlcAttr.ConstructorArguments[0].Value;
						if ((options & XamlCompilationOptions.Skip) == XamlCompilationOptions.Skip)
							skipassembly = true;
						if ((options & XamlCompilationOptions.Compile) == XamlCompilationOptions.Compile)
							skipassembly = false;
					}

					foreach (var module in assemblyDefinition.Modules)
					{
						var skipmodule = skipassembly;
						if (module.HasCustomAttributes &&
							(xamlcAttr =
								module.CustomAttributes.FirstOrDefault(
									ca => ca.AttributeType.FullName == "Xamarin.Forms.Xaml.XamlCompilationAttribute")) != null)
						{
							var options = (XamlCompilationOptions)xamlcAttr.ConstructorArguments[0].Value;
							if ((options & XamlCompilationOptions.Skip) == XamlCompilationOptions.Skip)
								skipmodule = true;
							if ((options & XamlCompilationOptions.Compile) == XamlCompilationOptions.Compile)
								skipmodule = false;
						}

						LoggingHelper.LogMessage(Low, $"{new string(' ', 2)}Module: {module.Name}");
						var resourcesToPrune = new List<EmbeddedResource>();
						foreach (var resource in module.Resources.OfType<EmbeddedResource>())
						{
							LoggingHelper.LogMessage(Low, $"{new string(' ', 4)}Resource: {resource.Name}");
							string classname;
							if (!resource.IsXaml(module, out classname))
							{
								LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}skipped.");
								continue;
							}
							TypeDefinition typeDef = module.GetType(classname);
							if (typeDef == null)
							{
								LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}no type found... skipped.");
								continue;
							}
							var skiptype = skipmodule;
							if (typeDef.HasCustomAttributes &&
								(xamlcAttr =
									typeDef.CustomAttributes.FirstOrDefault(
										ca => ca.AttributeType.FullName == "Xamarin.Forms.Xaml.XamlCompilationAttribute")) != null)
							{
								var options = (XamlCompilationOptions)xamlcAttr.ConstructorArguments[0].Value;
								if ((options & XamlCompilationOptions.Skip) == XamlCompilationOptions.Skip)
									skiptype = true;
								if ((options & XamlCompilationOptions.Compile) == XamlCompilationOptions.Compile)
									skiptype = false;
							}

							if (Type != null)
								skiptype = !(Type == classname);

							if (skiptype && !ForceCompile)
							{
								LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}has XamlCompilationAttribute set to Skip and not Compile... skipped.");
								continue;
							}

							var initComp = typeDef.Methods.FirstOrDefault(md => md.Name == "InitializeComponent");
							if (initComp == null)
							{
								LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}no InitializeComponent found... skipped.");
								continue;
							}

							CustomAttribute xamlFilePathAttr;
							var xamlFilePath = typeDef.HasCustomAttributes && (xamlFilePathAttr = typeDef.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.FullName == "Xamarin.Forms.Xaml.XamlFilePathAttribute")) != null ?
													  (string)xamlFilePathAttr.ConstructorArguments[0].Value :
													  resource.Name;

							MethodDefinition initCompRuntime = null;
							if (!ValidateOnly)
							{
								initCompRuntime = typeDef.Methods.FirstOrDefault(md => md.Name == "__InitComponentRuntime");
								if (initCompRuntime != null)
									LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}__InitComponentRuntime already exists... not creating");
								else
								{
									LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Creating empty {typeDef.Name}.__InitComponentRuntime");
									initCompRuntime = new MethodDefinition("__InitComponentRuntime", initComp.Attributes, initComp.ReturnType);
									initCompRuntime.Body.InitLocals = true;
									LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}done.");
									LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Copying body of InitializeComponent to __InitComponentRuntime");
									initCompRuntime.Body = new MethodBody(initCompRuntime);
									var iCRIl = initCompRuntime.Body.GetILProcessor();
									foreach (var instr in initComp.Body.Instructions)
										iCRIl.Append(instr);
									initComp.Body.Instructions.Clear();
									initComp.Body.GetILProcessor().Emit(OpCodes.Ret);
									initComp.Body.InitLocals = true;
									typeDef.Methods.Add(initCompRuntime);
									LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}done.");
								}
							}

							LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Parsing Xaml");
							var rootnode = ParseXaml(resource.GetResourceStream(), typeDef);
							if (rootnode == null)
							{
								LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}failed.");
								continue;
							}
							LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}done.");

							hasCompiledXamlResources = true;

							LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Replacing {0}.InitializeComponent ()");
							Exception e;
							if (!TryCoreCompile(initComp, initCompRuntime, rootnode, xamlFilePath, out e))
							{
								success = false;
								LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}failed.");
								(thrownExceptions = thrownExceptions ?? new List<Exception>()).Add(e);
								if (e is BuildException be)
									LoggingHelper.LogError("XamlC", be.Code.Code, be.HelpLink, xamlFilePath, be.XmlInfo?.LineNumber ?? 0, be.XmlInfo?.LinePosition ?? 0, 0, 0, ErrorMessages.ResourceManager.GetString(be.Code.ErrorMessageKey), be.MessageArgs);
								else if (e is XamlParseException xpe) //shouldn't happen anymore
									LoggingHelper.LogError("XamlC", null, xpe.HelpLink, xamlFilePath, xpe.XmlInfo.LineNumber, xpe.XmlInfo.LinePosition, 0, 0, xpe.Message);
								else if (e is XmlException xe)
									LoggingHelper.LogError("XamlC", null, xe.HelpLink, xamlFilePath, xe.LineNumber, xe.LinePosition, 0, 0, xe.Message);
								else
									LoggingHelper.LogError("XamlC", null, e.HelpLink, xamlFilePath, 0, 0, 0, 0, e.Message);
								LoggingHelper.LogMessage(Low, e.StackTrace);
								continue;
							}
							if (Type != null)
								InitCompForType = initComp;

							LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}done.");

							if (ValidateOnly)
								continue;

							if (OptimizeIL)
							{
								LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Optimizing IL");
								initComp.Body.Optimize();
								LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}done.");
							}

#pragma warning disable 0618
							if (OutputGeneratedILAsCode)
								LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Decompiling option has been removed. Use a 3rd party decompiler to admire the beauty of the IL generated");
#pragma warning restore 0618
							resourcesToPrune.Add(resource);
						}
						if (hasCompiledXamlResources)
						{
							LoggingHelper.LogMessage(Low, $"{new string(' ', 4)}Changing the module MVID");
							module.Mvid = Guid.NewGuid();
							LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}done.");
						}
						if (!KeepXamlResources)
						{
							if (resourcesToPrune.Any())
								LoggingHelper.LogMessage(Low, $"{new string(' ', 4)}Removing compiled xaml resources");
							foreach (var resource in resourcesToPrune)
							{
								LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Removing {resource.Name}");
								module.Resources.Remove(resource);
								LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}done.");
							}
						}
					}
					if (ValidateOnly)
					{
						LoggingHelper.LogMessage(Low, $"{new string(' ', 0)}ValidateOnly=True. Skipping writing assembly.");
						return success;
					}
					if (!hasCompiledXamlResources)
					{
						LoggingHelper.LogMessage(Low, $"{new string(' ', 0)}No compiled resources. Skipping writing assembly.");
						return success;
					}

					LoggingHelper.LogMessage(Low, $"{new string(' ', 0)}Writing the assembly");
					try
					{
						assemblyDefinition.Write(new WriterParameters
						{
							WriteSymbols = debug,
						});
						LoggingHelper.LogMessage(Low, $"{new string(' ', 2)}done.");
					}
					catch (Exception e)
					{
						LoggingHelper.LogMessage(Low, $"{new string(' ', 2)}failed.");
						LoggingHelper.LogErrorFromException(e);
						(thrownExceptions = thrownExceptions ?? new List<Exception>()).Add(e);
						LoggingHelper.LogMessage(Low, e.StackTrace);
						success = false;
					}
				}
			}
			return success;
		}

		bool TryCoreCompile(MethodDefinition initComp, MethodDefinition initCompRuntime, ILRootNode rootnode, string xamlFilePath, out Exception exception)
		{
			try
			{
				var body = new MethodBody(initComp);
				var module = body.Method.Module;
				body.InitLocals = true;
				var il = body.GetILProcessor();
				var resourcePath = GetPathForType(module, initComp.DeclaringType);

				il.Emit(Nop);

				if (initCompRuntime != null)
				{
					// Generating branching code for the Previewer

					//First using the ResourceLoader
					var nop = Instruction.Create(Nop);

					// if (ResourceLoader.IsEnabled && ...
					il.Emit(Call, module.ImportPropertyGetterReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "ResourceLoader"), "IsEnabled", isStatic: true));
					il.Emit(Brfalse, nop);

					il.Emit(Newobj, module.ImportCtorReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "ResourceLoader/ResourceLoadingQuery"), 0));

					//AssemblyName
					il.Emit(Dup); //dup the RLQ
					il.Emit(Ldtoken, module.ImportReference(initComp.DeclaringType));
					il.Emit(Call, module.ImportMethodReference(("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") }, isStatic: true));
					il.Emit(Call, module.ImportMethodReference(("mscorlib", "System.Reflection", "IntrospectionExtensions"), methodName: "GetTypeInfo", parameterTypes: new[] { ("mscorlib", "System", "Type") }, isStatic: true));
					il.Emit(Callvirt, module.ImportPropertyGetterReference(("mscorlib", "System.Reflection", "TypeInfo"), propertyName: "Assembly", flatten: true));
					il.Emit(Callvirt, module.ImportMethodReference(("mscorlib", "System.Reflection", "Assembly"), methodName: "GetName", parameterTypes: null));
					il.Emit(Callvirt, module.ImportPropertySetterReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "ResourceLoader/ResourceLoadingQuery"), "AssemblyName"));

					//ResourcePath
					il.Emit(Dup); //dup the RLQ
					il.Emit(Ldstr, resourcePath);
					il.Emit(Callvirt, module.ImportPropertySetterReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "ResourceLoader/ResourceLoadingQuery"), "ResourcePath"));

					//Instance
					il.Emit(Dup); //dup the RLQ
					il.Emit(Ldarg_0); //Instance = this
					il.Emit(Callvirt, module.ImportPropertySetterReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "ResourceLoader/ResourceLoadingQuery"), "Instance"));

					il.Emit(Call, module.ImportMethodReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "ResourceLoader"), "CanProvideContentFor", 1, isStatic: true));
					il.Emit(Brfalse, nop);
					il.Emit(Ldarg_0);
					il.Emit(Call, initCompRuntime);
					il.Emit(Ret);
					il.Append(nop);

					//Or using the deprecated XamlLoader
					nop = Instruction.Create(Nop);

					var getXamlFileProvider = module.ImportPropertyGetterReference(("Xamarin.Forms.Xaml", "Xamarin.Forms.Xaml.Internals", "XamlLoader"), propertyName: "XamlFileProvider", isStatic: true);
					il.Emit(Call, getXamlFileProvider);
					il.Emit(Brfalse, nop);
					il.Emit(Call, getXamlFileProvider);
					il.Emit(Ldarg_0);
					il.Emit(Call, module.ImportMethodReference(("mscorlib", "System", "Object"), methodName: "GetType", parameterTypes: null));
					il.Emit(Callvirt, module.ImportMethodReference(("mscorlib", "System", "Func`2"),
																   methodName: "Invoke",
																   paramCount: 1,
																   classArguments: new[] { ("mscorlib", "System", "Type"), ("mscorlib", "System", "String") }));
					il.Emit(Brfalse, nop);
					il.Emit(Ldarg_0);
					il.Emit(Call, initCompRuntime);
					il.Emit(Ret);
					il.Append(nop);
				}

				var visitorContext = new ILContext(il, body, module)
				{
					DefineDebug = DebugSymbols || (!string.IsNullOrEmpty(DebugType) && DebugType.ToLowerInvariant() != "none"),
					XamlFilePath = xamlFilePath
				};


				rootnode.Accept(new XamlNodeVisitor((node, parent) => node.Parent = parent), null);
				rootnode.Accept(new ExpandMarkupsVisitor(visitorContext), null);
				rootnode.Accept(new PruneIgnoredNodesVisitor(), null);
				rootnode.Accept(new CreateObjectVisitor(visitorContext), null);
				rootnode.Accept(new SetNamescopesAndRegisterNamesVisitor(visitorContext), null);
				rootnode.Accept(new SetFieldVisitor(visitorContext), null);
				rootnode.Accept(new SetResourcesVisitor(visitorContext), null);
				rootnode.Accept(new SetPropertiesVisitor(visitorContext, true), null);

				il.Emit(Ret);
				initComp.Body = body;
				exception = null;
				return true;
			}
			catch (Exception e)
			{
				exception = e;
				return false;
			}
		}

		internal static string GetPathForType(ModuleDefinition module, TypeReference type)
		{
			foreach (var ca in type.Module.GetCustomAttributes())
			{
				if (!TypeRefComparer.Default.Equals(ca.AttributeType, module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms.Xaml", "XamlResourceIdAttribute"))))
					continue;
				if (!TypeRefComparer.Default.Equals(ca.ConstructorArguments[2].Value as TypeReference, type))
					continue;
				return ca.ConstructorArguments[1].Value as string;
			}
			return null;
		}

		internal static string GetResourceIdForPath(ModuleDefinition module, string path)
		{
			foreach (var ca in module.GetCustomAttributes())
			{
				if (!TypeRefComparer.Default.Equals(ca.AttributeType, module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms.Xaml", "XamlResourceIdAttribute"))))
					continue;
				if (ca.ConstructorArguments[1].Value as string != path)
					continue;
				return ca.ConstructorArguments[0].Value as string;
			}
			return null;
		}
	}
}