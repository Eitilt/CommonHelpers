using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgEitilt.Common.Storage {
	/// <summary>
	/// Manipulates folders and their contents, and provides information about
	/// both.
	/// </summary>
	public interface IStorageFolder : IStorageItem {
		/// <summary>
		/// Creates a new file in this folder.
		/// </summary>
		/// 
		/// <remarks>
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a <see cref="Task{TResult}"/> rather than the modern
		/// <c>IAsyncOperation&lt;...&gt;</c>.
		/// </remarks>
		/// 
		/// <param name="desiredName">
		/// The desired name of the file to create.
		/// </param>
		/// <param name="option">
		/// The method used to handle situations where a file or folder by
		/// the name of <paramref name="desiredName"/> already exists in this
		/// folder.
		/// <para />
		/// Default value is
		/// <see cref="CreationCollisionOption.FailIfExists"/>.
		/// </param>
		/// 
		/// <returns>
		/// A handle to the new file, once the <see cref="Task{TResult}"/>
		/// completes.
		/// </returns>
		/// 
		/// <seealso cref="CreateFolderAsync(string, CreationCollisionOption)"/>
		/// <seealso cref="IStorageItem.Name"/>
		Task<StorageFile> CreateFileAsync(string desiredName, CreationCollisionOption option = CreationCollisionOption.FailIfExists);

		/// <summary>
		/// Creates a new folder nested within this folder.
		/// </summary>
		/// 
		/// <remarks>
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a <see cref="Task{TResult}"/> rather than the modern
		/// <c>IAsyncOperation&lt;...&gt;</c>.
		/// </remarks>
		/// 
		/// <param name="desiredName">
		/// The desired name of the folder to create.
		/// </param>
		/// <param name="option">
		/// The method used to handle situations where a file or folder by
		/// the name of <paramref name="desiredName"/> already exists in this
		/// folder.
		/// <para />
		/// Default value is
		/// <see cref="CreationCollisionOption.FailIfExists"/>.
		/// </param>
		/// 
		/// <returns>
		/// A handle to the new folder, once the <see cref="Task{TResult}"/>
		/// completes.
		/// </returns>
		/// 
		/// <seealso cref="CreateFileAsync(string, CreationCollisionOption)"/>
		/// <seealso cref="IStorageItem.Name"/>
		Task<StorageFolder> CreateFolderAsync(string desiredName, CreationCollisionOption option = CreationCollisionOption.FailIfExists);

		/// <summary>
		/// Gets the specified file using this folder as the base of the
		/// relative path.
		/// </summary>
		/// 
		/// <remarks>
		/// If the file is located within this folder, <paramref name="path"/>
		/// may simply be the <see cref="IStorageItem.Name"/>.
		/// <para/>
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a <see cref="Task{TResult}"/> rather than the modern
		/// <c>IAsyncOperation&lt;...&gt;</c>.
		/// </remarks>
		/// 
		/// <param name="path">The path relative to this folder.</param>
		/// 
		/// <returns>
		/// A handle to the file, once the <see cref="Task{TResult}"/>
		/// completes.
		/// </returns>
		/// 
		/// <seealso cref="GetFilesAsync"/>
		/// <seealso cref="GetItemAsync(string)"/>
		Task<StorageFile> GetFileAsync(string path);

		/// <summary>
		/// Gets all files contained in this folder.
		/// </summary>
		/// 
		/// <remarks>
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a <see cref="Task{TResult}"/> rather than the modern
		/// <c>IAsyncOperation&lt;...&gt;</c>.
		/// </remarks>
		/// 
		/// <returns>
		/// A list of handles to all such files, once the
		/// <see cref="Task{TResult}"/> completes.
		/// </returns>
		/// 
		/// <seealso cref="GetFileAsync(string)"/>
		/// <seealso cref="GetItemsAsync"/>
		Task<IReadOnlyList<StorageFile>> GetFilesAsync();

		/// <summary>
		/// Gets the specified folder using this one as the base of the
		/// relative path.
		/// </summary>
		/// 
		/// <remarks>
		/// If the folder is located within this one, <paramref name="path"/>
		/// may simply be the <see cref="IStorageItem.Name"/>.
		/// <para/>
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a <see cref="Task{TResult}"/> rather than the modern
		/// <c>IAsyncOperation&lt;...&gt;</c>.
		/// </remarks>
		/// 
		/// <param name="path">The path relative to this folder.</param>
		/// 
		/// <returns>
		/// A handle to the folder, once the <see cref="Task{TResult}"/>
		/// completes.
		/// </returns>
		/// 
		/// <seealso cref="GetFoldersAsync"/>
		/// <seealso cref="GetItemAsync(string)"/>
		Task<StorageFolder> GetFolderAsync(string path);

		/// <summary>
		/// Gets all folders nested within this one.
		/// </summary>
		/// 
		/// <remarks>
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a <see cref="Task{TResult}"/> rather than the modern
		/// <c>IAsyncOperation&lt;...&gt;</c>.
		/// </remarks>
		/// 
		/// <returns>
		/// A list of handles to all such folders, once the
		/// <see cref="Task{TResult}"/> completes.
		/// </returns>
		/// 
		/// <seealso cref="GetFolderAsync(string)"/>
		/// <seealso cref="GetItemsAsync"/>
		Task<IReadOnlyList<StorageFile>> GetFoldersAsync();

		/// <summary>
		/// Gets the specified storage item using this folder as the base of
		/// the relative path.
		/// </summary>
		/// 
		/// <remarks>
		/// If the item is located within this folder, <paramref name="path"/>
		/// may simply be the <see cref="IStorageItem.Name"/>.
		/// <para/>
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a <see cref="Task{TResult}"/> rather than the modern
		/// <c>IAsyncOperation&lt;...&gt;</c>.
		/// </remarks>
		/// 
		/// <param name="path">The path relative to this folder.</param>
		/// 
		/// <returns>
		/// A handle to the storage item, once the <see cref="Task{TResult}"/>
		/// completes.
		/// </returns>
		/// 
		/// <seealso cref="GetItemsAsync"/>
		/// <seealso cref="IStorageFolder2.TryGetItemAsync(string)"/>
		/// <seealso cref="GetFileAsync(string)"/>
		/// <seealso cref="GetFolderAsync(string)"/>
		Task<IStorageItem> GetItemAsync(string path);

		/// <summary>
		/// Gets all storage items contained in this folder.
		/// </summary>
		/// 
		/// <remarks>
		/// Warning: This is not fully compatible with <c>Windows.Storage</c>
		/// as it returns a <see cref="Task{TResult}"/> rather than the modern
		/// <c>IAsyncOperation&lt;...&gt;</c>.
		/// </remarks>
		/// 
		/// <returns>
		/// A list of handles to all such items, once the
		/// <see cref="Task{TResult}"/> completes.
		/// </returns>
		/// 
		/// <seealso cref="GetItemAsync(string)"/>
		/// <seealso cref="GetFilesAsync"/>
		/// <seealso cref="GetFoldersAsync"/>
		Task<IReadOnlyList<IStorageItem>> GetItemsAsync();
	}
}