/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

#if (!NETSTANDARD1_0 && !NETSTANDARD1_1)
#define SUPPORT_PROPERTYCHANGING_EVENT
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace AgEitilt.Common.Dictionary {
	/// <summary>
	/// Represents a generic read-only collection of key/value pairs while
	/// notifying listeners when those contents change.
	/// </summary>
	/// 
	/// <remarks>
	/// Each element is a key/value pair stored in a
	/// <see cref="KeyValuePair{TKey, TValue}"/> object. When iterated using a
	/// <c>foreach</c> loop (<c>For Each</c> in Visual Basic, <c>for each</c>
	/// in C++), the enumerated objects are therefore of that type rather than
	/// either <typeparamref name="TKey"/> or <typeparamref name="TValue"/>.
	/// <para/>
	/// Each pair must have a unique key. Implementations can vary in whether
	/// they allow that key to be <c>null</c>. The value can be <c>null</c>
	/// and does not have to be unique. The generic
	/// <see cref="ObservableDictionaryBase{TKey, TValue}"/> allows the
	/// contained keys and values to be enumerated, but it does not imply any
	/// particular sort order.
	/// </remarks>
	/// 
	/// <typeparam name="TKey">
	/// The type of keys in the dictionary.
	/// </typeparam>
	/// <typeparam name="TValue">
	/// The type of values in the dictionary.
	/// </typeparam>
	public interface IObservableReadOnlyDictionary<TKey, TValue>
		: IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable,
#if SUPPORT_PROPERTYCHANGING_EVENT
		  INotifyPropertyChanging,
#endif
		  INotifyCollectionChanged, INotifyPropertyChanged { }

	/// <summary>
	/// Represents a collection of keys and values while notifying listeners
	/// when those contents change, regardless of the underlying
	/// <see cref="IDictionary{TKey, TValue}"/>.
	/// </summary>
	/// 
	/// <remarks>
	/// Each element is a key/value pair stored in a
	/// <see cref="KeyValuePair{TKey, TValue}"/> object. When iterated using a
	/// <c>foreach</c> loop (<c>For Each</c> in Visual Basic, <c>for each</c>
	/// in C++), the enumerated objects are therefore of that type rather than
	/// either <typeparamref name="TKey"/> or <typeparamref name="TValue"/>.
	/// <para/>
	/// Each pair must have a unique key. Implementations can vary in whether
	/// they allow that key to be <c>null</c>. The value can be <c>null</c>
	/// and does not have to be unique. The generic
	/// <see cref="ObservableDictionaryBase{TKey, TValue}"/> allows the
	/// contained keys and values to be enumerated, but it does not imply any
	/// particular sort order.
	/// </remarks>
	/// 
	/// <typeparam name="TKey">
	/// The type of keys in the dictionary.
	/// </typeparam>
	/// <typeparam name="TValue">
	/// The type of values in the dictionary.
	/// </typeparam>
	public abstract partial class ObservableDictionaryBase<TKey, TValue>
		: IObservableReadOnlyDictionary<TKey, TValue>,
		  IDictionary, IDictionary<TKey, TValue> {
		/// <summary>
		/// Retrieve a reference to the underlying
		/// <see cref="IDictionary{TKey, TValue}"/> used by the particular
		/// instance as determined by the implementation.
		/// </summary>
		/// 
		/// <value>
		/// An instance providing methods common to all dictionaries.
		/// </value>
		abstract protected IDictionary<TKey, TValue> Dictionary { get; }

#region Content change event handling
		/// <summary>
		/// Occurs when the collection changes.
		/// </summary>
		/// 
		/// <remarks>
		/// If the sent <see cref="NotifyCollectionChangedEventArgs"/> is of
		/// type <see cref="NotifyCollectionChangedAction.Add"/>,
		/// <see cref="NotifyCollectionChangedAction.Remove"/>, or
		/// <see cref="NotifyCollectionChangedAction.Replace"/>,
		/// <see cref="NotifyCollectionChangedEventArgs.NewItems"/> and (if
		/// applicable) <see cref="NotifyCollectionChangedEventArgs.OldItems"/>
		/// are guaranteed to be an <see cref="IList"/> of
		/// <see cref="KeyValuePair{TKey, TValue}"/> objects parameterized to
		/// <typeparamref name="TKey"/> and <typeparamref name="TValue"/>.
		/// </remarks>
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		/// 
		/// <remarks>
		/// The <c>PropertyChanged</c> event can indicate all properties on
		/// the object have changed by using either <c>null</c> or
		/// <see cref="String.Empty"/> as the property name in the
		/// <see cref="PropertyChangedEventArgs"/>.
		/// </remarks>
		public event PropertyChangedEventHandler PropertyChanged;
#if SUPPORT_PROPERTYCHANGING_EVENT
		/// <summary>
		/// Occurs just before a property value changes.
		/// </summary>
		/// 
		/// <remarks>
		/// The <c>PropertyChanging</c> event can indicate all properties on
		/// the object have changed by using either <c>null</c> or
		/// <see cref="String.Empty"/> as the property name in the
		/// <see cref="PropertyChangingEventArgs"/>.
		/// </remarks>
		public event PropertyChangingEventHandler PropertyChanging;
#endif

		/// <summary>
		/// Convert an object into a function with the proper return type for,
		/// for example, <see cref="SendAddEvents(object, object, Func{bool}, NotifyCollectionChangedEventArgs, bool, bool, ICollection{string})"/>.
		/// </summary>
		/// 
		/// <param name="action">The action to wrap.</param>
		/// 
		/// <returns>The enclosing function.</returns>
		/// 
		/// <exception cref="ArgumentException">
		/// <paramref name="action"/> cannot be expressed as a valid type.
		/// </exception>
		static Func<Tuple<bool, TValue>> ParseAction(object action) {
			var actionNoResult = action as Action;
			var actionTestOnly = action as Func<bool>;
			var actionValueOnly = action as Func<TValue>;
			var actionPair = action as Func<Tuple<bool, TValue>>;
			if (actionPair == null) {
				if (actionNoResult != null) {
					return (() => {
						actionNoResult.Invoke();
						return Tuple.Create(true, default(TValue));
					});
				} else if (actionTestOnly != null) {
					return (() => {
						return Tuple.Create(actionTestOnly.Invoke(), default(TValue));
					});
				} else if (actionValueOnly != null) {
					return (() => { return Tuple.Create(true, actionValueOnly.Invoke()); });
				} else {
					throw new ArgumentException("Can't properly cast desired action", nameof(action));
				}
			} else {
				return actionPair;
			}
		}

		/// <summary>
		/// The properties affected by an addition action.
		/// </summary>
		protected virtual ISet<string> AddProperties => new HashSet<string>(new string[3]{
			nameof(Keys),
			nameof(Values),
			nameof(Count)
		});
		/// <summary>
		/// Notify all relevant listeners that some item is added to the
		/// <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// <paramref name="item"/> is ignored unless
		/// <paramref name="collectionArgs"/> is <c>null</c>.
		/// <para/>
		/// <paramref name="action"/> must be able to be expressed as one of:
		/// <see cref="Action"/>, <c>Func&lt;bool&gt;</c>,
		/// <c>Func&lt;TValue&gt;</c>, or
		/// <c>Func&lt;Tuple&lt;bool, TValue&gt;&gt;</c>.
		/// </remarks>
		/// 
		/// <param name="action">The action that causes the addition.</param>
		/// <param name="item">
		/// The new item(s), or <c>null</c> if it is already contained in
		/// <paramref name="collectionArgs"/>.
		/// </param>
		/// <param name="preTest">
		/// A simplified approximation of the test performed by
		/// <paramref name="action"/>, but that doesn't result in any changes,
		/// or <c>null</c> if it will always run.
		/// </param>
		/// <param name="collectionArgs">
		/// A description of how the collection is changed, or <c>null</c> to
		/// generate a new value using <paramref name="item"/>.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="AddProperties"/>.
		/// </param>
		/// 
		/// <returns>The value returned by <paramref name="action"/>.</returns>
		protected Tuple<bool, TValue> SendAddEvents(object action, object item = null,
				Func<bool> preTest = null, NotifyCollectionChangedEventArgs collectionArgs = null,
				bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) {
			var actionFunc = ParseAction(action);
			if (preTest == null)
				preTest = (() => true);

			if (collectionArgs == null) {
				if (item is IList itemList) {
					collectionArgs = new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Add,
						itemList
					);
				} else {
					collectionArgs = new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Add,
						item
					);
				}
			}

			if (preTest.Invoke()) {
#if SUPPORT_PROPERTYCHANGING_EVENT
				if (deferChanging == false)
					foreach (var prop in (properties ?? AddProperties))
						PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(prop));
#endif
				OnAdding(collectionArgs, deferChanging, deferChanged, properties);
			}

			var result = actionFunc.Invoke();
			if (result.Item1) {
				CollectionChanged?.Invoke(this, collectionArgs);
				if (deferChanged == false)
					foreach (var prop in (properties ?? AddProperties))
						PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

				OnAdd(collectionArgs, deferChanging, deferChanged, properties);
			}

			return result;
		}
		/// <summary>
		/// Notify all relevant listeners that some item is added to the
		/// <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// <paramref name="action"/> must be able to be expressed as one of:
		/// <see cref="Action"/>, <c>Func&lt;bool&gt;</c>,
		/// <c>Func&lt;TValue&gt;</c>, or
		/// <c>Func&lt;Tuple&lt;bool, TValue&gt;&gt;</c>.
		/// </remarks>
		/// 
		/// <param name="action">The action that causes the addition.</param>
		/// <param name="key">
		/// The key to be associated with the item.
		/// </param>
		/// <param name="value">The value of the item.</param>
		/// <param name="preTest">
		/// A simplified approximation of the test performed by
		/// <paramref name="action"/>, but that doesn't result in any changes,
		/// or <c>null</c> if it will always run.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="AddProperties"/>.
		/// </param>
		/// 
		/// <returns>The value returned by <paramref name="action"/>.</returns>
		protected Tuple<bool, TValue> SendAddEvents(object action, TKey key, TValue value, Func<bool> preTest = null,
				bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) =>
			SendAddEvents(action, new KeyValuePair<TKey, TValue>(key, value), preTest);
		/// <summary>
		/// Perform any implementation-specific event handling when a new item
		/// is just about to be added to <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <param name="collectionArgs">
		/// A description of how the collection will be changed.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="AddProperties"/>.
		/// </param>
		protected virtual void OnAdding(NotifyCollectionChangedEventArgs collectionArgs,
			bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) { }
		/// <summary>
		/// Perform any implementation-specific event handling when a new item
		/// has been added to <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <param name="collectionArgs">
		/// A description of how the collection was changed.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="AddProperties"/>.
		/// </param>
		protected virtual void OnAdd(NotifyCollectionChangedEventArgs collectionArgs,
			bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) { }

		/// <summary>
		/// The properties affected by a move action.
		/// </summary>
		protected virtual ISet<string> MoveProperties => new HashSet<string>(new string[0]{
		});
		/// <summary>
		/// Notify all relevant listeners that some item is moved within the
		/// <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// <paramref name="item"/> is ignored unless
		/// <paramref name="collectionArgs"/> is <c>null</c>.
		/// <para/>
		/// <paramref name="action"/> must be able to be expressed as one of:
		/// <see cref="Action"/>, <c>Func&lt;bool&gt;</c>,
		/// <c>Func&lt;TValue&gt;</c>, or
		/// <c>Func&lt;Tuple&lt;bool, TValue&gt;&gt;</c>.
		/// </remarks>
		/// 
		/// <param name="action">The action that causes the removal.</param>
		/// <param name="newIndex">
		/// The index to which the item is moved.
		/// </param>
		/// <param name="oldIndex">
		/// The index the item was previously located at.
		/// </param>
		/// <param name="item">
		/// The item(s) that will be removed, or <c>null</c> if it is already
		/// contained in <paramref name="collectionArgs"/>.
		/// </param>
		/// <param name="preTest">
		/// A simplified approximation of the test performed by
		/// <paramref name="action"/>, but that doesn't result in any changes,
		/// or <c>null</c> if it will always run.
		/// </param>
		/// <param name="collectionArgs">
		/// A description of how the collection is changed, or <c>null</c> to
		/// generate a new value using <paramref name="item"/>.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="MoveProperties"/>.
		/// </param>
		/// 
		/// <returns>The value returned by <paramref name="action"/>.</returns>
		protected Tuple<bool, TValue> SendMoveEvents(object action, int newIndex, int oldIndex, object item = null,
				Func<bool> preTest = null, NotifyCollectionChangedEventArgs collectionArgs = null,
				bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) {
			var actionFunc = ParseAction(action);
			if (preTest == null)
				preTest = (() => true);

			if (collectionArgs == null) {
				if (item is IList itemList) {
					collectionArgs = new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Move,
						itemList,
						newIndex,
						oldIndex
					);
				} else {
					collectionArgs = new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Move,
						item,
						newIndex,
						oldIndex
					);
				}
			}

			if (preTest.Invoke()) {
#if SUPPORT_PROPERTYCHANGING_EVENT
				if (deferChanging == false)
					foreach (var prop in (properties ?? MoveProperties))
						PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(prop));
