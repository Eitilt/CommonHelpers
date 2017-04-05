/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace AgEitilt.Common.Dictionary {
	/// <summary>
	/// Extension methods on the <see cref="IDictionary{TKey, TValue}"/>
	/// interface.
	/// </summary>
	public static class DictionaryExtension {
		/// <summary>
		/// Retrieve the requested value from the dictionary, creating a new
		/// entry if necessary.
		/// </summary>
		/// 
		/// <typeparam name="TKey">
		/// The type used for the dictionary's keys.
		/// </typeparam>
		/// <typeparam name="TValue">
		/// The type used for the dictionary's values.
		/// </typeparam>
		/// 
		/// <param name="dictionary">
		/// The <see cref="IDictionary{TKey, TValue}"/> on which to operate.
		/// </param>
		/// <param name="key">The dictionary key to access.</param>
		/// 
		/// <returns>
		/// The object located at <paramref name="key"/> in
		/// <paramref name="dictionary"/>.
		/// </returns>
		public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
				where TValue : new() {
			if (dictionary.ContainsKey(key) == false)
				dictionary.Add(key, new TValue());

			return dictionary[key];
		}
	}
}
