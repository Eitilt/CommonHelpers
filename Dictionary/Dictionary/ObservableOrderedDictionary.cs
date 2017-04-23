/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

#if (NET45) || (NETSTANDARD1_3)
#define SUPPORT_IORDEREDDICTIONARY
#endif

#if (!NETSTANDARD1_0 && !NETSTANDARD1_1)
#define SUPPORT_PROPERTYCHANGING_EVENT
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace AgEitilt.Common.Dictionary {
	/// <summary>
	/// A <see cref="Dictionary{TKey, TValue}"/> that additionally maintains a
	/// predictable ordering of its items as well as notifying listeners when
	/// those contents change.
	/// </summary>
	/// 
	/// <remarks>
	/// As this is built around the standard
	/// <see cref="Dictionary{TKey, TValue}"/> type, its keys may not be
	/// <c>null</c> but its values (as long as <typeparamref name="TValue"/>
	/// is a nullable type) can.
	/// <para/>
	/// Due to <see cref="int"/> being used as the index type in member
	/// overrides, it is recommended that <typeparamref name="TKey"/>
	/// <em>not</em> be given the type <see cref="int"/>, and instead that
	/// integer keys be cast to one of the other number types (such as
	/// <see cref="uint"/> or <see cref="short"/>).
	/// </remarks>
	/// 
	/// <typeparam name="TKey">
	/// The type of keys to values in the dictionary.
	/// </typeparam>
	/// <typeparam name="TValue">
	/// The type of the values stored in the dictionary.
	/// </typeparam>
	public class ObservableOrderedDictionary<TKey, TValue>
		: ObservableDictionaryBase<TKey, TValue>,
#if SUPPORT_IORDEREDDICTIONARY
		  IOrderedDictionary,
#endif
		  IList, IList<KeyValuePair<TKey, TValue>>, IReadOnlyList<KeyValuePair<TKey, TValue>> {
		/// <summary>
		/// The underlying item store.
		/// </summary>
		private OrderedDictionary<TKey, TValue> dictionary;
		/// <summary>
		/// Retrieve a reference to the underlying
		/// <see cref="IDictionary{TKey, TValue}"/> used by the particular
		/// instance as determined by the implementation.
		/// </summary>
		protected override IDictionary<TKey, TValue> Dictionary =>
			dictionary;


#region Content change event handling

#endregion

#region Constructors
		/// <summary>
		/// Initialize an empty dictionary with the default capacity and
		/// comparer for the key type.
		/// </summary>
		/// 
		/// <remarks>
		/// This constructor runs in <c>O(1) time</c>.
		/// </remarks>
		public ObservableOrderedDictionary() =>
			dictionary = new OrderedDictionary<TKey, TValue>();

		/// <summary>
		/// Initialize an empty dictionary with the specified capacity and
		/// the default comparer for the key type.
		/// </summary>
		/// 
		/// <remarks>
		/// This constructor runs in <c>O(1) time</c>.
		/// </remarks>
		/// 
		/// <param name="capacity">
		/// The initial capacity of the dictionary.
		/// </param>
		public ObservableOrderedDictionary(int capacity) =>
			dictionary = new OrderedDictionary<TKey, TValue>(capacity);

		/// <summary>
		/// Initialize a dictionary with the specified items and the default
		/// comparer for the key type.
		/// </summary>
		/// 
		/// <remarks>
		/// The initial capacity will match the number of elements in
		/// <paramref name="items"/>.
		/// <para/>
		/// This constructor runs in <c>O(n) time</c>, where <c>n</c> is the
		/// number of elements in <paramref name="items"/>.
		/// </remarks>
		/// 
		/// <param name="items">
		/// The items with which to initialize the dictionary.
		/// </param>
		public ObservableOrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items) =>
			dictionary = new OrderedDictionary<TKey, TValue>(items);

		/// <summary>
		/// Initialize an empty dictionary with the default capacity and the
		/// specified key comparer.
		/// </summary>
		/// 
		/// <remarks>
		/// This constructor runs in <c>O(1) time</c>.
		/// </remarks>
		/// 
		/// <param name="comparer">
		/// The means by which keys should be checked for equality.
		/// </param>
		public ObservableOrderedDictionary(IEqualityComparer<TKey> comparer) =>
			dictionary = new OrderedDictionary<TKey, TValue>(comparer);

		/// <summary>
		/// Initialize an empty dictionary with the specified capacity and key
		/// comparer.
		/// </summary>
		/// 
		/// <remarks>
		/// This constructor runs in <c>O(1) time</c>.
		/// </remarks>
		/// 
		/// <param name="capacity">
		/// The initial capacity of the dictionary.
		/// </param>
		/// <param name="comparer">
		/// The means by which keys should be checked for equality.
		/// </param>
		public ObservableOrderedDictionary(int capacity, IEqualityComparer<TKey> comparer) =>
			dictionary = new OrderedDictionary<TKey, TValue>(capacity, comparer);

		/// <summary>
		/// Initialize a dictionary with the specified items and key comparer.
		/// </summary>
		/// 
		/// <remarks>
		/// The initial capacity will match the number of elements in
		/// <paramref name="items"/>.
		/// <para/>
		/// This constructor runs in <c>O(n) time</c>, where <c>n</c> is the
		/// number of elements in <paramref name="items"/>.
		/// </remarks>
		/// 
		/// <param name="items">
		/// The items with which to initialize the dictionary.
		/// </param>
		/// <param name="comparer">
		/// The means by which keys should be checked for equality.
		/// </param>
		public ObservableOrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey> comparer) =>
			dictionary = new OrderedDictionary<TKey, TValue>(items, comparer);