#endif
				OnMoving(collectionArgs, deferChanging, deferChanged, properties);
			}

			var result = actionFunc.Invoke();
			if (result.Item1) {
				CollectionChanged?.Invoke(this, collectionArgs);
				if (deferChanged == false)
					foreach (var prop in (properties ?? MoveProperties))
						PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

				OnMove(collectionArgs, deferChanging, deferChanged, properties);
			}

			return result;
		}
		/// <summary>
		/// Notify all relevant listeners that some item is removed from the
		/// <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <param name="action">The action that causes the removal.</param>
		/// <param name="newIndex">
		/// The index to which the item is moved.
		/// </param>
		/// <param name="oldIndex">
		/// The index the item was previously located at.
		/// </param>
		/// <param name="key">
		/// The key previously associated with the item.
		/// </param>
		/// <param name="value">The value of the item.</param>
		/// <param name="preTest">
		/// A simplified approximation of the test performed by
		/// <paramref name="action"/>, but that doesn't result in any changes,
		/// or <c>null</c> if it will always run.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="MoveProperties"/>.
		/// </param>
		/// 
		/// <returns>The value returned by <paramref name="action"/>.</returns>
		protected Tuple<bool, TValue> SendMoveEvents(object action, int newIndex, int oldIndex, TKey key, TValue value, Func<bool> preTest = null,
				bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) =>
			SendMoveEvents(action, newIndex, oldIndex, new KeyValuePair<TKey, TValue>(key, value), preTest);
		/// <summary>
		/// Perform any implementation-specific event handling when an item
		/// is just about to be removed from <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <param name="collectionArgs">
		/// A description of how the collection will be changed.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="MoveProperties"/>.
		/// </param>
		protected virtual void OnMoving(NotifyCollectionChangedEventArgs collectionArgs,
			bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) { }
		/// <summary>
		/// Perform any implementation-specific event handling when an item
		/// has been removed from <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <param name="collectionArgs">
		/// A description of how the collection was changed.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="MoveProperties"/>.
		/// </param>
		protected virtual void OnMove(NotifyCollectionChangedEventArgs collectionArgs,
			bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) { }

		/// <summary>
		/// The properties affected by a removal action.
		/// </summary>
		protected virtual ISet<string> RemoveProperties => new HashSet<string>(new string[3]{
			nameof(Keys),
			nameof(Values),
			nameof(Count)
		});
		/// <summary>
		/// Notify all relevant listeners that some item is removed from the
		/// <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// <paramref name="item"/> is ignored unless
		/// <paramref name="collectionArgs"/> is <c>null</c>.
		/// <para/>
		/// <paramref name="action"/> must be able to be expressed as one of:
		/// <see cref="Action"/>, <c>Func&lt;bool&gt;</c>,
		/// <c>Func&lt;TValue&gt;</c>, or
		/// <c>Func&lt;Tuple&lt;bool, TValue&gt;&gt;</c>.
		/// </remarks>
		/// 
		/// <param name="action">The action that causes the removal.</param>
		/// <param name="item">
		/// The item(s) that will be removed, or <c>null</c> if it is already
		/// contained in <paramref name="collectionArgs"/>.
		/// </param>
		/// <param name="preTest">
		/// A simplified approximation of the test performed by
		/// <paramref name="action"/>, but that doesn't result in any changes,
		/// or <c>null</c> if it will always run.
		/// </param>
		/// <param name="collectionArgs">
		/// A description of how the collection is changed, or <c>null</c> to
		/// generate a new value using <paramref name="item"/>.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="RemoveProperties"/>.
		/// </param>
		/// 
		/// <returns>The value returned by <paramref name="action"/>.</returns>
		protected Tuple<bool, TValue> SendRemoveEvents(object action, object item = null,
				Func<bool> preTest = null, NotifyCollectionChangedEventArgs collectionArgs = null,
				bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) {
			var actionFunc = ParseAction(action);
			if (preTest == null)
				preTest = (() => true);

			if (collectionArgs == null) {
				if (item is IList itemList) {
					collectionArgs = new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Remove,
						itemList
					);
				} else {
					collectionArgs = new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Remove,
						item
					);
				}
			}

			if (preTest.Invoke()) {
#if SUPPORT_PROPERTYCHANGING_EVENT
				if (deferChanging == false)
					foreach (var prop in (properties ?? RemoveProperties))
						PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(prop));
