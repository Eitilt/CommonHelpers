/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

#if (NET45) || (NETSTANDARD1_3)
#define SUPPORT_IORDEREDDICTIONARY
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if SUPPORT_IORDEREDDICTIONARY
using System.Collections.Specialized;
#endif

namespace AgEitilt.Common.Dictionary {
	/// <summary>
	/// A read-only dictionary that additionally maintains a predictable
	/// ordering of its items.
	/// </summary>
	/// 
	/// <typeparam name="TKey">
	/// The type of keys to values in the dictionary.
	/// </typeparam>
	/// <typeparam name="TValue">
	/// The type of the values stored in the dictionary.
	/// </typeparam>
	public interface IReadOnlyOrderedDictionary<TKey, TValue>
		: IReadOnlyDictionary<TKey, TValue>, IReadOnlyList<KeyValuePair<TKey, TValue>> { }

	/// <summary>
	/// A <see cref="Dictionary{TKey, TValue}"/> that additionally maintains a
	/// predictable ordering of its items.
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
	/// <para/>
	/// TODO: Implement <see cref="LinkedList{T}"/>-style access.
	/// </remarks>
	/// 
	/// <typeparam name="TKey">
	/// The type of keys to values in the dictionary.
	/// </typeparam>
	/// <typeparam name="TValue">
	/// The type of the values stored in the dictionary.
	/// </typeparam>
	public class OrderedDictionary<TKey, TValue>
		: IReadOnlyOrderedDictionary<TKey, TValue>,
		  IDictionary, IDictionary<TKey, TValue>,
#if SUPPORT_IORDEREDDICTIONARY
		  IOrderedDictionary,
#endif
		  IList, IList<KeyValuePair<TKey, TValue>> {

		/// <summary>
		/// An enumerator for items in an
		/// <see cref="OrderedDictionary{TKey, TValue}"/>.
		/// </summary>
		public class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator {
			/// <summary>
			/// A reference to the dictionary's items.
			/// </summary>
			readonly LinkedList<KeyValuePair<TKey, TValue>> list;
			/// <summary>
			/// The currently-pointed-to item.
			/// </summary>
			LinkedListNode<KeyValuePair<TKey, TValue>> currentNode;
			/// <summary>
			/// Indicates that the enumerator should be considered "before the
			/// first element" in the dictionary.
			/// </summary>
			bool beforeStart = true;

			/// <summary>
			/// Initialize an enumerator over a specified dictionary.
			/// </summary>
			/// 
			/// <remarks>
			/// This constructor runs in <c>O(1)</c> time.
			/// </remarks>
			/// 
			/// <param name="dictionary">The dictionary to enumerate.</param>
			public Enumerator(OrderedDictionary<TKey, TValue> dictionary) =>
				list = dictionary.list;

			/// <summary>
			/// The item currently pointed to by the enumerator.
			/// </summary>
			public KeyValuePair<TKey, TValue> Current =>
				currentNode.Value;
			/// <summary>
			/// The item currently pointed to by the enumerator.
			/// </summary>
			object IEnumerator.Current =>
				Current;
			/// <summary>
			/// The item currently pointed to by the enumerator.
			/// </summary>
			DictionaryEntry IDictionaryEnumerator.Entry =>
				new DictionaryEntry(Current.Key, Current.Value);

			/// <summary>
			/// The key of the item currently pointed to by the enumerator.
			/// </summary>
			object IDictionaryEnumerator.Key =>
				Current.Key;

			/// <summary>
			/// The value of the item currently pointed to by the enumerator.
			/// </summary>
			object IDictionaryEnumerator.Value =>
				Current.Value;

			/// <summary>
			/// Releases all resources used by the object.
			/// </summary>
			public void Dispose() { }

			/// <summary>
			/// Increment the enumerator to point to the next element in the
			/// dictionary.
			/// </summary>
			/// 
			/// <returns>
			/// <c>true</c> if that next element exists and is successfully
			/// pointed to; otherwise <c>false</c>.
			/// </returns>
			public bool MoveNext() {
				if (beforeStart) {
					beforeStart = false;
					currentNode = list.First;
				} else {
					currentNode = currentNode.Next;
				}

				return (currentNode != null);
			}

			/// <summary>
			/// Return the enumerator to point to an element "before the
			/// start" of the dictionary.
			/// </summary>
			public void Reset() =>
				// Returning the current node to the start is handled in
				// `MoveNext()`
				beforeStart = true;
		}

		/// <summary>
		/// The underlying key-value map used to implement dictionary access.
		/// </summary>
		/// 
		/// <remarks>
		/// While this field would be simpler to maintain if its values were
		/// simply <c>LinkedListNode&lt;TValue&gt;</c> objects, using the
		/// longer (reference-typed) form allows the same objects to also be
		/// used in <see cref="list"/>, thus requiring less space and ensuring
		/// that any reordering of that list will be automatically reflected.
		/// </remarks>
		Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> dictionary;
		/// <summary>
		/// The underlying ordered list used to implement indexed access.
		/// </summary>
		LinkedList<KeyValuePair<TKey, TValue>> list = new LinkedList<KeyValuePair<TKey, TValue>>();


#region Constructors
		/// <summary>
		/// Initialize an empty dictionary with the default capacity and
		/// comparer for the key type.
		/// </summary>
		/// 
		/// <remarks>
		/// This constructor runs in <c>O(1) time</c>.
		/// </remarks>
		public OrderedDictionary() =>
			dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();

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
		public OrderedDictionary(int capacity) =>
			dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(capacity);

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
		public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items) {
			// Checking `.Count()` may iterate through `items` again, but it's
			// worth it for the classes where it instead runs in constant time
			dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(items.Count());

			PopulateDictionary(items);
		}

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
		public OrderedDictionary(IEqualityComparer<TKey> comparer) =>
			dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(comparer);

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
		public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer) =>
			dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(capacity, comparer);

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
		public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey> comparer) {
			// Checking `.Count()` may iterate through `items` again, but it's
			// worth it for the classes where it instead runs in constant time
			dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(items.Count(), comparer);

			PopulateDictionary(items);
		}

		/// <summary>
		/// Add all elements in an existing list to the dictionary.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation runs in <c>O(n)</c> time, where <c>n</c> is the
		/// number of elements in <paramref name="items"/>.
		/// </remarks>
		/// 
		/// <param name="items">The elements to add to the dictionary.</param>
		void PopulateDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items) {
			// As the `LinkedList` copy constructor runs in O(n) time anyway,
			// doing this manually isn't any significant loss
			foreach (var pair in items)
				dictionary.Add(pair.Key, list.AddLast(pair));
		}
