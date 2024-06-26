using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.ClassicModelAssertUsage
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public sealed class IsTrueAndTrueClassicModelAssertUsageCondensedCodeFix
        : ClassicModelAssertUsageCodeFix
    {
        public const string Suffix = " (condensed)";

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AnalyzerIdentifiers.IsTrueUsage,
            AnalyzerIdentifiers.TrueUsage);

        protected override int MinimumNumberOfParameters { get; } = 1;

        protected override string Title => base.Title + Suffix;

        protected override (ArgumentSyntax ActualArgument, ArgumentSyntax? ConstraintArgument) ConstructActualAndConstraintArguments(
            Diagnostic diagnostic,
            IReadOnlyDictionary<string, ArgumentSyntax> argumentNamesToArguments)
        {
            var actualArgument = argumentNamesToArguments[NUnitFrameworkConstants.NameOfConditionParameter].WithNameColon(null);
            return (actualArgument, null); // The condensed form doesn't have the constraint argument.
        }
    }
}