#endif
				OnRemoving(collectionArgs, deferChanging, deferChanged, properties);
			}

			var result = actionFunc.Invoke();
			if (result.Item1) {
				CollectionChanged?.Invoke(this, collectionArgs);
				if (deferChanged == false)
					foreach (var prop in (properties ?? RemoveProperties))
						PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

				OnRemove(collectionArgs, deferChanging, deferChanged, properties);
			}

			return result;
		}
		/// <summary>
		/// Notify all relevant listeners that some item is removed from the
		/// <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <param name="action">The action that causes the removal.</param>
		/// <param name="key">
		/// The key previously associated with the item.
		/// </param>
		/// <param name="value">The value of the item.</param>
		/// <param name="preTest">
		/// A simplified approximation of the test performed by
		/// <paramref name="action"/>, but that doesn't result in any changes,
		/// or <c>null</c> if it will always run.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="RemoveProperties"/>.
		/// </param>
		/// 
		/// <returns>The value returned by <paramref name="action"/>.</returns>
		protected Tuple<bool, TValue> SendRemoveEvents(object action, TKey key, TValue value, Func<bool> preTest = null,
				bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) =>
			SendRemoveEvents(action, new KeyValuePair<TKey, TValue>(key, value), preTest);
		/// <summary>
		/// Perform any implementation-specific event handling when an item
		/// is just about to be removed from <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <param name="collectionArgs">
		/// A description of how the collection will be changed.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="RemoveProperties"/>.
		/// </param>
		protected virtual void OnRemoving(NotifyCollectionChangedEventArgs collectionArgs,
			bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) { }
		/// <summary>
		/// Perform any implementation-specific event handling when an item
		/// has been removed from <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <param name="collectionArgs">
		/// A description of how the collection was changed.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="RemoveProperties"/>.
		/// </param>
		protected virtual void OnRemove(NotifyCollectionChangedEventArgs collectionArgs,
			bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) { }

		/// <summary>
		/// The properties affected by a replacement action.
		/// </summary>
		protected readonly ISet<string> replaceProperties = new HashSet<string>(new string[1]{
			nameof(Values)
		});
		/// <summary>
		/// Notify all relevant listeners that the value associated with some
		/// key is changed in <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// <paramref name="oldItem"/> and <paramref name="newItem"/> are
		/// ignored unless <paramref name="collectionArgs"/> is <c>null</c>.
		/// <para/>
		/// <paramref name="action"/> must be able to be expressed as one of:
		/// <see cref="Action"/>, <c>Func&lt;bool&gt;</c>,
		/// <c>Func&lt;TValue&gt;</c>, or
		/// <c>Func&lt;Tuple&lt;bool, TValue&gt;&gt;</c>.
		/// </remarks>
		/// 
		/// <param name="action">The action that causes the removal.</param>
		/// <param name="newItem">
		/// The item as it will be represented in <see cref="Dictionary"/>, or
		/// <c>null</c> if it is already contained in
		/// <paramref name="collectionArgs"/>.
		/// </param>
		/// <param name="oldItem">
		/// The item as it was previously represented in
		/// <see cref="Dictionary"/>, or <c>null</c> if it is already
		/// contained in <paramref name="collectionArgs"/>.
		/// </param>
		/// <param name="preTest">
		/// A simplified approximation of the test performed by
		/// <paramref name="action"/>, but that doesn't result in any changes,
		/// or <c>null</c> if it will always run.
		/// </param>
		/// <param name="collectionArgs">
		/// A description of how the collection is changed, or <c>null</c> to
		/// generate a new value using <paramref name="oldItem"/> and
		/// <paramref name="newItem"/>.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="replaceProperties"/>.
		/// </param>
		/// 
		/// <returns>The value returned by <paramref name="action"/>.</returns>
		protected Tuple<bool, TValue> SendReplaceEvents(object action, object newItem = null, object oldItem = null,
				Func<bool> preTest = null, NotifyCollectionChangedEventArgs collectionArgs = null,
				bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) {
			var actionFunc = ParseAction(action);
			if (preTest == null)
				preTest = (() => true);

			if (collectionArgs == null) {
				if ((newItem is IList newItemList) && (oldItem is IList oldItemList)) {
					collectionArgs = new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Replace,
						newItemList,
						oldItemList
					);
				} else {
					collectionArgs = new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Replace,
						newItem,
						oldItem
					);
				}
			}

			if (preTest.Invoke()) {
#if SUPPORT_PROPERTYCHANGING_EVENT
				if (deferChanging == false)
					foreach (var prop in (properties ?? replaceProperties))
						PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(prop));