#endregion

#region Item accessors
		/// <summary>
		/// Gets or sets the value at the specified index.
		/// </summary>
		/// 
		/// <remarks>
		/// If used as a setter and <paramref name="index"/> is equal to the
		/// number of items already in the dictionary the new item is added to
		/// the end, while if it's less, the item at that index is replaced.
		/// <para/>
		/// Getting or setting values through this property is <c>O(n/2)</c>.
		/// </remarks>
		/// 
		/// <param name="index">The index of the value to get or set.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// This is used as a setter, and the
		/// <see cref="KeyValuePair{TKey, TValue}.Key"/> of the new value is
		/// <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// This is used as a setter, and the
		/// <see cref="KeyValuePair{TKey, TValue}.Key"/> of the new value
		/// already exists in the dictionary.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index"/> is greater than or equal to 
		/// <see cref="OrderedDictionary{TKey, TValue}.Count"/>, or is less
		/// than 0.
		/// </exception>
		public KeyValuePair<TKey, TValue> this[int index] {
			get => dictionary[index];
			set => dictionary[index] = value;
		}
#if SUPPORT_IORDEREDDICTIONARY
		/// <summary>
		/// Gets or sets the value at the specified index.
		/// </summary>
		/// 
		/// <remarks>
		/// If used as a setter and <paramref name="index"/> is equal to the
		/// number of items already in the dictionary the new item is added to
		/// the end, while if it's less, the item at that index is replaced.
		/// <para/>
		/// Getting or setting values through this property is <c>O(n/2)</c>.
		/// </remarks>
		/// 
		/// <param name="index">The index of the value to get or set.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// This is used as a setter, and the
		/// <see cref="KeyValuePair{TKey, TValue}.Key"/> of the new value is
		/// <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// This is used as a setter, and the
		/// <see cref="KeyValuePair{TKey, TValue}.Key"/> of the new value
		/// already exists in the dictionary.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index"/> is greater than or equal to 
		/// <see cref="ObservableDictionaryBase{TKey, TValue}.Count"/>, or is
		/// less than 0.
		/// </exception>
		object IOrderedDictionary.this[int index] {
			get => (this as IList<KeyValuePair<TKey, TValue>>)[index];
			set => (this as IList<KeyValuePair<TKey, TValue>>)[index] = (KeyValuePair<TKey, TValue>)value;
		}
