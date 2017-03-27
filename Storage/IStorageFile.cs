using System.IO;
using System.Threading.Tasks;

namespace AgEitilt.Common.Storage {
	/// <summary>
	/// Represents a file. Provides information about the file and its
	/// contents, and ways to manipulate both.
	/// </summary>
	/// 
	/// <seealso cref="StorageFile"/>
	public interface IStorageFile : IStorageItem {
		/// <summary>
		/// Get the MIME type of the file contents.
		/// </summary>
		/// 
		/// <value>
		/// The type in standard MIME format; for example, <c>audio/mpeg</c>.
		/// </value>
		string ContentType { get; }

		/// <summary>
		/// Gets the type of the file as declared by its extension.
		/// </summary>
		/// 
		/// <value>
		/// The file extension; for example, <c>jpg</c>.
		/// </value>
		string FileType { get; }

		/// <summary>
		/// Replaces the specified file with a copy of this file.
		/// </summary>
		/// 
		/// <param name="fileToReplace">The file to replace.</param>
		/// 
		/// <returns>
		/// An awaitable <see cref="Task"/>, with no associated value.
		/// </returns>
		/// 
		/// <seealso cref="MoveAndReplaceAsync(IStorageFile)"/>
		/// <seealso cref="CopyAsync(IStorageFolder, string, NameCollisionOption)"/>
		Task CopyAndReplaceAsync(IStorageFile fileToReplace);

		/// <summary>
		/// Creates a copy of this file in the specified folder.
		/// </summary>
		/// 
		/// <remarks>
		/// The default value of <paramref name="option"/> was chosen for
		/// compatability with the <c>Windows.Storage</c> implementation.
		/// </remarks>
		/// 
		/// <param name="destinationFolder">
		/// The folder where the copy will be created.
		/// </param>
		/// <param name="option">
		/// The method used to handle situations where a file or folder by
		/// this name already exists in <paramref name="destinationFolder"/>.
		/// <para />
		/// Default value is <see cref="NameCollisionOption.FailIfExists"/>.
		/// </param>
		/// 
		/// <returns>
		/// A handle to the new file, once the <see cref="Task{TResult}"/>
		/// completes.
		/// </returns>
		/// 
		/// <seealso cref="MoveAsync(IStorageFolder, NameCollisionOption)"/>
		Task<StorageFile> CopyAsync(IStorageFolder destinationFolder, NameCollisionOption option = NameCollisionOption.FailIfExists);
		/// <summary>
		/// Creates a copy of this file in the specified folder, under a new
		/// name.
		/// </summary>
		/// 
		/// <param name="destinationFolder">
		/// The folder where the copy will be created.
		/// </param>
		/// <param name="desiredName">
		/// The name under which the copy will try to be saved. If a file of
		/// this name already exists in <paramref name="destinationFolder"/>,
		/// the behaviour is determined by <paramref name="option"/>.
		/// </param>
		/// <param name="option">
		/// The method used to handle situations where a file or folder by the
		/// name of <paramref name="desiredName"/> already exists in
		/// <paramref name="destinationFolder"/>.
		/// <para />
		/// Default value is
		/// <see cref="NameCollisionOption.GenerateUniqueName"/>.
		/// </param>
		/// 
		/// <returns>
		/// A handle to the new file, once the <see cref="Task{TResult}"/>
		/// completes.
		/// </returns>
		/// 
		/// <seealso cref="IStorageItem.Name"/>
		/// <seealso cref="MoveAsync(IStorageFolder, string, NameCollisionOption)"/>
		/// <seealso cref="CopyAndReplaceAsync(IStorageFile)"/>
		Task<StorageFile> CopyAsync(
			IStorageFolder destinationFolder,
			string desiredName,
			NameCollisionOption option = NameCollisionOption.GenerateUniqueName
		);

		/*TODO: For the Move* functions, but not the Copy*, an exception is
		 * thrown if `destinationFolder` is memory-only, not physical. If such
		 * virtual files are introduced, it may be better to break documented
		 * compatibility and add the throws to the Copy* functions as well.
		 * Either way, if memory folders are implemented, add the <exception/>
		 * documentation to the comments.
		 */

