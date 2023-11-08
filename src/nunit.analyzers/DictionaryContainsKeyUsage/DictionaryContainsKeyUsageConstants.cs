namespace NUnit.Analyzers.DictionaryContainsKeyUsage
{
    internal static class DictionaryContainsKeyUsageConstants
    {
        // TODO: Fix messages.
        public const string ActualTitle = "Wrong actual type used with the Dictionary";
        public const string ActualMessage = "The '{0}' constraint cannot be used with actual argument of type '{1}'";
        public const string ActualDescription = "The DictionaryContainsKeyConstraint requires the actual argument to be a dictionary.";

        public const string ExpectedTitle = "Incompatible types for EqualTo constraint";
        public const string ExpectedMessage = "The EqualTo constraint always fails as the actual and the expected value cannot be equal";
        public const string ExpectedDescription = "The EqualTo constraint always fails as the actual and the expected value cannot be equal.";
    }
}
