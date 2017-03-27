using System;
using System.IO;
using System.Threading.Tasks;

namespace AgEitilt.Common.Storage {
	/// <summary>
	/// Manipulates storage items (files and folders) and their contents,
	/// and provides information about them.
	/// </summary>
	/// 
	/// <seealso cref="StorageFile"/>
	/// <seealso cref="StorageFolder"/>
	public interface IStorageItem {
		/// <summary>
		/// Describes the properties of a storage item.
		/// </summary>
		/// 
		/// <value>The attributes of the file or folder.</value>
		FileAttributes Attributes { get; }

		/// <summary>
		/// Gets the date and time the current item was created.
		/// </summary>
		/// 
		/// <remarks>
		/// For the date and time of the last edit, see
		/// <see cref="GetBasicPropertiesAsync"/>.
		/// </remarks>
		/// 
		/// <value>
		/// The date and time the current item was created, or <c>null</c> if
		/// this information is not included on the file; for example (in
		/// string format), "Fri Sep 16 13:47:08 PDT 2011".
		/// </value>
		DateTimeOffset? DateCreated { get; }

		/// <summary>
		/// Gets the name of the storage item, including (if it exists) the
		/// file extension.
		/// </summary>
		/// 
		/// <value>The full name of the file or folder.</value>
		/// 
		/// <seealso cref="RenameAsync(string, NameCollisionOption)"/>
		string Name { get; }

		/// <summary>
		/// The full path of the file on the file system, if it is located in
		/// such an accessible place.
		/// </summary>
		/// 
		/// <remarks>
		/// Do not rely on this property to access an item because some items
		/// may not have file-system paths. For example, if the item is backed
		/// by a URI, or was picked using the file picker.
		/// </remarks>
		/// 
		/// <value>
		/// The full path of the item (including its name), or an empty string
		/// if it is not physically identifiable on the file system.
		/// </value>
		string Path { get; }

		/// <summary>
		/// Removes the current item from disk.
		/// </summary>
		/// 
		/// <param name="option">
		/// Whether to always skip the Recycle Bin and permanently delete the
		/// item, or if that decision should rely on the default behaviour.
		/// <para/>
		/// Default value is the latter
		/// (<see cref="StorageDeleteOption.Default"/>).
		/// </param>
		/// 
		/// <returns>
		/// An awaitable <see cref="Task"/>, with no associated value.
		/// </returns>
		Task DeleteAsync(StorageDeleteOption option = StorageDeleteOption.Default);

		/// <summary>
		/// Gets the basic properties of the current item.
		/// </summary>
		/// 
		/// <returns>
		/// The basic properties describing the item, once the
		/// <see cref="Task{TResult}"/> completes.
		/// </returns>
		Task<BasicProperties> GetBasicPropertiesAsync();

		/// <summary>
		/// Determines whether the current item is of the specified type.
		/// </summary>
		/// 
		/// <param name="type">The value to match against.</param>
		/// 
		/// <returns>
		/// <c>true</c> if the item is <paramref name="type"/>, otherwise
		/// <c>false</c>.
		/// </returns>
		bool IsOfType(StorageItemTypes type);

		/// <summary>
		/// Renames the current item.
		/// </summary>
		/// 
		/// <param name="desiredName">
		/// The name under which the item will try to be saved. If a file of
		/// this name already exists in this folder, the behaviour is
		/// determined by <paramref name="option"/>.
		/// </param>
		/// <param name="option">
		/// The method used to handle situations where a file by the name of
		/// <paramref name="desiredName"/> already exists in this folder.
		/// <para />
		/// Default value is <see cref="NameCollisionOption.FailIfExists"/>.
		/// </param>
		/// 
		/// <returns>
		/// An awaitable <see cref="Task"/>, with no associated value.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// <paramref name="desiredName"/> is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// A file by the name of <paramref name="desiredName"/> already
		/// exists and <paramref name="option"/> is
		/// <see cref="NameCollisionOption.FailIfExists"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="desiredName"/> (as appended to the folder path) is
		/// longer than allowed by the file system.
		/// </exception>
		/// <exception cref="FormatException">
		/// <paramref name="desiredName"/> is not a valid name for the file
		/// system.
		/// </exception>
		/// <exception cref="FileNotFoundException">
		/// This file is deleted before the returned <see cref="Task"/>
		/// finishes.
		/// </exception>
		/// 
		/// <seealso cref="Name"/>
		Task RenameAsync(String desiredName, NameCollisionOption option = NameCollisionOption.FailIfExists);
	}
}