		/// <summary>
		/// Replaces the specified file with this file, removing the latter
		/// from its current location.
		/// </summary>
		/// 
		/// <remarks>
		/// This has the same result as calling
		/// <see cref="MoveAsync(IStorageFolder, string, NameCollisionOption)"/>
		/// with <c>desiredName</c> set to <see cref="IStorageItem.Name"/> and
		/// <c>option</c> left as the default value.
		/// </remarks>
		/// 
		/// <param name="fileToReplace">The file to replace.</param>
		/// 
		/// <returns>
		/// An awaitable <see cref="Task"/>, with no associated value.
		/// </returns>
		/// 
		/// <seealso cref="CopyAndReplaceAsync(IStorageFile)"/>
		/// <seealso cref="MoveAsync(IStorageFolder, string, NameCollisionOption)"/>
		Task MoveAndReplaceAsync(IStorageFile fileToReplace);

		/// <summary>
		/// Moves this file to the specified folder.
		/// </summary>
		/// 
		/// <remarks>
		/// The default value of <paramref name="option"/> was chosen for
		/// compatibility with the <c>Windows.Storage</c> implementation.
		/// </remarks>
		/// 
		/// <param name="destinationFolder">
		/// The new location of this file.
		/// </param>
		/// <param name="option">
		/// The method used to handle situations where a file or folder by
		/// this name already exists in <paramref name="destinationFolder"/>.
		/// <para />
		/// Default value is <see cref="NameCollisionOption.FailIfExists"/>.
		/// </param>
		/// 
		/// <returns>
		/// A handle to the new file, once the <see cref="Task{TResult}"/>
		/// completes.
		/// </returns>
		/// 
		/// <seealso cref="CopyAsync(IStorageFolder, NameCollisionOption)"/>
		Task<StorageFile> MoveAsync(IStorageFolder destinationFolder, NameCollisionOption option = NameCollisionOption.FailIfExists);
		/// <summary>
		/// Moves and renames this file to the specified folder and name.
		/// </summary>
		/// 
		/// <param name="destinationFolder">
		/// The new location of this file.
		/// </param>
		/// <param name="desiredName">
		/// The name under which the item will try to be saved. If a file of
		/// this name already exists in <paramref name="destinationFolder"/>,
		/// the behaviour is determined by <paramref name="option"/>.
		/// </param>
		/// <param name="option">
		/// The method used to handle situations where a file or folder by the
		/// name of <paramref name="desiredName"/> already exists in
		/// <paramref name="destinationFolder"/>.
		/// <para />
		/// Default value is
		/// <see cref="NameCollisionOption.GenerateUniqueName"/>.
		/// </param>
		/// 
		/// <returns>
		/// A handle to the new file, once the <see cref="Task{TResult}"/>
		/// completes.
		/// </returns>
		/// 
		/// <seealso cref="IStorageItem.Name"/>
		/// <seealso cref="CopyAsync(IStorageFolder, string, NameCollisionOption)"/>
		/// <seealso cref="IStorageItem.RenameAsync(string, NameCollisionOption)"/>
		/// <seealso cref="MoveAndReplaceAsync(IStorageFile)"/>
		Task<StorageFile> MoveAsync(
			IStorageFolder destinationFolder,
			string desiredName,
			NameCollisionOption option = NameCollisionOption.GenerateUniqueName
		);

		/// <summary>
		/// Opens a random-access stream over the file.
		/// </summary>
		/// 
		/// <param name="accessMode">The type of access to allow.</param>
		/// <param name="options">
		/// The allowed interactions between multiple streams reading from or
		/// writing to this file.
		/// </param>
		/// 
		/// <returns>
		/// A stream providing access to the data within the file, once the
		/// <see cref="Task{TResult}"/> completes.
		/// </returns>
		Task<FileStream> OpenAsync(
			FileAccessMode accessMode,
			StorageOpenOptions options = StorageOpenOptions.None
		);

		//TODO: Add OpenTransactedWriteAsync once we have added a concept of
		// transactions (overloads in both IStorageFile and IStorageFile2)
	}
}
