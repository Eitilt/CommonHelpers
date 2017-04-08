#if (!NETSTANDARD1_0)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AgEitilt.Common.Dictionary {
	/// <summary>
	/// Represents a collection of keys and values while notifying listeners
	/// when those contents change.
	/// </summary>
	/// 
	/// <typeparam name="TKey">
	/// The type of keys in the dictionary.
	/// </typeparam>
	/// <typeparam name="TValue">
	/// The type of values in the dictionary.
	/// </typeparam>
	public class ConcurrentObservableDictionary<TKey, TValue> : ObservableDictionaryBase<TKey, TValue> {
		/// <summary>
		/// The underlying item store.
		/// </summary>
		private ConcurrentDictionary<TKey, TValue> dictionary;
		/// <summary>
		/// Retrieve a reference to the underlying
		/// <see cref="IDictionary{TKey, TValue}"/> used by the particular
		/// instance as determined by the implementation.
		/// </summary>
		protected override IDictionary<TKey, TValue> Dictionary =>
			dictionary;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="ConcurrentObservableDictionary{TKey, TValue}"/> class
		/// that is empty, has the default initial capacity, and uses the
		/// default equality comparer for the key type.
		/// </summary>
		public ConcurrentObservableDictionary() =>
			dictionary = new ConcurrentDictionary<TKey, TValue>();
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="ConcurrentObservableDictionary{TKey, TValue}"/> class
		/// that contains elements copied from the specified collection of
		/// key/value pairs and uses the default equality comparer for the key
		/// type.
		/// </summary>
		/// 
		/// <remarks>
		/// The initial capacity of the new
		/// <see cref="ConcurrentObservableDictionary{TKey, TValue}"/> is
		/// large enough to contain all the elements in
		/// <paramref name="collection"/>.
		/// </remarks>
		/// 
		/// <param name="collection">
		/// The elements which should be copied to the new
		/// <see cref="ConcurrentObservableDictionary{TKey, TValue}"/>.
		/// </param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="collection"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="collection"/> contains one or more duplicate keys.
		/// </exception>
		public ConcurrentObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) =>
			dictionary = new ConcurrentDictionary<TKey, TValue>(collection);
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="ConcurrentObservableDictionary{TKey, TValue}"/> class
		/// that is empty, has the default initial capacity, and uses the
		/// specified <see cref="IEqualityComparer{T}"/>.
		/// </summary>
		/// 
		/// <param name="comparer">
		/// The <see cref="IEqualityComparer{T}"/> implementation to use when
		/// comparing keys, or <c>null</c> to use the default
		/// <see cref="IEqualityComparer{T}"/> for the type of the key.
		/// </param>
		public ConcurrentObservableDictionary(IEqualityComparer<TKey> comparer) =>
			dictionary = new ConcurrentDictionary<TKey, TValue>(comparer);
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="ConcurrentObservableDictionary{TKey, TValue}"/> class
		/// that contains elements copied from the specified collection of
		/// key/value pairs and uses the specified
		/// <see cref="IEqualityComparer{T}"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// The initial capacity of the new
		/// <see cref="ObservableDictionary{TKey, TValue}"/> is large enough
		/// to contain all the elements in <paramref name="collection"/>.
		/// </remarks>
		/// 
		/// <param name="collection">
		/// The elements which should be copied to the new
		/// <see cref="ConcurrentObservableDictionary{TKey, TValue}"/>.
		/// </param>
		/// <param name="comparer">
		/// The <see cref="IEqualityComparer{T}"/> implementation to use when
		/// comparing keys, or <c>null</c> to use the default
		/// <see cref="IEqualityComparer{T}"/> for the type of the key.
		/// </param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="collection"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="collection"/> contains one or more duplicate keys.
		/// </exception>
		public ConcurrentObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) =>
			dictionary = new ConcurrentDictionary<TKey, TValue>(collection, comparer);
		//TODO: Add `ConcurrentObservableDictionary(int, int)`,
		// `ConcurrentObservableDictionary(int, IEnumerable<KeyValuePair<TKey,TValue>>, IEqualityComparer<TKey>)`,
		// and `ConcurrentDictionary(int, int, IEqualityComparer<TKey>)`

		/// <summary>
		/// Simplifies testing whether the dictionary contains no items.
		/// </summary>
		/// 
		/// <value>
		/// <c>true</c> if no key/value pair exists in the dictionary;
		/// otherwise <c>false</c>.
		/// </value>
		public bool IsEmpty =>
			dictionary.IsEmpty;

		/// <summary>
		/// Add the item generated by <paramref name="updateValueFactory"/> to
		/// the dictionary if <paramref name="key"/> already exists in it, or
		/// that generated by <paramref name="addValueFactory"/> otherwise;
		/// returning the respective value.
		/// </summary>
		/// 
		/// <param name="key">
		/// The object to use as the key of the element to add or update.
		/// </param>
		/// <param name="addValueFactory">
		/// The function used to generate a value if the dictionary did not
		/// contain <paramref name="key"/>.
		/// </param>
		/// <param name="updateValueFactory">
		/// The function used to generate a value if the dictionary already
		/// contained <paramref name="key"/>.
		/// </param>
		/// 
		/// <returns>
		/// The value generated by <paramref name="updateValueFactory"/> if
		/// the key already existed; otherwise that generated by
		/// <paramref name="addValueFactory"/>.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory) {
			Func<TValue> action = () =>
				dictionary.AddOrUpdate(key, addValueFactory, updateValueFactory);
			if (ContainsKey(key)) {
				var oldValue = this[key];
				var newValue = updateValueFactory(key, oldValue);
				return SendReplaceEvents(action, key, newValue, oldValue);
			} else {
				var newValue = addValueFactory(key);
				return SendAddEvents(action, key, newValue);
			}
		}
		/// <summary>
		/// Add <paramref name="addValue"/> to the dictionary if
		/// <paramref name="key"/> already exists in it, or
		/// <paramref name="addValue"/> otherwise; returning the respective
		/// value.
		/// </summary>
		/// 
		/// <param name="key">
		/// The object to use as the key of the element to add or update.
		/// </param>
		/// <param name="addValue">
		/// The value to add if the dictionary did not contain
		/// <paramref name="key"/>.
		/// </param>
		/// <param name="updateValueFactory">
		/// The function used to generate a value if the dictionary already
		/// contained <paramref name="key"/>.
		/// </param>
		/// 
		/// <returns>
		/// The value generated by <paramref name="updateValueFactory"/> if
		/// the key already existed; otherwise <paramref name="addValue"/>.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory) =>
			AddOrUpdate(key, (_ => addValue), updateValueFactory);

		/// <summary>
		/// Retrieve the requested value from the dictionary, creating a new
		/// entry if necessary.
		/// </summary>
		/// 
		/// <param name="key">The dictionary key to access.</param>
		/// <param name="valueFactory">
		/// The function used to generate a value if the dictionary did not
		/// contain <paramref name="key"/>.
		/// </param>
		/// 
		/// <returns>
		/// The object located at <paramref name="key"/> in the dictionary.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) {
			if (ContainsKey(key)) {
				return dictionary.GetOrAdd(key, valueFactory);
			} else {
				return SendAddEvents(
					(() => dictionary.GetOrAdd(key, valueFactory)),
					key,
					valueFactory(key)
				);
			}
		}
		/// <summary>
		/// Retrieve the requested value from the dictionary, creating a new
		/// entry if necessary.
		/// </summary>
		/// 
		/// <param name="key">The dictionary key to access.</param>
		/// <param name="value">
		/// The value to add if <paramref name="key"/> was not in the
		/// dictionary.
		/// </param>
		/// 
		/// <returns>
		/// The object located at <paramref name="key"/> in the dictionary.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		public TValue GetOrAdd(TKey key, TValue value) =>
			GetOrAdd(key, (_ => value));

		/// <summary>
		/// Copies the key/value pairs comprising the dictionary to an array.
		/// </summary>
		/// 
		/// <returns>The array of key/value pairs.</returns>
		public KeyValuePair<TKey, TValue>[] ToArray() =>
			dictionary.ToArray();

		/// <summary>
		/// Add a value to the dictionary as long as the desired key does not
		/// already exist.
		/// </summary>
		/// 
		/// <param name="key">
		/// The object to use as the key of the element to add.
		/// </param>
		/// <param name="value">
		/// The object to use as the value of the element to add.
		/// </param>
		/// 
		/// <returns>
		/// <c>true</c> if the value was added successfully, or false if
		/// <paramref name="key"/> already exists.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		public bool TryAdd(TKey key, TValue value) =>
			SendAddEvents((() => ContainsKey(key) == false), (() => dictionary.TryAdd(key, value)), key, value);

		/// <summary>
		/// Remove a value from the dictionary if the given key exists.
		/// </summary>
		/// 
		/// <param name="key">The dictionary key to remove.</param>
		/// <param name="value">
		/// Once this method completes, the value previously associated with
		/// <paramref name="key"/> if it existed in the dictionary, or the
		/// default value of <typeparamref name="TValue"/> otherwise.
		/// </param>
		/// 
		/// <returns>
		/// <c>true</c> if the value was removed successfully, or false if
		/// <paramref name="key"/> did not exist.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		public bool TryRemove(TKey key, out TValue value) {
			TryGetValue(key, out value);
			/* May have a slight issue if another thread updates the value at
			 * this exact point, and if that becomes an issue, can add a
			 * finer-grained Send* mixing `bool` and `TValue` lambdas.
			 */
			return SendRemoveEvents(
				(() => ContainsKey(key)),
				(() => dictionary.TryRemove(key, out _)),
				key,
				value
			);
		}

		/// <summary>
		/// Replace a value in the dictionary if the given key exists and the
		/// existing value is equal to a desired comparison.
		/// </summary>
		/// 
		/// <param name="key">The dictionary key to replace.</param>
		/// <param name="newValue">
		/// The value to update the key with, if all conditions are met.
		/// </param>
		/// <param name="comparisonValue">
		/// The value to use for testing the equality of the existing key.
		/// </param>
		/// 
		/// <returns>
		/// <c>true</c> if the value was replaced successfully, or false if
		/// <paramref name="key"/> did not exist or the existing value is not
		/// equal to <paramref name="comparisonValue"/>.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue) {
			TryGetValue(key, out var oldValue);
			/* May have a slight issue if another thread updates the value at
			 * this exact point, and if that becomes an issue, can add a
			 * finer-grained Send* mixing `bool` and `TValue` lambdas.
			 */
			return SendReplaceEvents(
				(() => ContainsKey(key) && this[key].Equals(comparisonValue)),
				(() => dictionary.TryUpdate(key, newValue, comparisonValue)),
				key,
				newValue,
				oldValue
			);
		}
	}
}

#endif