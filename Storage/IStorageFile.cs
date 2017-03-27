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
		/// <remarks>
		/// Warning: This is likely not fully compatible with
		/// <c>Windows.Storage</c>; while the documentation there does not
		/// describe what value is returned if the file has no extension, it
		/// is likely an empty string. <c>null</c> was chosen here for clearer
		/// semantics.
		/// </remarks>
		/// 
		/// <value>
		/// The file extension; for example, <c>.jpg</c>, or null if this is
		/// not included in the file name.
		/// </value>
		string FileType { get; }

		/// <summary>
		/// Replaces the specified file with a copy of this file.
		/// </summary>
		/// 
		/// <remarks>
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a simple <see cref="Task"/> rather than the modern
		/// <c>IAsyncAction</c>.
		/// </remarks>
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
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a <see cref="Task{TResult}"/> rather than the modern
		/// <c>IAsyncOperation&lt;...&gt;</c>.
		/// <para />
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
		/// <remarks>
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a <see cref="Task{TResult}"/> rather than the modern
		/// <c>IAsyncOperation&lt;...&gt;</c>.
		/// </remarks>
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
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a simple <see cref="Task"/> rather than the modern
		/// <c>IAsyncAction</c>.
		/// <para />
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
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a <see cref="Task{TResult}"/> rather than the modern
		/// <c>IAsyncOperation&lt;...&gt;</c>.
		/// <para />
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
		/// <remarks>
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a <see cref="Task{TResult}"/> rather than the modern
		/// <c>IAsyncOperation&lt;...&gt;</c>.
		/// </remarks>
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
		/// <param name="accessMode">The type of access to allow.</param>
		/// 
		/// <returns>
		/// A stream providing access to the data within the file, once the
		/// <see cref="Task{TResult}"/> completes.
		/// </returns>
		/// 
		/// <seealso cref="IStorageFile2.OpenAsync(FileAccessMode, StorageOpenOptions)"/>
		Task<FileStream> OpenAsync(FileAccessMode accessMode);

		//TODO: Add OpenTransactedWriteAsync once we have added a concept of
		// transactions
	}
}
