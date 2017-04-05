/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;

namespace AgEitilt.Common.Stream {
	/// <summary>
	/// Extension methods on the <see cref="System.IO.Stream"/> class.
	/// </summary>
	public static class StreamExtension {
		/// <summary>
		/// Read from the stream until it ends or the requested number of
		/// bytes has been reached.
		/// </summary>
		/// 
		/// <param name="stream">
		/// The <see cref="System.IO.Stream"/> on which to operate.
		/// </param>
		/// <param name="buffer">
		/// The destination in which to save the read bytes.
		/// </param>
		/// <param name="offset">
		/// The index in <paramref name="buffer"/> of the first read byte.
		/// </param>
		/// <param name="count">The number of bytes to read.</param>
		/// 
		/// <returns>
		/// The number of bytes that were successfully read. This may be less
		/// than <paramref name="count"/> if the stream ended before that
		/// number of bytes was reached.
		/// </returns>
		public static int ReadAll(this System.IO.Stream stream, byte[] buffer, int offset, int count) {
			int total = 0;

			while (count > 0) {
				int read = stream.Read(buffer, offset, count);
				if (read == 0) {
					break;
				} else {
					offset += read;
					total += read;
					count -= read;
				}
			}

			return total;
		}

		/// <summary>
		/// Read from the stream asynchronously until it ends or the requested
		/// number of bytes has been reached.
		/// </summary>
		/// 
		/// <param name="stream">
		/// The <see cref="System.IO.Stream"/> on which to operate.
		/// </param>
		/// <param name="buffer">
		/// The destination in which to save the read bytes.
		/// </param>
		/// <param name="offset">
		/// The index in <paramref name="buffer"/> of the first read byte.
		/// </param>
		/// <param name="count">The number of bytes to read.</param>
		/// 
		/// <returns>
		/// The number of bytes that were successfully read. This may be less
		/// than <paramref name="count"/> if the stream ended before that
		/// number of bytes was reached.
		/// </returns>
		public async static Task<int> ReadAllAsync(this System.IO.Stream stream, byte[] buffer, int offset, int count) {
			int total = 0;

			while (count > 0) {
				int read = await stream.ReadAsync(buffer, offset, count);
				if (read == 0) {
					break;
				} else {
					offset += read;
					total += read;
					count -= read;
				}
			}

			return total;
		}
	}
}
