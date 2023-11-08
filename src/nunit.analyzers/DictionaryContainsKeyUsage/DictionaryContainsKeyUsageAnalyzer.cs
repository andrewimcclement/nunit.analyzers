using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.Operations;

namespace NUnit.Analyzers.DictionaryContainsKeyUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DictionaryContainsKeyUsageAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor actualNotDictionaryDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.DictionaryContainsKeyActualIsNotDictionary,
            title: DictionaryContainsKeyUsageConstants.ActualTitle,
            messageFormat: DictionaryContainsKeyUsageConstants.ActualMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Error,
            description: DictionaryContainsKeyUsageConstants.ActualDescription);
        private static readonly DiagnosticDescriptor incompatibleTypesDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.DictionaryContainsKeyIncompatibleTypes,
            title: DictionaryContainsKeyUsageConstants.ExpectedTitle,
            messageFormat: DictionaryContainsKeyUsageConstants.ExpectedMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Error,
            description: DictionaryContainsKeyUsageConstants.ExpectedDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(actualNotDictionaryDescriptor, incompatibleTypesDescriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            foreach (var constraintPart in constraintExpression.ConstraintParts)
            {
                if ((!IsDoesContainKey(constraintPart) && !IsContainsKey(constraintPart))
                    || constraintPart.Root?.Type?.GetFullMetadataName() != NUnitFrameworkConstants.FullNameOfDictionaryContainsKeyConstraint)
                {
                    continue;
                }

                if (constraintPart.HasIncompatiblePrefixes())
                {
                    return;
                }

                var expectedType = constraintPart.GetExpectedArgument()?.Type;
                var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);

                if (actualType is null || expectedType is null)
                {
                    continue;
                }

                var actualTypeDisplay = actualType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                if (!actualType.IsIReadOnlyDictionary(out var elementType, out _)
                    && !actualType.IsIReadOnlyDictionary(out elementType, out _))
                {
                    context.ReportDiagnostic(Diagnostic.Create(actualNotDictionaryDescriptor,
                                                               actualOperation.Syntax.GetLocation(),
                                                               ConstraintDiagnosticDescription(constraintPart),
                                                               actualTypeDisplay));
                    continue;
                }

                // Valid, if collection element type matches expected type.
                if (NUnitEqualityComparerHelper.CanBeEqual(elementType, expectedType, context.Compilation))
                {
                    continue;
                }

                var expectedTypeDisplay = expectedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                context.ReportDiagnostic(Diagnostic.Create(
                    incompatibleTypesDescriptor,
                    constraintPart.GetLocation(),
                    ConstraintDiagnosticDescription(constraintPart),
                    actualTypeDisplay,
                    expectedTypeDisplay));
            }
        }

        private static bool IsDoesContainKey(ConstraintExpressionPart constraintPart)
        {
            return constraintPart.GetConstraintName() == NUnitFrameworkConstants.NameOfDoesContainKey;
        }

        private static bool IsContainsKey(ConstraintExpressionPart constraintPart)
        {
            return constraintPart.HelperClass?.Name == NUnitFrameworkConstants.NameOfContains
                   && constraintPart.GetConstraintName() == NUnitFrameworkConstants.NameOfContainsKey;
        }

        private static string ConstraintDiagnosticDescription(ConstraintExpressionPart constraintPart)
        {
            return constraintPart.HelperClass?.Name is not null
                ? $"{constraintPart.HelperClass?.Name}.{constraintPart.GetConstraintName()}"
                : (constraintPart.GetConstraintName() ?? "");
        }
    }
}
