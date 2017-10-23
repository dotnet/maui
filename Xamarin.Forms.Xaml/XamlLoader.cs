//
// XamlLoader.cs
//
// Author:
//       Stephane Delcroix <stephane@mi8.be>
//
// Copyright (c) 2013 Mobile Inception
// Copyright (c) 2013-2014 Xamarin, Inc
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml.Internals
{
	[Obsolete ("Replaced by ResourceLoader")]
	public static class XamlLoader
	{
		static Func<Type, string> xamlFileProvider;

		public static Func<Type, string> XamlFileProvider {
			get { return xamlFileProvider; }
			internal set {
				xamlFileProvider = value;
				//¯\_(ツ)_/¯ the previewer forgot to set that bool
				DoNotThrowOnExceptions = value != null;
			}
		}

		internal static bool DoNotThrowOnExceptions { get; set; }
	}
}

namespace Xamarin.Forms.Xaml
{
	static class XamlLoader
	{
		public static void Load(object view, Type callingType)
		{
			var xaml = GetXamlForType(callingType);
			if (string.IsNullOrEmpty(xaml))
				throw new XamlParseException(string.Format("No embeddedresource found for {0}", callingType), new XmlLineInfo());
			Load(view, xaml);
		}

		public static void Load(object view, string xaml)
		{
			using (var textReader = new StringReader(xaml))
			using (var reader = XmlReader.Create(textReader))
			{
				while (reader.Read())
				{
					//Skip until element
					if (reader.NodeType == XmlNodeType.Whitespace)
						continue;
					if (reader.NodeType != XmlNodeType.Element)
					{
						Debug.WriteLine("Unhandled node {0} {1} {2}", reader.NodeType, reader.Name, reader.Value);
						continue;
					}

					var rootnode = new RuntimeRootNode (new XmlType (reader.NamespaceURI, reader.Name, null), view, (IXmlNamespaceResolver)reader);
					XamlParser.ParseXaml (rootnode, reader);
					Visit (rootnode, new HydrationContext {
						RootElement = view,
#pragma warning disable 0618
						ExceptionHandler = ResourceLoader.ExceptionHandler ?? (Internals.XamlLoader.DoNotThrowOnExceptions ? e => { }: (Action<Exception>)null)
#pragma warning restore 0618
					});
					break;
				}
			}
		}

		[Obsolete ("Use the XamlFileProvider to provide xaml files. We will remove this when Cycle 8 hits Stable.")]
		public static object Create (string xaml, bool doNotThrow = false)
		{
			object inflatedView = null;
			using (var textreader = new StringReader(xaml))
			using (var reader = XmlReader.Create (textreader)) {
				while (reader.Read ()) {
					//Skip until element
					if (reader.NodeType == XmlNodeType.Whitespace)
						continue;
					if (reader.NodeType != XmlNodeType.Element) {
						Debug.WriteLine ("Unhandled node {0} {1} {2}", reader.NodeType, reader.Name, reader.Value);
						continue;
					}

					var rootnode = new RuntimeRootNode (new XmlType (reader.NamespaceURI, reader.Name, null), null, (IXmlNamespaceResolver)reader);
					XamlParser.ParseXaml (rootnode, reader);
					var visitorContext = new HydrationContext {
						ExceptionHandler = doNotThrow ? e => { } : (Action<Exception>)null,
					};
					var cvv = new CreateValuesVisitor (visitorContext);
					cvv.Visit ((ElementNode)rootnode, null);
					inflatedView = rootnode.Root = visitorContext.Values [rootnode];
					visitorContext.RootElement = inflatedView as BindableObject;

					Visit (rootnode, visitorContext);
					break;
				}
			}
			return inflatedView;
		}

		static void Visit (RootNode rootnode, HydrationContext visitorContext)
		{
			rootnode.Accept (new XamlNodeVisitor ((node, parent) => node.Parent = parent), null); //set parents for {StaticResource}
			rootnode.Accept (new ExpandMarkupsVisitor (visitorContext), null);
			rootnode.Accept (new PruneIgnoredNodesVisitor(), null);
			rootnode.Accept (new NamescopingVisitor (visitorContext), null); //set namescopes for {x:Reference}
			rootnode.Accept (new CreateValuesVisitor (visitorContext), null);
			rootnode.Accept (new RegisterXNamesVisitor (visitorContext), null);
			rootnode.Accept (new FillResourceDictionariesVisitor (visitorContext), null);
			rootnode.Accept (new ApplyPropertiesVisitor (visitorContext, true), null);
		}

		static string GetXamlForType(Type type)
		{
			//the Previewer might want to provide it's own xaml for this... let them do that
			//the check at the end is preferred (using ResourceLoader). keep this until all the previewers are updated

			string xaml;
#pragma warning disable 0618
			if (ResourceLoader.ResourceProvider == null && (xaml = Internals.XamlLoader.XamlFileProvider?.Invoke(type)) != null)
				return xaml;
#pragma warning restore 0618

			var typeInfo = type.GetTypeInfo();
			var assembly = typeInfo.Assembly;
			var resourceId = typeInfo.GetCustomAttribute<XamlResourceIdAttribute>()?.ResourceId;
			if (resourceId == null)
				return null;

			using (var stream = assembly.GetManifestResourceStream(resourceId)) {
				if (stream != null)
					using (var reader = new StreamReader(stream))
						xaml = reader.ReadToEnd();
				else
					xaml = null;
			}

			var alternateXaml = ResourceLoader.ResourceProvider?.Invoke(resourceId);
			return alternateXaml ?? xaml;
		}

		public class RuntimeRootNode : RootNode
		{
			public RuntimeRootNode(XmlType xmlType, object root, IXmlNamespaceResolver resolver) : base (xmlType, resolver)
			{
				Root = root;
			}

			public object Root { get; internal set; }
		}
	}
}