#endregion

#region Item accessors
		/// <summary>
		/// Step through the list to find the node at an arbitrary index.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation runs in <c>O(n/2)</c> time.
		/// </remarks>
		/// 
		/// <param name="index">The index of the node to access.</param>
		/// 
		/// <returns>The node at <paramref name="index"/>.</returns>
		/// 
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index"/> is greater than or equal to 
		/// <see cref="Count"/>, or is less than 0.
		/// </exception>
		LinkedListNode<KeyValuePair<TKey, TValue>> NodeAt(int index) {
			LinkedListNode<KeyValuePair<TKey, TValue>> node;

			if (index >= Count) {
				throw new IndexOutOfRangeException(String.Format(Resources.Strings.Error_IndexTooHigh, index, (Count - 1)));
			} else if (index < 0) {
				throw new IndexOutOfRangeException(String.Format(Resources.Strings.Error_NegativeIndex, index));
			} else if (index <= (Count / 2)) {
				node = list.First;
				for (uint i = 0; i < index; ++i)
					node = node.Next;
			} else {
				node = list.Last;
				for (uint i = ((uint)Count - 1); i > index; --i)
					node = node.Previous;
			}

			return node;
		}

		/// <summary>
		/// Gets or sets the value associated with the specific key.
		/// </summary>
		/// 
		/// <remarks>
		/// If used as a setter and an item already exists with a key equal to
		/// <paramref name="key"/>, the old value is overwritten.
		/// <para/>
		/// Getting or setting values through this property approaches
		/// <c>O(1)</c>.
		/// </remarks>
		/// 
		/// <param name="key">The key of the value to get or set.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="KeyNotFoundException">
		/// This is used as an accessor, and the dictionary does not contain
		/// any item with a key equal to <paramref name="key"/>.
		/// </exception>
		public TValue this[TKey key] {
			get => dictionary[key].Value.Value;
			set {
				if (dictionary.TryGetValue(key, out var keyItem))
					Update(keyItem, value);
				else
					Add(key, value);
			}
		}
		/// <summary>
		/// Gets or sets the value associated with the specific key.
		/// </summary>
		/// 
		/// <remarks>
		/// If used as a setter and an item already exists with a key equal to
		/// <paramref name="key"/>, the old value is overwritten.
		/// <para/>
		/// Getting or setting values through this property takes <c>O(1)</c>.
		/// </remarks>
		/// 
		/// <param name="key">The key of the value to get or set.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="InvalidCastException">
		/// <paramref name="key"/> can not be expressed as a <c>TKey</c>, or
		/// the new value can not be expressed as a <c>TValue</c>.
		/// </exception>
		/// <exception cref="KeyNotFoundException">
		/// This is used as an accessor, and the dictionary does not contain
		/// any item with a key equal to <paramref name="key"/>.
		/// </exception>
		object IDictionary.this[object key] {
			get => this[(TKey)key];
			set => this[(TKey)key] = (TValue)value;
		}

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
		/// <see cref="Count"/>, or is less than 0.
		/// </exception>
		public KeyValuePair<TKey, TValue> this[int index] {
			get => NodeAt(index).Value;
			set {
				if (index == Count) {
					Add(value);
				} else {
					var node = NodeAt(index);
					if (node.Value.Key.Equals(value.Key)) {
						Update(node, value.Value);
					} else {
						dictionary.Add(value.Key, node);
						dictionary.Remove(node.Value.Key);
						node.Value = value;
					}
				}
			}
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
		/// <see cref="Count"/>, or is less than 0.
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
		/// <see cref="Count"/>, or is less than 0.
		/// </exception>
		object IList.this[int index] {
			get => (this as IList<KeyValuePair<TKey, TValue>>)[index];
			set => (this as IList<KeyValuePair<TKey, TValue>>)[index] = (KeyValuePair<TKey, TValue>)value;
		}

		/// <summary>
		/// Get a list of the keys in the dictionary, in the proper order.
		/// </summary>
		/// 
		/// <remarks>
		/// This is a static collection; any changes made to the dictionary
		/// will not be reflected in the returned object.
		/// <para/>
		/// This operation runs in <c>O(n)</c> time; in almost all cases, it
		/// will be better to use <see cref="GetEnumerator"/> to iterate
		/// through the items as <see cref="KeyValuePair{TKey, TValue}"/>.
		/// </remarks>
		public ICollection<TKey> Keys =>
			(from node in list
			 select node.Key
			).ToList();
		/// <summary>
		/// Get a list of the keys in the dictionary, in the proper order.
		/// </summary>
		/// 
		/// <remarks>
		/// This is a static collection; any changes made to the dictionary
		/// will not be reflected in the returned object.
		/// <para/>
		/// This operation runs in <c>O(n)</c> time; in almost all cases, it
		/// will be better to use <see cref="GetEnumerator"/> to iterate
		/// through the items as <see cref="KeyValuePair{TKey, TValue}"/>.
		/// </remarks>
		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys =>
			Keys;
		/// <summary>
		/// Get a list of the keys in the dictionary, in the proper order.
		/// </summary>
		/// 
		/// <remarks>
		/// This is a static collection; any changes made to the dictionary
		/// will not be reflected in the returned object.
		/// <para/>
		/// This operation runs in <c>O(n)</c> time; in almost all cases, it
		/// will be better to use <see cref="GetEnumerator"/> to iterate
		/// through the items as <see cref="KeyValuePair{TKey, TValue}"/>.
		/// </remarks>
		ICollection IDictionary.Keys =>
			Keys as ICollection;

		/// <summary>
		/// Get a list of the values in the dictionary, in the proper order.
		/// </summary>
		/// 
		/// <remarks>
		/// This is a static collection; any changes made to the dictionary
		/// will not be reflected in the returned object.
		/// <para/>
		/// This operation runs in <c>O(n)</c> time; in almost all cases, it
		/// will be better to use <see cref="GetEnumerator"/> to iterate
		/// through the items as <see cref="KeyValuePair{TKey, TValue}"/>.
		/// </remarks>
		public ICollection<TValue> Values =>
			(from node in list
			 select node.Value
			).ToList();
		/// <summary>
		/// Get a list of the keys in the dictionary, in the proper order.
		/// </summary>
		/// 
		/// <remarks>
		/// This is a static collection; any changes made to the dictionary
		/// will not be reflected in the returned object.
		/// <para/>
		/// This operation runs in <c>O(n)</c> time; in almost all cases, it
		/// will be better to use <see cref="GetEnumerator"/> to iterate
		/// through the items as <see cref="KeyValuePair{TKey, TValue}"/>.
		/// </remarks>
		IEnumerable< TValue > IReadOnlyDictionary<TKey, TValue>.Values =>
			Values;
		/// <summary>
		/// Get a list of the keys in the dictionary, in the proper order.
		/// </summary>
		/// 
		/// <remarks>
		/// This is a static collection; any changes made to the dictionary
		/// will not be reflected in the returned object.
		/// <para/>
		/// This operation runs in <c>O(n)</c> time; in almost all cases, it
		/// will be better to use <see cref="GetEnumerator"/> to iterate
		/// through the items as <see cref="KeyValuePair{TKey, TValue}"/>.
		/// </remarks>
		ICollection IDictionary.Values =>
			Values as ICollection;

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
		public bool Contains(KeyValuePair<TKey, TValue> item) =>
			TryGetValue(item.Key, out var value) && value.Equals(item.Value);
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
			Contains((KeyValuePair<TKey, TValue>)item);

		/// <summary>
		/// Indicates whether an item with the specified key exists in the
		/// dictionary.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation approaches <c>O(1)</c> time.
		/// </remarks>
		/// 
		/// <param name="key">The key to search for.</param>
		/// 
		/// <returns>
		/// <c>true</c> if <paramref name="key"/> exists in the dictionary;
		/// otherwise <c>false</c>.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		public bool ContainsKey(TKey key) =>
			dictionary.ContainsKey(key);
		/// <summary>
		/// Indicates whether an item with the specified key exists in the
		/// dictionary.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation approaches <c>O(1)</c> time.
		/// </remarks>
		/// 
		/// <param name="key">The key to search for.</param>
		/// 
		/// <returns>
		/// <c>true</c> if <paramref name="key"/> exists in the dictionary;
		/// otherwise <c>false</c>.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="InvalidCastException">
		/// <paramref name="key"/> can not be expressed as a <c>TKey</c>.
		/// </exception>
		bool IDictionary.Contains(object key) =>
			ContainsKey((TKey)key);

		/// <summary>
		/// Indicates whether any item in the dictionary has the specified
		/// value.
		/// </summary>
		/// 
		/// <remarks>This operation runs in <c>O(n)</c> time.</remarks>
		/// 
		/// <param name="value">The value to search for.</param>
		/// 
		/// <returns>
		/// <c>true</c> if an item with a value equal to
		/// <paramref name="value"/> exists in the dictionary; otherwise
		/// <c>false</c>.
		/// </returns>
		public bool ContainsValue(TValue value) {
			foreach (var node in list)
				if (node.Value.Equals(value))
					return true;
			return false;
		}

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
		public Enumerator GetEnumerator() =>
			new Enumerator(this);
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
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() =>
			GetEnumerator();
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
		IEnumerator IEnumerable.GetEnumerator() =>
			GetEnumerator();
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
		IDictionaryEnumerator IDictionary.GetEnumerator() =>
			GetEnumerator();
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
			GetEnumerator();
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
		public int IndexOf(TKey key) {
			int index = -1;

			if (dictionary.TryGetValue(key, out var node))
				for (; node != null; ++index)
					node = node.Previous;

			return index;
		}
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
		public int IndexOf(KeyValuePair<TKey, TValue> item) {
			if (dictionary.TryGetValue(item.Key, out var node) && node.Value.Value.Equals(item.Value))
				return IndexOf(item.Key);
			else
				return -1;
		}
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
			IndexOf((KeyValuePair<TKey, TValue>)item);

		/// <summary>
		/// Gets the value associated with a specific key, if it exists in the
		/// dictionary.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation approaches <c>O(1)</c> time.
		/// </remarks>
		/// 
		/// <param name="key">The key of the value to access.</param>
		/// <param name="value">
		/// When this method returns, this will be set to the value associated
		/// with <paramref name="key"/> in the dictionary if it exists;
		/// otherwise, the default value of <c>TValue</c>.
		/// </param>
		/// 
		/// <returns>
		/// <c>true</c> if the dictionary contains such a value; otherwise
		/// <c>false</c>.
		/// </returns>
		public bool TryGetValue(TKey key, out TValue value) {
			var result = dictionary.TryGetValue(key, out var node);
			value = node.Value.Value;
			return result;
		}
#endregion

#region Properties
		/// <summary>
		/// Gets the number of items contained in this dictionary.
		/// </summary>
		/// 
		/// <remarks>
		/// Accessing this value runs in <c>O(1)</c> time.
		/// </remarks>
		public int Count =>
			dictionary.Count;

		/// <summary>
		/// Indicates whether items may be added to or removed from this
		/// dictionary after creation.
		/// </summary>
		/// 
		/// <remarks>
		/// Accessing this value runs in <c>O(1)</c> time.
		/// </remarks>
		bool IsFixedSize =>
			(dictionary as IDictionary).IsFixedSize || (list as IList).IsFixedSize;
		/// <summary>
		/// Indicates whether items may be added to or removed from this
		/// dictionary after creation.
		/// </summary>
		/// 
		/// <remarks>
		/// Accessing this value runs in <c>O(1)</c> time.
		/// </remarks>
		bool IDictionary.IsFixedSize =>
			IsFixedSize;
		/// <summary>
		/// Indicates whether items may be added to or removed from this
		/// dictionary after creation.
		/// </summary>
		/// 
		/// <remarks>
		/// Accessing this value runs in <c>O(1)</c> time.
		/// </remarks>
		bool IList.IsFixedSize =>
			IsFixedSize;

		/// <summary>
		/// Indicates whether this dictionary can be modified after creation.
		/// </summary>
		/// 
		/// <remarks>
		/// Accessing this value runs in <c>O(1)</c> time.
		/// </remarks>
		bool IsReadOnly =>
			(dictionary as IDictionary).IsReadOnly || (list as IList).IsReadOnly;
		/// <summary>
		/// Indicates whether this dictionary can be modified after creation.
		/// </summary>
		/// 
		/// <remarks>
		/// Accessing this value runs in <c>O(1)</c> time.
		/// </remarks>
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly =>
			IsReadOnly;
		/// <summary>
		/// Indicates whether this dictionary can be modified after creation.
		/// </summary>
		/// 
		/// <remarks>
		/// Accessing this value runs in <c>O(1)</c> time.
		/// </remarks>
		bool IDictionary.IsReadOnly =>
			IsReadOnly;
		/// <summary>
		/// Indicates whether this dictionary can be modified after creation.
		/// </summary>
		/// 
		/// <remarks>
		/// Accessing this value runs in <c>O(1)</c> time.
		/// </remarks>
		bool IList.IsReadOnly =>
			IsReadOnly;

		/// <summary>
		/// Indicates whether access to the dictionary is synchronized.
		/// </summary>
		/// 
		/// <remarks>
		/// Accessing this value runs in <c>O(1)</c> time.
		/// </remarks>
		bool ICollection.IsSynchronized =>
			(dictionary as ICollection).IsSynchronized && (list as ICollection).IsSynchronized;

		/// <summary>
		/// Gets an object that can be used to synchronize access to the
		/// dictionary.
		/// </summary>
		/// 
		/// <remarks>
		/// Accessing this value runs in <c>O(1)</c> time.
		/// </remarks>
		object ICollection.SyncRoot =>
			(dictionary as ICollection).SyncRoot;
#endregion

#region Modifiers
		/// <summary>
		/// Add a new item to the dictionary, after all existing items.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation typically approaches <c>O(1)</c> time, except if
		/// the available capacity has been filled; in that case, it runs in
		/// <c>O(n)</c> time where <c>n</c> is <see cref="Count"/>.
		/// </remarks>
		/// 
		/// <param name="item">The item to add.</param>
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
		public void Add(KeyValuePair<TKey, TValue> item) {
			var node = new LinkedListNode<KeyValuePair<TKey, TValue>>(item);
			dictionary.Add(item.Key, node);
			list.AddLast(node);
		}
		/// <summary>
		/// Add a new item to the dictionary, after all existing items.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation typically approaches <c>O(1)</c> time, except if
		/// the available capacity has been filled; in that case, it runs in
		/// <c>O(n)</c> time where <c>n</c> is <see cref="Count"/>.
		/// </remarks>
		/// 
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
		public void Add(TKey key, TValue value) =>
			Add(new KeyValuePair<TKey, TValue>(key, value));
		/// <summary>
		/// Add a new item to the dictionary, after all existing items.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation typically approaches <c>O(1)</c> time, except if
		/// the available capacity has been filled; in that case, it runs in
		/// <c>O(n)</c> time where <c>n</c> is <see cref="Count"/>.
		/// </remarks>
		/// 
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
		/// <exception cref="InvalidCastException">
		/// <paramref name="key"/> can not be expressed as a <c>TKey</c>, or
		/// <paramref name="value"/> can not be expressed as a <c>TValue</c>.
		/// </exception>
		void IDictionary.Add(object key, object value) =>
			Add((TKey)key, (TValue)value);
		/// <summary>
		/// Add a new item to the dictionary, after all existing items.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation typically approaches <c>O(1)</c> time, except if
		/// the available capacity has been filled; in that case, it runs in
		/// <c>O(n)</c> time where <c>n</c> is <see cref="Count"/>.
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
		int IList.Add(object item) {
			Add((KeyValuePair<TKey, TValue>)item);
			return (Count - 1);
		}

		/// <summary>
		/// Remove all items from the dictionary.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation runs in <c>O(n)</c> time, where <c>n</c> is the
		/// <em>capacity</em> of the dictionary (not <see cref="Count"/>).
		/// </remarks>
		public void Clear() {
			dictionary.Clear();
			list.Clear();
		}

		/// <summary>
		/// Copies all elements in the dictionary to an array, in order,
		/// starting at the specified index in that array.
		/// </summary>
		/// 
		/// <remarks>
		/// This ultimately uses
		/// <see cref="Array.Copy(Array, int, Array, int, int)"/>, and so that
		/// documentation will describe the behaviour in more detail.
		/// <para/>
		/// This operation runs in <c>O(n)</c> time.
		/// </remarks>
		/// 
		/// <param name="array">
		/// The array to receive the dictionary's items.
		/// </param>
		/// <param name="arrayIndex">
		/// The index in <paramref name="array"/> where the first item will be
		/// located.
		/// </param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is less than 0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="arrayIndex"/> plus <see cref="Count"/> is greater
		/// than the length of <paramref name="array"/>.
		/// </exception>
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
			list.CopyTo(array, arrayIndex);
		/// <summary>
		/// Copies all elements in the dictionary to an array, in order,
		/// starting at the specified index in that array.
		/// </summary>
		/// 
		/// <remarks>
		/// This ultimately uses
		/// <see cref="Array.Copy(Array, int, Array, int, int)"/>, and so that
		/// documentation will describe the behaviour in more detail.
		/// <para/>
		/// This operation runs in <c>O(n)</c> time.
		/// </remarks>
		/// 
		/// <param name="array">
		/// The array to receive the dictionary's items.
		/// </param>
		/// <param name="index">
		/// The index in <paramref name="array"/> where the first item will be
		/// located.
		/// </param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="index"/> plus <see cref="Count"/> is greater than
		/// the length of <paramref name="array"/>.
		/// </exception>
		/// <exception cref="InvalidCastException">
		/// <paramref name="array"/> can not be expressed as an array of
		/// <see cref="KeyValuePair{TKey, TValue}"/>, with the proper type
		/// parameters.
		/// </exception>
		void ICollection.CopyTo(Array array, int index) =>
			CopyTo((KeyValuePair<TKey, TValue>[])array, index);

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
		/// <paramref name="index"/> is greater than <see cref="Count"/>, or
		/// is less than 0.
		/// </exception>
		public void Insert(int index, KeyValuePair<TKey, TValue> item) {
			var node = new LinkedListNode<KeyValuePair<TKey, TValue>>(item);
			dictionary.Add(item.Key, node);

			if (index == 0)
				list.AddFirst(node);
			else if (index == Count)
				list.AddLast(node);
			else
				list.AddBefore(NodeAt(index), node);
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
		/// <paramref name="index"/> is greater than <see cref="Count"/>, or
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
		/// <paramref name="index"/> is greater than <see cref="Count"/>, or
		/// is less than 0.
		/// </exception>
		/// <exception cref="InvalidCastException">
		/// <paramref name="key"/> can not be expressed as a <c>TKey</c>, or
		/// <paramref name="value"/> can not be expressed as a <c>TValue</c>.
		/// </exception>
		public void Insert(int index, object key, object value) =>
			Insert(index, (TKey)key, (TValue)value);
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
		/// <paramref name="index"/> is greater than <see cref="Count"/>, or
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
		/// Remove the item associated with the specified node from the
		/// dictionary.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation approaches <c>O(1)</c> time.
		/// </remarks>
		/// 
		/// <param name="node">The key of the item to remove.</param>
		/// 
		/// <returns>
		/// <c>true</c> if the item was removed successfully; otherwise
		/// <c>false</c> (including if the item did not exist).
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="node"/> is <c>null</c>.
		/// </exception>
		bool Remove(LinkedListNode<KeyValuePair<TKey, TValue>> node) {
			bool result = dictionary.Remove(node.Value.Key);
			list.Remove(node);

			return result;
		}
		/// <summary>
		/// Remove the item associated with the specified key from the
		/// dictionary.
		/// </summary>
		/// 
		/// <remarks>
		/// No exception is thrown if <paramref name="key"/> does not exist.
		/// <para/>
		/// This operation approaches <c>O(1)</c> time.
		/// </remarks>
		/// 
		/// <param name="key">The key of the item to remove.</param>
		/// 
		/// <returns>
		/// <c>true</c> if the item was removed successfully; otherwise
		/// <c>false</c> (including if the item did not exist).
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		public bool Remove(TKey key) {
			try {
				return Remove(dictionary[key]);
			} catch (KeyNotFoundException) {
				return false;
			}
		}
		/// <summary>
		/// Remove the item at the specified index from the dictionary.
		/// </summary>
		/// 
		/// <remarks>
		/// No exception is thrown if <paramref name="key"/> does not exist.
		/// <para/>
		/// This operation approaches <c>O(1)</c> time.
		/// </remarks>
		/// 
		/// <param name="key">The key of the item to remove.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		void IDictionary.Remove(object key) =>
			Remove((TKey)key);
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
		/// <see cref="Count"/>, or is less than 0.
		/// </exception>
		public void RemoveAt(int index) =>
			Remove(NodeAt(index));

		/// <summary>
		/// Remove the specified key/value pair from the dictionary, if it
		/// exists.
		/// </summary>
		/// 
		/// <remarks>
		/// This operation approaches <c>O(1)</c> time.
		/// </remarks>
		/// 
		/// <param name="item">The key/value pair to remove.</param>
		/// 
		/// <returns>
		/// <c>true</c> if the item was removed successfully; otherwise
		/// <c>false</c> (including if the item did not exist).
		/// </returns>
		public bool Remove(KeyValuePair<TKey, TValue> item) {
			LinkedListNode<KeyValuePair<TKey, TValue>> node;
			try {
				node = dictionary[item.Key];
			// We don't care about either of the exceptions as this operates
			// directly on the underlying collection
			} catch (ArgumentNullException) {
				return false;
			} catch (KeyNotFoundException) {
				return false;
			}

			if (node.Value.Value.Equals(item.Value))
				return Remove(node);
			else
				return false;
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
		void IList.Remove(object value) =>
			Remove((KeyValuePair<TKey, TValue>)value);

		/// <summary>
		/// Replace the value of the key/value pair pointed to by the given
		/// node.
		/// </summary>
		/// 
		/// <param name="node">The node pointing to the target item.</param>
		/// <param name="value">The new value of the item.</param>
		void Update(LinkedListNode<KeyValuePair<TKey, TValue>> node, TValue value) {
			node.Value = new KeyValuePair<TKey, TValue>(node.Value.Key, value);
		}
#endregion
	}
}
