/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Linq;

namespace AgEitilt.Common.Dictionary {
	/// <summary>
	/// Test sequences for equality based on their values, not their
	/// object references.
	/// </summary>
	/// 
	/// <remarks>
	/// Implementation from <see href="http://stackoverflow.com/a/7244729"/>.
	/// </remarks>
	/// 
	/// <typeparam name="TElement">
	/// The underlying type of the sequence.
	/// </typeparam>
	public class ArrayEqualityComparer<TElement> : IEqualityComparer<TElement[]> {
		/// <summary>
		/// Provide a more robust meant of testing the equality of
		/// elements.
		/// </summary>
		/// 
		/// <remarks>
		/// TODO: Allow specifying custom EqualityComparer.
		/// </remarks>
		static readonly EqualityComparer<TElement> elementComparer = EqualityComparer<TElement>.Default;

		/// <summary>
		/// Check two sequences for value equality.
		/// </summary>
		/// 
		/// <returns>Whether the sequences are equal.</returns>
		public bool Equals(TElement[] x, TElement[] y) {
			if (x == y)
				return true;
			else if ((x == null) || (y == null))
				return false;

			if (x.Length != y.Length)
				return false;

			return x.SequenceEqual(y, elementComparer);
		}

		/// <summary>
		/// Calculate a hash code based on the values of the sequence.
		/// </summary>
		/// 
		/// <param name="obj">The sequence to hash.</param>
		/// 
		/// <returns>The calculated hash.</returns>
		public int GetHashCode(TElement[] obj) {
			if (obj == null)
				return 0;

			int hash = 17;
			foreach (TElement t in obj) {
				hash *= 31;
				hash += elementComparer.GetHashCode(t);
			}

			return hash;
		}
	}
}