#endif
		/// <summary>
		/// Gets or sets the value at the specified index.
		/// </summary>
		/// 
		/// <remarks>
		/// If used as a setter and <paramref name="index"/> is equal to the
		/// number of items already in the dictionary the new item is added to
		/// the end, while if it's less, the item at that index is replaced.
		/// <para/>
		/// Getting or setting values through this property is <c>O(n/2)</c>.
		/// </remarks>
		/// 
		/// <param name="index">The index of the value to get or set.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// This is used as a setter, and the
		/// <see cref="KeyValuePair{TKey, TValue}.Key"/> of the new value is
		/// <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// This is used as a setter, and the
		/// <see cref="KeyValuePair{TKey, TValue}.Key"/> of the new value
		/// already exists in the dictionary.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index"/> is greater than or equal to 
		/// <see cref="ObservableDictionaryBase{TKey, TValue}.Count"/>, or is
		/// less than 0.
		/// </exception>
		object IList.this[int index] {
			get => (this as IList<KeyValuePair<TKey, TValue>>)[index];
			set => (this as IList<KeyValuePair<TKey, TValue>>)[index] = (KeyValuePair<TKey, TValue>)value;
		}

		/// <summary>
		/// Indicates whether an item with the specified key and value exists
		/// in the dictionary.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation approaches <c>O(1)</c> time.
		/// </remarks>
		/// 
		/// <param name="item">The key/value pair to search for.</param>
		/// 
		/// <returns>
		/// <c>true</c> if <paramref name="item"/> exists in the dictionary;
		/// otherwise <c>false</c>.
		/// </returns>
		/// 
		/// <exception cref="InvalidCastException">
		/// <paramref name="item"/> can not be expressed as a
		/// <see cref="KeyValuePair{TKey, TValue}"/>.
		/// </exception>
		bool IList.Contains(object item) =>
			(dictionary as IList).Contains(item);

#if SUPPORT_IORDEREDDICTIONARY
		/// <summary>
		/// Get an object allowing iteration through the dictionary, in the
		/// proper order.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation runs in <c>O(1)</c> time.
		/// </remarks>
		/// 
		/// <returns>An enumerator over the dictionary.</returns>
		IDictionaryEnumerator IOrderedDictionary.GetEnumerator() =>
			(dictionary as IOrderedDictionary)?.GetEnumerator();
#endif

		/// <summary>
		/// Returns the index of the item associated with the specified key.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation runs in <c>O(n)</c> time.
		/// </remarks>
		/// 
		/// <param name="key">The key of the item to look up.</param>
		/// 
		/// <returns>
		/// The index of the item associated with <paramref name="key"/>, or
		/// <c>-1</c> if no such item exists in the dictionary.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		public int IndexOf(TKey key) =>
			dictionary.IndexOf(key);
		/// <summary>
		/// Returns the index of the item with the specified key and value.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation runs in <c>O(n)</c> time.
		/// </remarks>
		/// 
		/// <param name="item">The item to look up.</param>
		/// 
		/// <returns>
		/// The index of the item represented by <paramref name="item"/>, or
		/// <c>-1</c> if no such item exists in the dictionary.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="item"/> or its
		/// <see cref="KeyValuePair{TKey, TValue}.Key"/> is <c>null</c>.
		/// </exception>
		public int IndexOf(KeyValuePair<TKey, TValue> item) =>
			dictionary.IndexOf(item);
		/// <summary>
		/// Returns the index of the item with the specified key and value.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation runs in <c>O(n)</c> time.
		/// </remarks>
		/// 
		/// <param name="item">The item to look up.</param>
		/// 
		/// <returns>
		/// The index of the item represented by <paramref name="item"/>, or
		/// <c>-1</c> if no such item exists in the dictionary.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="item"/> or its
		/// <see cref="KeyValuePair{TKey, TValue}.Key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="InvalidCastException">
		/// <paramref name="item"/> can not be expressed as a
		/// <see cref="KeyValuePair{TKey, TValue}"/> with the proper type
		/// parameters.
		/// </exception>
		int IList.IndexOf(object item) =>
			(dictionary as IList).IndexOf(item);
#endregion

