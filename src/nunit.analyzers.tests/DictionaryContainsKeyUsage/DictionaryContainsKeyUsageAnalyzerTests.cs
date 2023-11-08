using System.Collections;
using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.DictionaryContainsKeyUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DictionaryContainsKeyUsage;

[TestFixtureSource(nameof(ConstraintExpressions))]
public class DictionaryContainsKeyUsageAnalyzerTests
{
    private static readonly DiagnosticAnalyzer analyzer = new DictionaryContainsKeyUsageAnalyzer();
    private static readonly string[] ConstraintExpressions = { "Does.ContainKey", "Contains.Key" };
    private static readonly ExpectedDiagnostic actualIsNotDictionaryDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.DictionaryContainsKeyActualIsNotDictionary);

    // private static readonly ExpectedDiagnostic incompatibleTypesDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.DictionaryContainsKeyActualIsNotDictionary);

    private readonly string constraintExpression;

    public DictionaryContainsKeyUsageAnalyzerTests(string constraintExpression)
    {
        this.constraintExpression = constraintExpression;
    }

/*    [Test]
    public void Blah()
    {
        Assert.That(analyzer, Is.Not.Null);
        var dict = new Dictionary<int, int> { { 1, 3 } };
        var mock = new MockDictionary<int, int>(dict);
        Assert.That(mock, Does.ContainKey(1));
    }*/

    [Test]
    public void AnalyzeWhenNonDictionaryActualArgumentProvided()
    {
        var testCode = TestUtility.WrapInTestMethod($"Assert.That(123, {this.constraintExpression}(1));");

        RoslynAssert.Diagnostics(analyzer,
                                 actualIsNotDictionaryDiagnostic.WithMessage($"The '{this.constraintExpression}' constraint cannot be used with actual argument of type 'int'"),
                                 testCode);
    }

    private class MockDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> dictionary;

        public MockDictionary(IDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary;
        }

        public int Count => this.dictionary.Count;

        public bool IsReadOnly => this.dictionary.IsReadOnly;

        public ICollection<TKey> Keys => this.dictionary.Keys;

        public ICollection<TValue> Values => this.dictionary.Values;

        public TValue this[TKey key]
        {
            get => this.dictionary[key];
            set => this.dictionary[key] = value;
        }

        public bool ContainsKey(TKey key)
        {
            return this.dictionary.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            this.dictionary.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            return this.dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.dictionary.TryGetValue(key, out value!);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.dictionary.Add(item);
        }

        public void Clear()
        {
            this.dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.dictionary.Remove(item);
        }
    }

    private class MockReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly IReadOnlyDictionary<TKey, TValue> dictionary;

        public MockReadOnlyDictionary(IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary;
        }

        public int Count => this.dictionary.Count;

        public IEnumerable<TKey> Keys => this.dictionary.Keys;

        public IEnumerable<TValue> Values => this.dictionary.Values;

        public TValue this[TKey key] => this.dictionary[key];

        public bool ContainsKey(TKey key)
        {
            return this.dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.dictionary.TryGetValue(key, out value!);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.dictionary).GetEnumerator();
        }
    }
}
