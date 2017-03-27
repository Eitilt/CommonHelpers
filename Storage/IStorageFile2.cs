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
