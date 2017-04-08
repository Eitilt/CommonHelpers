/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace AgEitilt.Common.Dictionary {
	/// <summary>
	/// Represents a collection of keys and values while notifying listeners
	/// when those contents change.
	/// </summary>
	/// 
	/// <remarks>
	/// Retrieving a value by using its key is very fast, close to <c>O(1)</c>,
	/// because the underlying <see cref="Dictionary{TKey, TValue}"/> class is
	/// implemented as a hash table.
	/// <para/>
	/// Each element is a key/value pair stored in a
	/// <see cref="KeyValuePair{TKey, TValue}"/> object. When iterated using a
	/// <c>foreach</c> loop (<c>For Each</c> in Visual Basic, <c>for each</c>
	/// in C++), the enumerated objects are therefore of that type rather than
	/// either <typeparamref name="TKey"/> or <typeparamref name="TValue"/>.
	/// <para/>
	/// As long as an object is used as a key in the dictionary, it must not
	/// change in any way that affects its hash value. Every key must be
	/// unique according to the instance's equality comparer. A key cannot be
	/// <c>null</c>, but a value can be if the value type
	/// <typeparamref name="TKey"/> is a reference type.
	/// <para/>
	/// <see cref="ObservableDictionary{TKey, TValue}"/> requires an equality
	/// implementation to determine whether keys are equal. You can specify an
	/// implementation of the <see cref="IEqualityComparer{T}"/> generic
	/// interface by using a constructor that accepts a comparer parameter; if
	/// you do not specify an implementation, the default generic equality
	/// comparer is used. If type <typeparamref name="TKey"/> implements the
	/// <see cref="IEquatable{T}"/> generic interface, the default equality
	/// comparer uses that implementation.
	/// </remarks>
	/// 
	/// <typeparam name="TKey">
	/// The type of keys in the dictionary.
	/// </typeparam>
	/// <typeparam name="TValue">
	/// The type of values in the dictionary.
	/// </typeparam>
	public class ObservableDictionary<TKey, TValue> : ObservableDictionaryBase<TKey, TValue> {
		/// <summary>
		/// The underlying item store.
		/// </summary>
		private Dictionary<TKey, TValue> dictionary;
		/// <summary>
		/// Retrieve a reference to the underlying
		/// <see cref="IDictionary{TKey, TValue}"/> used by the particular
		/// instance as determined by the implementation.
		/// </summary>
		protected override IDictionary<TKey, TValue> Dictionary =>
			dictionary;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="ObservableDictionary{TKey, TValue}"/> class that is
		/// empty, has the default initial capacity, and uses the default
		/// equality comparer for the key type.
		/// </summary>
		/// 
		/// <remarks>
		/// This constructor is an <c>O(1)</c> operation.
		/// </remarks>
		public ObservableDictionary() =>
			dictionary = new Dictionary<TKey, TValue>();
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="ObservableDictionary{TKey, TValue}"/> class that is
		/// empty, has the specified initial capacity, and uses the default
		/// equality comparer for the key type.
		/// </summary>
		/// 
		/// <remarks>
		/// This constructor is an <c>O(1)</c> operation.
		/// </remarks>
		/// 
		/// <param name="capacity">
		/// The initial number of elements that the dictionary can contain
		/// before needing to be expanded.
		/// </param>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="capacity"/> is less than 0.
		/// </exception>
		public ObservableDictionary(int capacity) =>
			dictionary = new Dictionary<TKey, TValue>(capacity);
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="ObservableDictionary{TKey, TValue}"/> class that is
		/// empty, has the default initial capacity, and uses the specified
		/// <see cref="IEqualityComparer{T}"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// This constructor is an <c>O(1)</c> operation.
		/// </remarks>
		/// 
		/// <param name="comparer">
		/// The <see cref="IEqualityComparer{T}"/> implementation to use when
		/// comparing keys, or <c>null</c> to use the default
		/// <see cref="IEqualityComparer{T}"/> for the type of the key.
		/// </param>
		public ObservableDictionary(IEqualityComparer<TKey> comparer) =>
			dictionary = new Dictionary<TKey, TValue>(comparer);
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="ObservableDictionary{TKey, TValue}"/> class that
		/// contains elements copied from the specified
		/// <see cref="IDictionary{TKey, TValue}"/> and uses the default
		/// equality comparer for the key type.
		/// </summary>
		/// 
		/// <remarks>
		/// The initial capacity of the new
		/// <see cref="ObservableDictionary{TKey, TValue}"/> is large enough
		/// to contain all the elements in <paramref name="dictionary"/>.
		/// <para/>
		/// This constructor is an <c>O(n)</c> operation, where <c>n</c> is
		/// the number of elements in <paramref name="dictionary"/>.
		/// </remarks>
		/// 
		/// <param name="dictionary">
		/// The <see cref="IDictionary{TKey, TValue}"/> whose elements are
		/// copied to the new <see cref="ObservableDictionary{TKey, TValue}"/>.
		/// </param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="dictionary"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="dictionary"/> contains one or more duplicate keys.
		/// </exception>
		public ObservableDictionary(IDictionary<TKey, TValue> dictionary) =>
			this.dictionary = new Dictionary<TKey, TValue>(dictionary);
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="ObservableDictionary{TKey, TValue}"/> class that is
		/// empty, has the specified initial capacity, and uses the specified
		/// <see cref="IEqualityComparer{T}"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// This constructor is an <c>O(1)</c> operation.
		/// </remarks>
		/// 
		/// <param name="capacity">
		/// The initial number of elements that the dictionary can contain
		/// before needing to be expanded.
		/// </param>
		/// <param name="comparer">
		/// The <see cref="IEqualityComparer{T}"/> implementation to use when
		/// comparing keys, or <c>null</c> to use the default
		/// <see cref="IEqualityComparer{T}"/> for the type of the key.
		/// </param>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="capacity"/> is less than 0.
		/// </exception>
		public ObservableDictionary(int capacity, IEqualityComparer<TKey> comparer) =>
			dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="ObservableDictionary{TKey, TValue}"/> class that
		/// contains elements copied from the specified
		/// <see cref="IDictionary{TKey, TValue}"/> and uses the specified
		/// <see cref="IEqualityComparer{T}"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// The initial capacity of the new
		/// <see cref="ObservableDictionary{TKey, TValue}"/> is large enough
		/// to contain all the elements in <paramref name="dictionary"/>.
		/// <para/>
		/// This constructor is an <c>O(n)</c> operation, where <c>n</c> is
		/// the number of elements in <paramref name="dictionary"/>.
		/// </remarks>
		/// 
		/// <param name="dictionary">
		/// The <see cref="IDictionary{TKey, TValue}"/> whose elements are
		/// copied to the new <see cref="ObservableDictionary{TKey, TValue}"/>.
		/// </param>
		/// <param name="comparer">
		/// The <see cref="IEqualityComparer{T}"/> implementation to use when
		/// comparing keys, or <c>null</c> to use the default
		/// <see cref="IEqualityComparer{T}"/> for the type of the key.
		/// </param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="dictionary"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="dictionary"/> contains one or more duplicate keys.
		/// </exception>
		public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) =>
			this.dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);

		/// <summary>
		/// Gets the <see cref="IEqualityComparer{T}"/> that is used to
		/// determine equality of keys for the dictionary. 
		/// </summary>
		/// 
		/// <remarks>
		/// Getting the value of this property is an <c>O(1)</c> operation.
		/// </remarks>
		/// 
		/// <value>
		/// The <see cref="IEqualityComparer{T}"/> generic interface
		/// implementation that is used to determine equality of keys for the
		/// current <see cref="ObservableDictionary{TKey, TValue}"/> and to
		/// provide hash values for the keys.
		/// </value>
		public IEqualityComparer<TKey> Comparer =>
			dictionary.Comparer;

		/// <summary>
		/// Gets a collection containing the keys in the
		/// <see cref="Dictionary{TKey, TValue}"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// The order of the keys in the
		/// <see cref="Dictionary{TKey, TValue}.KeyCollection"/> is
		/// unspecified, but it is the same order as the associated values in
		/// the <see cref="Dictionary{TKey, TValue}.ValueCollection"/> 
		/// returned by the <see cref="Values"/> property.
		/// <para/>
		/// The returned <see cref="Dictionary{TKey, TValue}.KeyCollection"/>
		/// is not a static copy; instead, the
		/// <see cref="Dictionary{TKey, TValue}.KeyCollection"/> refers back
		/// to the keys in the original <see cref="Dictionary{TKey, TValue}"/>.
		/// Therefore, changes to the <see cref="Dictionary{TKey, TValue}"/>
		/// continue to be reflected in the
		/// <see cref="Dictionary{TKey, TValue}.KeyCollection"/>.
		/// <para/>
		/// Getting the value of this property is an <c>O(1)</c> operation.
		/// </remarks>
		/// 
		/// <value>
		/// A <see cref="Dictionary{TKey, TValue}.KeyCollection"/> containing
		/// the keys in the <see cref="Dictionary{TKey, TValue}"/>.
		/// </value>
		public new Dictionary<TKey, TValue>.KeyCollection Keys =>
			dictionary.Keys;

		/// <summary>
		/// Gets a collection containing the values in the
		/// <see cref="Dictionary{TKey, TValue}"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// The order of the values in the
		/// <see cref="Dictionary{TKey, TValue}.ValueCollection"/> is
		/// unspecified, but it is the same order as the associated values in
		/// the <see cref="Dictionary{TKey, TValue}.KeyCollection"/> 
		/// returned by the <see cref="Keys"/> property.
		/// <para/>
		/// The returned <see cref="Dictionary{TKey, TValue}.ValueCollection"/>
		/// is not a static copy; instead, the
		/// <see cref="Dictionary{TKey, TValue}.ValueCollection"/> refers back
		/// to the values in the original
		/// <see cref="Dictionary{TKey, TValue}"/>. Therefore, changes to the
		/// <see cref="Dictionary{TKey, TValue}"/> continue to be reflected in
		/// the <see cref="Dictionary{TKey, TValue}.ValueCollection"/>.
		/// <para/>
		/// Getting the value of this property is an <c>O(1)</c> operation.
		/// </remarks>
		/// 
		/// <value>
		/// A <see cref="Dictionary{TKey, TValue}.ValueCollection"/> containing
		/// the values in the <see cref="Dictionary{TKey, TValue}"/>.
		/// </value>
		public new Dictionary<TKey, TValue>.ValueCollection Values =>
			dictionary.Values;

		/// <summary>
		/// Determines whether the <see cref="IDictionary{TKey, TValue}"/>
		/// contains an element with the specified value.
		/// </summary>
		/// 
		/// <remarks>
		/// This method is an <c>O(1)</c> operation, where <c>n</c> is
		/// <see cref="ObservableDictionaryBase{TKey, TValue}.Count"/>.
		/// </remarks>
		/// 
		/// <param name="value">
		/// The value to locate. This can be <c>null</c> for reference types.
		/// </param>
		/// 
		/// <returns>
		/// <c>true</c> if the <see cref="IDictionary{TKey, TValue}"/>
		/// contains an element with a value equal to <paramref name="value"/>;
		/// otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsValue(TValue value) =>
			dictionary.ContainsValue(value);

		/// <summary>
		/// Returns an enumerator that iterates through the key-value pairs in
		/// the dictionary.
		/// </summary>
		/// 
		/// <remarks>
		/// This method is an <c>O(1)</c> operation.
		/// </remarks>
		/// 
		/// <returns>
		/// An enumerator that can be used to iterate through the dictionary.
		/// </returns>
		public new Dictionary<TKey, TValue>.Enumerator GetEnumerator() =>
			dictionary.GetEnumerator();
	}
}