#endif
				OnReplacing(collectionArgs, deferChanging, deferChanged, properties);
			}

			var result = actionFunc.Invoke();
			if (result.Item1) {
				CollectionChanged?.Invoke(this, collectionArgs);
				if (deferChanged == false)
					foreach (var prop in (properties ?? replaceProperties))
						PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

				OnReplace(collectionArgs, deferChanging, deferChanged, properties);
			}

			return result;
		}
		/// <summary>
		/// Notify all relevant listeners that the value associated with some
		/// key is changed in <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// <paramref name="action"/> must be able to be expressed as one of:
		/// <see cref="Action"/>, <c>Func&lt;bool&gt;</c>,
		/// <c>Func&lt;TValue&gt;</c>, or
		/// <c>Func&lt;Tuple&lt;bool, TValue&gt;&gt;</c>.
		/// </remarks>
		/// 
		/// <param name="action">The action that causes the removal.</param>
		/// <param name="key">The key at which the change occurred.</param>
		/// <param name="newValue">
		/// The new value associated with <paramref name="key"/>.
		/// </param>
		/// <param name="oldValue">
		/// The value previously associated with <paramref name="key"/>.
		/// </param>
		/// <param name="preTest">
		/// A simplified approximation of the test performed by
		/// <paramref name="action"/>, but that doesn't result in any changes,
		/// or <c>null</c> if it will always run.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="replaceProperties"/>.
		/// </param>
		/// 
		/// <returns>The value returned by <paramref name="action"/>.</returns>
		protected Tuple<bool, TValue> SendReplaceEvents(object action, TKey key, TValue newValue, TValue oldValue, Func<bool> preTest = null,
				bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) =>
			SendReplaceEvents(action, new KeyValuePair<TKey, TValue>(key, newValue), new KeyValuePair<TKey, TValue>(key, oldValue), preTest);
		/// <summary>
		/// Perform any implementation-specific event handling when an item is
		/// just about to be changed in <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <param name="collectionArgs">
		/// A description of how the collection will be changed.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="replaceProperties"/>.
		/// </param>
		protected virtual void OnReplacing(NotifyCollectionChangedEventArgs collectionArgs,
			bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) { }
		/// <summary>
		/// Perform any implementation-specific event handling when an item
		/// has been changed in <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <param name="collectionArgs">
		/// A description of how the collection was changed.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="replaceProperties"/>.
		/// </param>
		protected virtual void OnReplace(NotifyCollectionChangedEventArgs collectionArgs,
			bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) { }

		/// <summary>
		/// The properties affected by a reset action.
		/// </summary>
		protected virtual ISet<string> ResetProperties => new HashSet<string>(new string[3]{
			nameof(Keys),
			nameof(Values),
			nameof(Count)
		});
		/// <summary>
		/// Notify all relevant listeners that <see cref="Dictionary"/> is
		/// cleared.
		/// </summary>
		/// 
		/// <remarks>
		/// <paramref name="action"/> must be able to be expressed as one of:
		/// <see cref="Action"/>, <c>Func&lt;bool&gt;</c>,
		/// <c>Func&lt;TValue&gt;</c>, or
		/// <c>Func&lt;Tuple&lt;bool, TValue&gt;&gt;</c>.
		/// </remarks>
		/// 
		/// <param name="action">The action that causes the removal.</param>
		/// <param name="preTest">
		/// A simplified approximation of the test performed by
		/// <paramref name="action"/>, but that doesn't result in any changes,
		/// or <c>null</c> if it will always run.
		/// </param>
		/// <param name="collectionArgs">
		/// A description of how the collection is changed, or <c>null</c> to
		/// generate a new value.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="ResetProperties"/>.
		/// </param>
		/// 
		/// <returns>The value returned by <paramref name="action"/>.</returns>
		protected Tuple<bool, TValue> SendResetEvents(object action,
				Func<bool> preTest = null, NotifyCollectionChangedEventArgs collectionArgs = null,
				bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) {
			var actionFunc = ParseAction(action);
			if (preTest == null)
				preTest = (() => true);

			if (collectionArgs == null) {
				collectionArgs = new NotifyCollectionChangedEventArgs(
					NotifyCollectionChangedAction.Reset
				);
			}

			if (preTest.Invoke()) {
#if SUPPORT_PROPERTYCHANGING_EVENT
				if (deferChanging == false)
					foreach (var prop in (properties ?? ResetProperties))
						PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(prop));
#endif
				OnResetting(collectionArgs, deferChanging, deferChanged, properties);
			}

			var result = actionFunc.Invoke();
			if (result.Item1) {
				CollectionChanged?.Invoke(this, collectionArgs);
				if (deferChanged == false)
					foreach (var prop in (properties ?? ResetProperties))
						PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

				OnReset(collectionArgs, deferChanging, deferChanged, properties);
			}

			return result;
		}
		/// <summary>
		/// Perform any implementation-specific event handling when
		/// <see cref="Dictionary"/> is just about to be cleared.
		/// </summary>
		/// 
		/// <param name="collectionArgs">
		/// A description of how the collection will be changed.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="ResetProperties"/>.
		/// </param>
		protected virtual void OnResetting(NotifyCollectionChangedEventArgs collectionArgs,
			bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) { }
		/// <summary>
		/// Perform any implementation-specific event handling when
		/// <see cref="Dictionary"/> has been cleared.
		/// </summary>
		/// 
		/// <param name="collectionArgs">
		/// A description of how the collection was changed.
		/// </param>
		/// <param name="deferChanging">
		/// Whether to skip notifying listeners that a property is about to
		/// change; defaults to <c>false</c>.
		/// </param>
		/// <param name="deferChanged">
		/// Whether to skip notifying listeners that a property has just
		/// changed; defaults to <c>false</c>.
		/// </param>
		/// <param name="properties">
		/// The properties that will be affected by the change, or <c>null</c>
		/// to use <see cref="ResetProperties"/>.
		/// </param>
		protected virtual void OnReset(NotifyCollectionChangedEventArgs collectionArgs,
			bool deferChanging = false, bool deferChanged = false, ICollection<string> properties = null) { }
