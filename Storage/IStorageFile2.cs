using System.IO;
using System.Threading.Tasks;

namespace AgEitilt.Common.Storage {
	/// <summary>
	/// Provides additional methods for manipulating files.
	/// </summary>
	public interface IStorageFile2 : IStorageFile {
		/// <summary>
		/// Opens a random-access stream over the file.
		/// </summary>
		/// 
		/// <remarks>
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a <see cref="Task{TResult}"/> rather than the modern
		/// <c>IAsyncOperation&lt;...&gt;</c>. Additionally, the value type
		/// returned by awaiting that task is a <see cref="FileStream"/>
		/// rather than a <c>IRandomAccessStream</c>; this is particularly
		/// dangerous because the former doesn't guarantee as strongly that
		/// reading from it will result in that number of bytes.
		/// <para/>
		/// TODO: Expand the package to include that interface, to avoid any
		/// issues that may occur.
		/// </remarks>
		/// 
		/// <param name="accessMode">The type of access to request.</param>
		/// <param name="options">
		/// The allowed interactions between multiple streams reading from or
		/// writing to this file.
		/// </param>
		/// 
		/// <returns>
		/// A stream providing access to the data within the file, once the
		/// <see cref="Task{TResult}"/> completes.
		/// </returns>
		/// 
		/// <seealso cref="IStorageFile.OpenAsync(FileAccessMode)"/>
		Task<FileStream> OpenAsync(FileAccessMode accessMode, StorageOpenOptions options);

		//TODO: Add OpenTransactedWriteAsync once we have added a concept of
		// transactions
	}
}
