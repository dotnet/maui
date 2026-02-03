namespace Microsoft.Maui.Controls.BindingSourceGen;

using static Microsoft.Maui.Controls.BindingSourceGen.UnsafeAccessorsMethodName;

public sealed record Setter(string[] PatternMatchingExpressions, string AssignmentStatement)
{
	public static Setter From(
		IEnumerable<IPathPart> path,
		string sourceVariableName = "source",
		string assignedValueExpression = "value")
	{
		string accessAccumulator = sourceVariableName;
		List<string> patternMatchingExpressions = new();
		bool skipNextConditionalAccess = false;

		void AddPatternMatchingExpression(string pattern)
		{
			var tmpVariableName = $"p{patternMatchingExpressions.Count}";
			patternMatchingExpressions.Add($"{accessAccumulator} is {pattern} {tmpVariableName}");
			accessAccumulator = tmpVariableName;
		}

		foreach (var part in path)
		{
			var skipConditionalAccess = skipNextConditionalAccess;
			skipNextConditionalAccess = false;
			bool isLastPart = part == path.Last();

			if (part is Cast { TargetType: var targetType })
			{
				AddPatternMatchingExpression(targetType.GlobalName);

				// the current `is T` expression makes sure that the value is not null
				// so if a conditional access to a member/indexer follows, we can skip the next null check
				skipNextConditionalAccess = true;
			}
			else if (part is ConditionalAccess { Part: var innerPart })
			{
				if (!skipConditionalAccess)
				{
					AddPatternMatchingExpression("{}");
				}

				accessAccumulator = AccessExpressionBuilder.ExtendExpression(accessAccumulator, innerPart);

				// If the inner part is a value type and the NEXT part is the last part, we need to capture
				// the value type in a variable to be able to set its properties.
				// We only do this when:
				// 1. Inner is value type
				// 2. Not the last part
				// 3. Next part is the last part
				// 4. Next part is NOT a ConditionalAccess or Cast (those add their own pattern match)
				// Example: Foo.Bar.Title where Bar is struct and Title is last - we need to capture Bar.
				// Counter-example: a.B?.C where B is struct - the ?.C adds a pattern match for B, so we don't need to.
				bool innerIsValueType = innerPart switch
				{
					MemberAccess { IsValueType: true } => true,
					IndexAccess { IsValueType: true } => true,
					_ => false
				};
				
				if (innerIsValueType && !isLastPart)
				{
					var pathList = path.ToList();
					int currentIndex = pathList.IndexOf(part);
					bool nextPartIsLast = currentIndex >= 0 && currentIndex == pathList.Count - 2;
					bool nextPartAddsPatternMatch = currentIndex + 1 < pathList.Count && 
						(pathList[currentIndex + 1] is ConditionalAccess or Cast);
					
					if (nextPartIsLast && !nextPartAddsPatternMatch)
					{
						AddPatternMatchingExpression("{}");
					}
				}
			}
			else if (part is MemberAccess { IsValueType: true } && !isLastPart)
			{
				// It is necessary to create a variable for value types in order to set their properties.
				// We can simply reuse the pattern matching mechanism to declare the variable.
				accessAccumulator = AccessExpressionBuilder.ExtendExpression(accessAccumulator, part);
				AddPatternMatchingExpression("{}");
			}
			else if (!isLastPart)
			{
				// For non-last parts, extend the expression using the getter path
				accessAccumulator = AccessExpressionBuilder.ExtendExpression(accessAccumulator, part);
			}
			// For the last part, we don't extend the expression here
			// The assignment is handled by BuildAssignmentStatement
		}

		return new Setter(
			patternMatchingExpressions.ToArray(),
			AssignmentStatement: BuildAssignmentStatement(accessAccumulator, path.Any() ? path.Last() : null, assignedValueExpression));
	}

	public static string BuildAssignmentStatement(string accessAccumulator, IPathPart? lastPart, string assignedValueExpression = "value") =>
		lastPart switch
		{
			MemberAccess { Kind: AccessorKind.Field, IsSetterInaccessible: true } memberAccess => $"{CreateUnsafeFieldAccessorMethodName(memberAccess.MemberName)}({accessAccumulator}) = {assignedValueExpression};",
			MemberAccess { Kind: AccessorKind.Property, IsSetterInaccessible: true } memberAccess => $"{CreateUnsafePropertyAccessorSetMethodName(memberAccess.MemberName)}({accessAccumulator}, {assignedValueExpression});",
			MemberAccess memberAccess => $"{accessAccumulator}.{memberAccess.MemberName} = {assignedValueExpression};",
			IndexAccess indexAccess => indexAccess.Index switch
			{
				int numericIndex => $"{accessAccumulator}[{numericIndex}] = {assignedValueExpression};",
				string stringIndex => $"{accessAccumulator}[\"{stringIndex}\"] = {assignedValueExpression};",
				_ => throw new NotSupportedException($"Unsupported index type: {indexAccess.Index.GetType()}"),
			},
			_ => $"{accessAccumulator} = {assignedValueExpression};",
		};
}