#endregion

#region Item accessors
		/// <summary>
		/// Gets or sets the element with the specified key.
		/// </summary>
		/// 
		/// <remarks>
		/// You can use this property to add new elements by setting the value
		/// of a key that does not exist in the dictionary; for example, 
		/// <c>myDictionary["myNonexistentKey"] = myValue</c> in C# 
		/// (<c>myCollection("myNonexistentKey") = myValue</c> in Visual
		/// Basic). However, if the specified key already exists in the
		/// dictionary, setting the value in this manner overwrites the
		/// existing element. In contrast, the <see cref="Add(TKey, TValue)"/>
		/// method does not modify existing elements.
		/// <para/>
		/// Implementations can vary in whether they allow <paramref name="key"/>
		/// to be <c>null</c>, but the assigned value can always be as long as
		/// <typeparamref name="TValue"/> is a reference type.
		/// <para/>
		/// Getting or setting the value of this property approaches an
		/// <c>O(1)</c> operation.
		/// </remarks>
		/// 
		/// <param name="key">The key of the element to get or set.</param>
		/// 
		/// <value>The element with the specified key.</value>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c> and the implementation does
		/// not support this.
		/// </exception>
		/// <exception cref="KeyNotFoundException">
		/// The property is used as an accessor and <paramref name="key"/> is
		/// not found.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The property is used as a setter and the implementation is
		/// read-only.
		/// </exception>
		/// 
		/// <seealso cref="IsReadOnly"/>
		public TValue this[TKey key] {
			get => Dictionary[key];
			set {
				if (Dictionary.ContainsKey(key))
					Update(key, value);
				else
					Add(key, value);
			}
		}
		/// <summary>
		/// Gets or sets the element with the specified key.
		/// </summary>
		/// 
		/// <remarks>
		/// You can use this property to add new elements by setting the value
		/// of a key that does not exist in the dictionary; for example, 
		/// <c>myDictionary["myNonexistentKey"] = myValue</c> in C# 
		/// (<c>myCollection("myNonexistentKey") = myValue</c> in Visual
		/// Basic). However, if the specified key already exists in the
		/// dictionary, setting the value in this manner overwrites the
		/// existing element. In contrast, the <see cref="Add(TKey, TValue)"/>
		/// method does not modify existing elements.
		/// <para/>
		/// Implementations can vary in whether they allow <paramref name="key"/>
		/// to be <c>null</c>, but the assigned value can always be as long as
		/// <typeparamref name="TValue"/> is a reference type.
		/// <para/>
		/// Getting or setting the value of this property approaches an
		/// <c>O(1)</c> operation.
		/// </remarks>
		/// 
		/// <param name="key">The key of the element to get or set.</param>
		/// 
		/// <value>
		/// The element with the specified key, or <c>null</c> if the key does
		/// not exist.
		/// </value>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c> and the implementation does
		/// not support this.
		/// </exception>
		/// <exception cref="KeyNotFoundException">
		/// The property is used as an accessor and <paramref name="key"/> is
		/// not found.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The property is used as a setter and the implementation is read-
		/// only.
		/// </exception>
		/// 
		/// <seealso cref="IsReadOnly"/>
		object IDictionary.this[object key] {
			get => (Dictionary as IDictionary)?[key];
			set {
				var converted = Dictionary as IDictionary;
				// Shouldn't happen as IDictionary<TKey, TValue> implements
				// IDictionary, but check just in case
				if (converted == null)
					return;

				if (converted.Contains(key))
					Update((TKey)key, (TValue)value);
				else
					Add((TKey)key, (TValue)value);
			}
		}

		/// <summary>
		/// Gets an <see cref="ICollection{T}"/> containing the keys of
		/// <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// The order of the keys in the returned <see cref="ICollection{T}"/>
		/// is unspecified, but it is guaranteed to be the same order as the
		/// corresponding values in the <see cref="ICollection{T}"/> returned
		/// by the <see cref="Values"/> property.
		/// </remarks>
		/// 
		/// <value>
		/// An <see cref="ICollection{T}"/> containing the keys of the object
		/// that implements <see cref="IDictionary{TKey, TValue}"/>.
		/// </value>
		public ICollection<TKey> Keys =>
			Dictionary.Keys;
		/// <summary>
		/// Gets an <see cref="ICollection"/> containing the keys of
		/// <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// The order of the keys in the returned <see cref="ICollection"/>
		/// is unspecified, but it is guaranteed to be the same order as the
		/// corresponding values in the <see cref="ICollection"/> returned
		/// by the <see cref="Values"/> property.
		/// </remarks>
		/// 
		/// <value>
		/// An <see cref="ICollection"/> containing the keys of the object
		/// that implements <see cref="IDictionary{TKey, TValue}"/>.
		/// </value>
		ICollection IDictionary.Keys =>
			// Shouldn't be null as IDictionary<TKey, TValue> implements
			// IDictionary, but check just in case
			(Dictionary as IDictionary)?.Keys;
		/// <summary>
		/// Gets a read-only, enumerable collection containing the keys of
		/// <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// The order of the keys in the returned <see cref="IEnumerable{T}"/>
		/// is unspecified, but it is guaranteed to be the same order as the
		/// corresponding values in the <see cref="IEnumerable{T}"/> returned
		/// by the <see cref="Values"/> property.
		/// </remarks>
		/// 
		/// <value>
		/// An <see cref="IEnumerable{T}"/> containing the keys of the object
		/// that implements <see cref="IDictionary{TKey, TValue}"/>.
		/// </value>
		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

		/// <summary>
		/// Gets an <see cref="ICollection{T}"/> containing the values of
		/// <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// The order of the values in the returned <see cref="ICollection{T}"/>
		/// is unspecified, but it is guaranteed to be the same order as the
		/// corresponding keys in the <see cref="ICollection{T}"/> returned
		/// by the <see cref="Keys"/> property.
		/// </remarks>
		/// 
		/// <value>
		/// An <see cref="ICollection{T}"/> containing the values of the object
		/// that implements <see cref="IDictionary{TKey, TValue}"/>.
		/// </value>
		public ICollection<TValue> Values =>
			Dictionary.Values;
		/// <summary>
		/// Gets an <see cref="ICollection"/> containing the values of
		/// <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// The order of the keys in the returned <see cref="ICollection"/>
		/// is unspecified, but it is guaranteed to be the same order as the
		/// corresponding keys in the <see cref="ICollection"/> returned
		/// by the <see cref="Keys"/> property.
		/// </remarks>
		/// 
		/// <value>
		/// An <see cref="ICollection{T}"/> containing the values of the object
		/// that implements <see cref="IDictionary{TKey, TValue}"/>.
		/// </value>
		ICollection IDictionary.Values =>
			// Shouldn't be null as IDictionary<TKey, TValue> implements
			// IDictionary, but check just in case
			(Dictionary as IDictionary)?.Values;

		/// <summary>
		/// Gets a read-only, enumerable collection containing the values of
		/// <see cref="Dictionary"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// The order of the values in the returned <see cref="IEnumerable{T}"/>
		/// is unspecified, but it is guaranteed to be the same order as the
		/// corresponding keys in the <see cref="IEnumerable{T}"/> returned
		/// by the <see cref="Keys"/> property.
		/// </remarks>
		/// 
		/// <value>
		/// An <see cref="IEnumerable{T}"/> containing the keys of the object
		/// that implements <see cref="IDictionary{TKey, TValue}"/>.
		/// </value>
		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values =>
			Values;

		/// <summary>
		/// Determines whether the <see cref="IDictionary{TKey, TValue}"/>
		/// contains a specific item.
		/// </summary>
		/// 
		/// <param name="item">
		/// The <see cref="KeyValuePair{TKey, TValue}"/> to locate.
		/// </param>
		/// 
		/// <returns>
		/// <c>true</c> if the value in the
		/// <see cref="IDictionary{TKey, TValue}"/> associated with the key of
		/// <paramref name="item"/> matches the latter's value; otherwise
		/// <c>false</c>.
		/// </returns>
		public bool Contains(KeyValuePair<TKey, TValue> item) =>
			Dictionary.Contains(item);

		/// <summary>
		/// Determines whether the <see cref="IDictionary{TKey, TValue}"/>
		/// contains an element with the specified key.
		/// </summary>
		/// 
		/// <param name="key">The key to locate.</param>
		/// 
		/// <returns>
		/// <c>true</c> if the <see cref="IDictionary{TKey, TValue}"/>
		/// contains an element associated with <paramref name="key"/>;
		/// otherwise, <c>false</c>.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		public bool ContainsKey(TKey key) =>
			Dictionary.ContainsKey(key);
		/// <summary>
		/// Determines whether the <see cref="IDictionary"/> contains an
		/// element with the specified key.
		/// </summary>
		/// 
		/// <param name="item">The key to locate.</param>
		/// 
		/// <returns>
		/// <c>true</c> if the <see cref="IDictionary"/> contains an element
		/// associated with a key <paramref name="item"/>; otherwise,
		/// <c>false</c>.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="item"/> is <c>null</c>.
		/// </exception>
		/// 
		/// <seealso cref="ContainsKey(TKey)"/>
		bool IDictionary.Contains(object item) =>
			// Shouldn't be null as IDictionary<TKey, TValue> implements
			// IDictionary, but check just in case
			(Dictionary as IDictionary)?.Contains(item) ?? false;

		/// <summary>
		/// Returns an enumerator that iterates through the key-value pairs in
		/// the dictionary.
		/// </summary>
		/// 
		/// <returns>
		/// An enumerator that can be used to iterate through the dictionary.
		/// </returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
			Dictionary.GetEnumerator();
		/// <summary>
		/// Returns an enumerator that iterates through the key-value pairs in
		/// the dictionary as <see cref="DictionaryEntry"/> instances boxed to
		/// <see cref="Object"/>.
		/// </summary>
		/// 
		/// <returns>
		/// An enumerator that can be used to iterate through the dictionary.
		/// </returns>
		IDictionaryEnumerator IDictionary.GetEnumerator() =>
			// Shouldn't be null as IDictionary<TKey, TValue> implements
			// IDictionary, but check just in case
			(Dictionary as IDictionary)?.GetEnumerator();
		/// <summary>
		/// Returns an enumerator that iterates through the key-value pairs in
		/// the dictionary.
		/// </summary>
		/// 
		/// <returns>
		/// An enumerator that can be used to iterate through the dictionary.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator() =>
			Dictionary.GetEnumerator();

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// 
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">
		/// When this method returns, the value associated with
		/// <paramref name="key"/>, if that key is found; otherwise, the
		/// default value for the type of the value parameter. This parameter
		/// is passed uninitialized.
		/// </param>
		/// 
		/// <returns></returns>
		public bool TryGetValue(TKey key, out TValue value) =>
			Dictionary.TryGetValue(key, out value);
