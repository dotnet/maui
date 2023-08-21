// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System;

namespace Microsoft.Maui
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public class MissingMapperAttribute : Attribute
	{
		public MissingMapperAttribute()
		{

		}

		public MissingMapperAttribute(string description)
		{
			Description = description;
		}

		public string? Description { get; set; }
	}
}