#region Properties
		/// <summary>
		/// Indicates whether items may be added to or removed from this
		/// dictionary after creation.
		/// </summary>
		/// 
		/// <remarks>
		/// Accessing this value runs in <c>O(1)</c> time.
		/// </remarks>
		bool IList.IsFixedSize =>
			(dictionary as IDictionary).IsFixedSize;

		/// <summary>
		/// Indicates whether this dictionary can be modified after creation.
		/// </summary>
		/// 
		/// <remarks>
		/// Accessing this value runs in <c>O(1)</c> time.
		/// </remarks>
		bool IList.IsReadOnly =>
			(dictionary as IList).IsReadOnly;
		#endregion

		#region Modifiers
		/// <summary>
		/// Adds a pre-created <see cref="KeyValuePair{TKey, TValue}"/> to the
		/// <see cref="IDictionary{TKey, TValue}"/>.
		/// </summary>
		/// 
		/// <param name="item">The item to add.</param>
		public override void Add(KeyValuePair<TKey, TValue> item) =>
			SendAddEvents(new Action(() => Dictionary.Add(item)), item,
				new Func<bool>(() => Dictionary.ContainsKey(item.Key) == false), new NotifyCollectionChangedEventArgs(
					NotifyCollectionChangedAction.Add,
					item,
					Dictionary.Count
			));
		/// <summary>
		/// Add a new item to the dictionary, after all existing items.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation typically approaches <c>O(1)</c> time, except if
		/// the available capacity has been filled; in that case, it runs in
		/// <c>O(n)</c> time where <c>n</c> is 
		/// <see cref="ObservableDictionaryBase{TKey, TValue}.Count"/>.
		/// </remarks>
		/// 
		/// <param name="item">
		/// The key/value pair to add to the dictionary.
		/// </param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// The <see cref="KeyValuePair{TKey, TValue}.Key"/> of
		/// <paramref name="item"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// An item associated with the
		/// <see cref="KeyValuePair{TKey, TValue}.Key"/> of
		/// <paramref name="item"/> already exists in the dictionary.
		/// </exception>
		/// <exception cref="InvalidCastException">
		/// <paramref name="item"/> can not be expressed as a
		/// <see cref="KeyValuePair{TKey, TValue}"/> with the proper type
		/// parameters.
		/// </exception>
		int IList.Add(object item) =>
			(dictionary as IList).Add(item);

		/// <summary>
		/// Add the given key/value pair to the dictionary, at the specified
		/// index.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation approaches <c>O(1)</c> time.
		/// </remarks>
		/// 
		/// <param name="index">The index at which to add the pair.</param>
		/// <param name="item">The key/value pair to add.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// The <see cref="KeyValuePair{TKey, TValue}.Key"/> of
		/// <paramref name="item"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// An item associated with the
		/// <see cref="KeyValuePair{TKey, TValue}.Key"/> of
		/// <paramref name="item"/> already exists in the dictionary.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index"/> is greater than
		/// <see cref="ObservableDictionaryBase{TKey, TValue}.Count"/>, or is
		/// less than 0.
		/// </exception>
		public void Insert(int index, KeyValuePair<TKey, TValue> item) {
			if (index == Count) {
				dictionary.Add(item);
			} else {
				var properties = AddProperties;
				properties.UnionWith(MoveProperties);

				var items = dictionary.Skip(index);

				SendAddEvents(new Action(() => dictionary.Insert(index, item)), deferChanged: true, properties: properties,
					collectionArgs: new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Add,
						item,
						index
				));
				SendMoveEvents(null, deferChanging: true, properties: properties,
					collectionArgs: new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Move,
						items,
						index + 1,
						index
				));
			}
		}
		/// <summary>
		/// Add the given key/value pair to the dictionary, at the specified
		/// index.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation approaches <c>O(1)</c> time.
		/// </remarks>
		/// 
		/// <param name="index">The index at which to add the pair.</param>
		/// <param name="key">The key at which to add the value.</param>
		/// <param name="value">The value to add to the dictionary.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// An item associated with a key equal to <paramref name="key"/>
		/// already exists in the dictionary.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index"/> is greater than
		/// <see cref="ObservableDictionaryBase{TKey, TValue}.Count"/>, or
		/// is less than 0.
		/// </exception>
		public void Insert(int index, TKey key, TValue value) =>
			Insert(index, new KeyValuePair<TKey, TValue>(key, value));
		/// <summary>
		/// Add the given key/value pair to the dictionary, at the specified
		/// index.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation approaches <c>O(1)</c> time.
		/// </remarks>
		/// 
		/// <param name="index">The index at which to add the pair.</param>
		/// <param name="key">The key at which to add the value.</param>
		/// <param name="value">The value to add to the dictionary.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// An item associated with a key equal to <paramref name="key"/>
		/// already exists in the dictionary.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index"/> is greater than
		/// <see cref="ObservableDictionaryBase{TKey, TValue}.Count"/>, or
		/// is less than 0.
		/// </exception>
		/// <exception cref="InvalidCastException">
		/// <paramref name="key"/> can not be expressed as a <c>TKey</c>, or
		/// <paramref name="value"/> can not be expressed as a <c>TValue</c>.
		/// </exception>
		public void Insert(int index, object key, object value) =>
			Insert(index, new KeyValuePair<TKey, TValue>((TKey)key, (TValue)value));
		/// <summary>
		/// Add the given key/value pair to the dictionary, at the specified
		/// index.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation approaches <c>O(1)</c> time.
		/// </remarks>
		/// 
		/// <param name="index">The index at which to add the pair.</param>
		/// <param name="item">The key/value pair to add.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// The <see cref="KeyValuePair{TKey, TValue}.Key"/> of
		/// <paramref name="item"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// An item associated with the same key as <paramref name="item"/>
		/// already exists in the dictionary.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index"/> is greater than
		/// <see cref="ObservableDictionaryBase{TKey, TValue}.Count"/>, or
		/// is less than 0.
		/// </exception>
		/// <exception cref="InvalidCastException">
		/// <paramref name="item"/> can not be expressed as a
		/// <see cref="KeyValuePair{TKey, TValue}"/> with the proper type
		/// parameters.
		/// </exception>
		void IList.Insert(int index, object item) =>
			Insert(index, (KeyValuePair<TKey, TValue>)item);

		/// <summary>
		/// Removes the element with the specified key from the
		/// <see cref="IDictionary{TKey, TValue}"/>.
		/// </summary>
		/// 
		/// <param name="key">The key of the element to remove.</param>
		/// 
		/// <returns>
		/// <c>true</c> if the element is successfully removed; otherwise,
		/// <c>false</c>. This method also returns <c>false</c> if
		/// <paramref name="key"/> was not found in the original
		/// <see cref="IDictionary{TKey, TValue}"/>.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The <see cref="IDictionary{TKey, TValue}"/> is read-only.
		/// </exception>
		public override bool Remove(TKey key) {
			if (dictionary.ContainsKey(key) == false) {
				// Throw proper exceptions
				return dictionary.Remove(key);
			} else {
				var properties = RemoveProperties;
				properties.UnionWith(MoveProperties);

				var index = dictionary.IndexOf(key);

				SendRemoveEvents(null, deferChanged: true, properties: properties,
					collectionArgs: new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Remove,
						new KeyValuePair<TKey, TValue>(key, dictionary[key]),
						index
				));
				return SendMoveEvents(new Action(() => dictionary.Remove(key)), deferChanging: true, properties: properties,
					collectionArgs: new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Move,
						dictionary.Skip(index),
						index,
						index + 1
				)).Item1;
			}
		}

		/// <summary>
		/// Remove the specified key/value pair from the dictionary, if it
		/// exists.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation approaches <c>O(1)</c> time.
		/// </remarks>
		/// 
		/// <param name="value">The key/value pair to remove.</param>
		/// 
		/// <returns>
		/// <c>true</c> if the item was removed successfully; otherwise
		/// <c>false</c> (including if the item did not exist).
		/// </returns>
		void IList.Remove(object value) {
			var item = (KeyValuePair<TKey, TValue>)value;

			if (Dictionary.Contains(item))
				Remove(item.Key);
		}

		/// <summary>
		/// Remove the item at the specified index from the dictionary.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation runs in <c>O(n/2)</c> time.
		/// </remarks>
		/// 
		/// <param name="index">The index of the item to remove.</param>
		/// 
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index"/> is greater than or equal to 
		/// <see cref="ObservableDictionaryBase{TKey, TValue}.Count"/>, or is
		/// less than 0.
		/// </exception>
		public void RemoveAt(int index) {
			if (index == Count) {
				dictionary.RemoveAt(index);
			} else {
				var properties = RemoveProperties;
				properties.UnionWith(MoveProperties);

				var item = dictionary[index];

				SendRemoveEvents(new Action(() => dictionary.RemoveAt(index)), deferChanged: true, properties: properties,
					collectionArgs: new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Remove,
						item,
						index
				));
				SendMoveEvents(null, deferChanging: true, properties: properties,
					collectionArgs: new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Move,
						dictionary.Skip(index),
						index,
						index + 1
				));
			}
		}
#endregion
	}
}