#endregion

#region Properties
		/// <summary>
		/// Gets the number of key-value pairs contained in
		/// <see cref="Dictionary"/>.
		/// </summary>
		public int Count =>
				Dictionary.Count;
		/// <summary>
		/// Gets the number of key-value pairs contained in
		/// <see cref="Dictionary"/>.
		/// </summary>
		int ICollection.Count =>
			// Shouldn't be null as IDictionary<TKey, TValue> implements
			// IDictionary, but check just in case
			(Dictionary as IDictionary)?.Count ?? 0;

		/// <summary>
		/// Indicates whether this dictionary can be modified after creation.
		/// </summary>
		/// 
		/// <remarks>
		/// Accessing this value runs in <c>O(1)</c> time.
		/// </remarks>
		bool IsReadOnly =>
			Dictionary.IsReadOnly;
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
		/// Gets a value indicating whether <see cref="Dictionary"/> has a
		/// fixed size.
		/// </summary>
		/// 
		/// <remarks>
		/// A collection with a fixed size does not allow the addition or
		/// removal of elements after the collection is created, but does
		/// allow the modification of existing elements.
		/// <para/>
		/// A collection with a fixed size is simply a collection with a
		/// wrapper that prevents adding and removing elements; therefore, if
		/// changes are made to the underlying collection, including the
		/// addition or removal of elements, the fixed-size collection
		/// reflects those changes.
		/// </remarks>
		bool IDictionary.IsFixedSize =>
			// Shouldn't be null as IDictionary<TKey, TValue> implements
			// IDictionary, but check just in case
			(Dictionary as IDictionary)?.IsFixedSize ?? false;

		/// <summary>
		/// Gets a value indicating whether access to <see cref="Dictionary"/>
		/// is synchronized (thread safe).
		/// </summary>
		bool ICollection.IsSynchronized =>
			// Shouldn't be null as IDictionary<TKey, TValue> implements
			// IDictionary, but check just in case
			(Dictionary as IDictionary)?.IsSynchronized ?? false;

		/// <summary>
		/// Gets an object that can be used to synchronize access to the
		/// <see cref="IDictionary"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// For collections whose underlying store is not publicly available,
		/// the expected implementation is to return the current instance.
		/// Note that the pointer to the current instance might not be
		/// sufficient for collections that wrap other collections; those
		/// should return the underlying collection's <c>SyncRoot</c> property.
		/// <para/>
		/// Most collection classes in the <see cref="System.Collections"/>
		/// namespace also implement a <c>Synchronized</c> method, which
		/// provides a synchronized wrapper around the underlying collection.
		/// However, derived classes can provide their own synchronized
		/// version of the collection using the <c>SyncRoot</c> property. The
		/// synchronizing code must perform operations on the <c>SyncRoot</c>
		/// property of the collection, not directly on the collection. This
		/// ensures proper operation of collections that are derived from
		/// other objects. Specifically, it maintains proper synchronization
		/// with other threads that might be simultaneously modifying the
		/// collection instance.
		/// <para/>
		/// Enumerating through a collection is intrinsically not a
		/// thread-safe procedure. Even when a collection is synchronized,
		/// other threads can still modify the collection, which causes the
		/// enumerator to throw an exception. To guarantee thread safety
		/// during enumeration, you can either lock the collection during the
		/// entire enumeration or catch the exceptions resulting from changes
		/// made by other threads.
		/// </remarks>
		object ICollection.SyncRoot =>
			(Dictionary as IDictionary)?.SyncRoot;
#endregion

#region Modifiers
		/// <summary>
		/// Adds an element with the provided key and value to the
		/// <see cref="IDictionary{TKey, TValue}"/>.
		/// </summary>
		/// 
		/// <param name="key">
		/// The object to use as the key of the element to add.
		/// </param>
		/// <param name="value">
		/// The object to use as the value of the element to add.
		/// </param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// An element with the same key already exists in the
		/// <see cref="IDictionary{TKey, TValue}"/>.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The <see cref="IDictionary{TKey, TValue}"/> is read-only.
		/// </exception>
		/// 
		/// <seealso cref="IsReadOnly"/>
		public void Add(TKey key, TValue value) =>
			Add(new KeyValuePair<TKey, TValue>(key, value));
		/// <summary>
		/// Adds a pre-created <see cref="KeyValuePair{TKey, TValue}"/> to the
		/// <see cref="IDictionary{TKey, TValue}"/>.
		/// </summary>
		/// 
		/// <param name="item">The item to add.</param>
		public virtual void Add(KeyValuePair<TKey, TValue> item) =>
			SendAddEvents(new Action(() => Dictionary.Add(item)), item, new Func<bool>(() => Dictionary.ContainsKey(item.Key) == false));
		/// <summary>
		/// Adds an element with the provided key and value to the
		/// <see cref="IDictionary"/>.
		/// </summary>
		/// 
		/// <param name="key">
		/// The <see cref="Object"/> to use as the key of the element to add.
		/// </param>
		/// <param name="value">
		/// The <see cref="Object"/> to use as the value of the element to add.
		/// </param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// An element with the same key already exists in the
		/// <see cref="IDictionary{TKey, TValue}"/>.
		/// </exception>
		/// <exception cref="InvalidCastException">
		/// <paramref name="key"/> can not be expressed as a <c>TKey</c>, or
		/// <paramref name="value"/> can not be expressed as a <c>TValue</c>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The <see cref="IDictionary{TKey, TValue}"/> is read-only.
		/// </exception>
		/// 
		/// <seealso cref="IsReadOnly"/>
		void IDictionary.Add(object key, object value) =>
			Add((TKey)key, (TValue)value);

		/// <summary>
		/// Removes all items from the <see cref="IDictionary{TKey, TValue}"/>.
		/// </summary>
		/// 
		/// <exception cref="NotSupportedException">
		/// The <see cref="IDictionary{TKey, TValue}"/> is read-only.
		/// </exception>
		/// 
		/// <seealso cref="IsReadOnly"/>
		public virtual void Clear() =>
			SendResetEvents(new Action(() => Dictionary.Clear()));

		/// <summary>
		/// Copies the elements of the <see cref="IDictionary{TKey, TValue}"/>
		/// to an <see cref="Array"/>, starting at a particular index.
		/// </summary>
		/// 
		/// <param name="array">
		/// The one-dimensional <see cref="Array"/> that is the destination of
		/// the elements copied from the
		/// <see cref="IDictionary{TKey, TValue}"/>. The <see cref="Array"/>
		/// must have zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">
		/// The zero-based index in <paramref name="array"/> at which copying
		/// begins.
		/// </param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is less than 0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The number of elements in the
		/// <see cref="IDictionary{TKey, TValue}"/> is greater than the
		/// available space from <paramref name="arrayIndex"/> to the end of
		/// the destination array.
		/// </exception>
		public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
			Dictionary.CopyTo(array, arrayIndex);
		/// <summary>
		/// Copies the elements of the <see cref="IDictionary{TKey, TValue}"/>
		/// to an <see cref="Array"/>, starting at a particular index.
		/// </summary>
		/// 
		/// <param name="array">
		/// The one-dimensional <see cref="Array"/> that is the destination of
		/// the elements copied from the
		/// <see cref="IDictionary{TKey, TValue}"/>. The <see cref="Array"/>
		/// must have zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">
		/// The zero-based index in <paramref name="array"/> at which copying
		/// begins.
		/// </param>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is less than 0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="array"/> is multidimensional.
		/// <para/>
		/// -or-
		/// <para/>
		/// The number of elements in the
		/// <see cref="IDictionary{TKey, TValue}"/> is greater than the
		/// available space from <paramref name="arrayIndex"/> to the end of
		/// the destination array.
		/// <para/>
		/// -or-
		/// <para/>
		/// <typeparamref name="TKey"/> cannot be cast automatically to the
		/// underlying type of <paramref name="array"/>.
		/// </exception>
		void ICollection.CopyTo(Array array, int arrayIndex) =>
			(Dictionary as IDictionary).CopyTo(array, arrayIndex);

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
		/// 
		/// <seealso cref="IsReadOnly"/>
		public virtual bool Remove(TKey key) {
			var containedKey = Dictionary.TryGetValue(key, out TValue oldValue);

			return SendRemoveEvents(new Func<bool>(() => Dictionary.Remove(key)), key, oldValue, new Func<bool>(() => containedKey)).Item1;
		}
		/// <summary>
		/// Removes the element with the specified key from the
		/// <see cref="IDictionary{TKey, TValue}"/> if it also has the
		/// specified value.
		/// </summary>
		/// 
		/// <param name="item">The element to remove.</param>
		/// 
		/// <returns>
		/// <c>true</c> if the element is successfully removed; otherwise,
		/// <c>false</c>. This method also returns <c>false</c> if
		/// such a pair was not found in the original
		/// <see cref="IDictionary{TKey, TValue}"/>.
		/// </returns>
		/// 
		/// <exception cref="NotSupportedException">
		/// The <see cref="IDictionary{TKey, TValue}"/> is read-only.
		/// </exception>
		/// 
		/// <seealso cref="IsReadOnly"/>
		public bool Remove(KeyValuePair<TKey, TValue> item) {
			if (Dictionary.Contains(item))
				return Remove(item.Key);
			else
				return false;
		}
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
		/// 
		/// <seealso cref="IsReadOnly"/>
		void IDictionary.Remove(object key) =>
			Remove((TKey)key);

		/// <summary>
		/// Replace the value of an existing item in the
		/// <see cref="IDictionary{TKey, TValue}"/> with a new one.
		/// </summary>
		/// 
		/// <param name="key">The key of the element to change.</param>
		/// <param name="value">The new value of the item.</param>
		public virtual void Update(TKey key, TValue value) {
			if (Dictionary.TryGetValue(key, out var oldValue) && oldValue.Equals(value))
				SendReplaceEvents(new Action(() => Dictionary[key] = value), key, value, oldValue);
		}
#endregion
	}
}
