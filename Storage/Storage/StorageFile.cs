using System;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using MimeDetective;
using MimeDetective.Extensions;

namespace AgEitilt.Common.Storage {
	/// <summary>
	/// Provides methods to describe and manipulate a concrete file and its
	/// contents.
	/// </summary>
	public class StorageFile : IStorageItem, IStorageItemPropertiesWithProvider, IStorageFile, IStorageFilePropertiesWithAvailability {
#if UWP
		/// <summary>
		/// A handle to the underlying file.
		/// </summary>
		Windows.Storage.StorageFile file = null;

		/// <summary>
		/// Ensure the file is in a valid state for access, throwing the
		/// proper exception if it's not.
		/// </summary>
		/// 
		/// <param name="openMode">
		/// Which type of access to determine the accessibility with, or
		/// <c>null</c> if <see cref="file"/> should only be checked for field
		/// presence (non-null).
		/// </param>
		/// 
		/// <returns>
		/// <see cref="file"/>, if it can be safely accessed.
		/// </returns>
		/// 
		/// <exception cref="FileNotFoundException">
		/// <paramref name="openMode"/> is not null and <see cref="file"/>
		/// doesn't exist on the file system, probably due to deletion.
		/// </exception>
		Windows.Storage.StorageFile CheckFile(FileAccess? openMode = null) {
			if (file == null)
				// Internal error, and so not declared
				throw new MissingFieldException(Resources.Strings.Exception_FileNull);

			return file;
		}
#else
		/// <summary>
		/// A handle to the underlying file.
		/// </summary>
		FileInfo file = null;

		/// <summary>
		/// Ensure the file is in a valid state for access, throwing the
		/// proper exception if it's not.
		/// </summary>
		/// 
		/// <param name="openMode">
		/// Which type of access to determine the accessibility with, or
		/// <c>null</c> if <see cref="file"/> should only be checked for field
		/// presence (non-null).
		/// </param>
		/// 
		/// <returns>
		/// <see cref="file"/>, if it can be safely accessed.
		/// </returns>
		/// 
		/// <exception cref="FileNotFoundException">
		/// <paramref name="openMode"/> is not null and <see cref="file"/>
		/// doesn't exist on the file system, probably due to deletion.
		/// </exception>
		FileInfo CheckFile(FileAccess? openMode = null) {
			if (file == null)
				// Internal error, and so not declared
				throw new MissingFieldException(Resources.Strings.Exception_FileNull);
			if ((openMode.HasValue) && (file.Exists == false)) {
				throw new FileNotFoundException(
					String.Format(Resources.Strings.Exception_FileNotFound, file.Name),
					file.FullName
				);
			}

			return file;
		}
#endif

		/// <summary>
		/// The previous file type detected.
		/// </summary>
		FileType mimeCache = null;
		/// <summary>
		/// The task detecting the file's MIME type, if one is in progress.
		/// </summary>
		Task<FileType> readingMime = null;
		/// <summary>
		/// Asynchronously detect the file's MIME type from its header.
		/// </summary>
		/// 
		/// <param name="useCached">
		/// If <c>false</c>, recheck the type of the file; otherwise use the
		/// last type detected.
		/// </param>
		/// 
		/// <returns>A task that completes once the type is read.</returns>
		/// 
		/// <exception cref="DirectoryNotFoundException">
		/// The enclosing directory no longer exists, and can't use the cached
		/// type.
		/// </exception>
		/// <exception cref="FileNotFoundException">
		/// <see cref="file"/> doesn't exist on the file system, probably due
		/// to deletion, and can't use the cached type.
		/// </exception>
		/// <exception cref="UnauthorizedAccessException">
		/// The file is unable to be read or is a directory, and can't use the
		/// cached type.
		/// </exception>
		async Task<FileType> ReadMimeAsync(bool useCached = true) {
			if (useCached && (mimeCache != null)) {
				return mimeCache;
			} else if (readingMime == null) {
				//TODO: Make test-and-set atomic
#if UWP
				readingMime = await (await CheckFile(FileAccess.Read).OpenReadAsync()).GetFileTypeAsync(stream);
#else
				//TODO: Get around throwing IOException if already opened
				readingMime = CheckFile(FileAccess.Read).OpenRead().GetFileTypeAsync();
#endif

				mimeCache = await readingMime;
				// Reset flag to indicate work is completed
				readingMime = null;

				return mimeCache;
			} else {
				return await readingMime;
			}
		}

		/// <summary>
		/// Describes the properties of a storage item.
		/// </summary>
		/// 
		/// <value>The attributes of the file or folder.</value>
		/// 
		/// <exception cref="DirectoryNotFoundException">
		/// The enclosing directory no longer exists.
		/// </exception>
		/// <exception cref="UnauthorizedAccessException">
		/// The file is unable to be read, or is a directory.
		/// </exception>
		public FileAttributes Attributes {
			get {
				try {
					//TODO: Cut down on the thrown exceptions
					return CheckFile().Attributes;
				} catch (SecurityException e) {
					// Rewrap to better follow other signatures
					throw new UnauthorizedAccessException(e.Message, e);
				}
			}
		}

		/// <summary>
		/// Get the MIME type of the file contents.
		/// </summary>
		/// 
		/// <remarks>
		/// If this is accessed shortly after the <see cref="StorageFile"/> is
		/// created, this may be a long operation as this is being read from
		/// the file on disk. It is saved after the first processing, however,
		/// so later accesses are only slightly more than a direct field
		/// access.
		/// </remarks>
		/// 
		/// <value>
		/// The type in standard MIME format; for example, <c>audio/mpeg</c>.
		/// </value>
		/// 
		/// <exception cref="DirectoryNotFoundException">
		/// The enclosing directory no longer exists, and can't use the cached
		/// type.
		/// </exception>
		/// <exception cref="FileNotFoundException">
		/// <see cref="file"/> not found, probably due to deletion, and can't
		/// use the cached type.
		/// </exception>
		/// <exception cref="UnauthorizedAccessException">
		/// The file is unable to be read or is a directory, and can't use the
		/// cached type.
		/// </exception>
		/// 
		/// <seealso cref="DisplayType"/>
		public string ContentType =>
			ReadMimeAsync().Result.Mime;

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
		/// 
		/// <exception cref="DirectoryNotFoundException">
		/// The enclosing directory no longer exists.
		/// </exception>
		/// <exception cref="FileNotFoundException">
		/// <see cref="file"/> doesn't exist on the file system, probably due
		/// to deletion.
		/// </exception>
		public DateTimeOffset? DateCreated {
			get {
				try {
#if UWP
					return CheckFile().DateCreated;
#else
					return new DateTimeOffset(CheckFile().CreationTimeUtc);
#endif
				} catch (DirectoryNotFoundException e) {
					// Want to only handle IOExceptions below, so need to
					// include this case to not be overzealous
					throw e;
				} catch (IOException e) {
					/* Potentially not always as correct, but makes external
					 * exception handling simpler, and the documentation
					 * implies it's the rough semantics.
					 */
					throw new FileNotFoundException(e.Message, e);
				} catch (PlatformNotSupportedException) {
					//TODO: Try to get this in other manners for
					// non-Windows NT platforms
					return null;
				}
			}
		}

		/// <summary>
		/// Gets a user-friendly name for the file.
		/// </summary>
		/// 
		/// <remarks>
		/// This will typically be <see cref="Name"/> stripped of
		/// the last file extension.
		/// </remarks>
		/// 
		/// <value>
		/// The user-friendly name.
		/// </value>
		public string DisplayName =>
			Name.Substring(0, Name.Length - (FileType?.Length ?? 0));

		/// <summary>
		/// Gets a user-friendly description of the item's content.
		/// </summary>
		/// 
		/// <remarks>
		/// For example, an image file might return "JPG image".
		/// </remarks>
		/// 
		/// <value>
		/// The user-friendly type of content.
		/// </value>
		/// 
		/// <exception cref="DirectoryNotFoundException">
		/// The enclosing directory no longer exists, and can't use the cached
		/// type.
		/// </exception>
		/// <exception cref="FileNotFoundException">
		/// <see cref="file"/> doesn't exist on the file system, probably due
		/// to deletion, and can't use the cached type.
		/// </exception>
		/// <exception cref="UnauthorizedAccessException">
		/// The file is unable to be read or is a directory, and can't use the
		/// cached type.
		/// </exception>
		/// 
		/// <seealso cref="ContentType"/>
		public string DisplayType =>
			//TODO: Actually parse this into readable resources
			ContentType;

		/// <summary>
		/// Gets the extension of the file .
		/// </summary>
		/// 
		/// <remarks>
		/// If the file has multiple extensions, only the last is returned.
		/// <para/>
		/// Do not rely on this to determine the file type; while it is the
		/// primary indication of type on Windows systems, on other platforms
		/// it may only provide a hint (for example, standard plaintext files
		/// on Linux are typically not given any extension at all, but
		/// <c>text.jpg</c> would be just as valid). If code depends on the
		/// type of a file, <see cref="ContentType"/> should be used instead.
		/// <para/>
		/// TODO: Generate default extension based on MIME type, if it's not
		/// included?
		/// </remarks>
		/// 
		/// <value>
		/// The file extension; for example, <c>.jpg</c>, or null if this is
		/// not included in the file name.
		/// </value>
		public string FileType {
			get {
#if UWP
				var ext = CheckFile().FileType;
#else
				var ext = CheckFile().Extension;
#endif
				return (ext.Length == 0 ? ext : null);
			}
		}

		/// <summary>
		/// Indicates whether the file is located in an accessible location.
		/// </summary>
		/// 
		/// <remarks>TODO: Make this more robust.</remarks>
		/// 
		/// <value>
		/// <c>true</c> if the file is local (and all parent folders can be
		/// read by this process), if it is cached locally, or if it can be
		/// downloaded; otherwise, <c>false</c>.
		/// </value>
		/// 
		/// <exception cref="FileNotFoundException">
		/// <see cref="file"/> doesn't exist on the file system, probably due
		/// to deletion, and can't use the cached type.
		/// </exception>
		public bool IsAvailable {
			get {
				try {
					CheckFile(FileAccess.Read);
				} catch (FileNotFoundException) {
					return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Gets the name of the storage item, including (if it exists) the
		/// file extension.
		/// </summary>
		/// 
		/// <value>The full name of the file or folder.</value>
		/// 
		/// <seealso cref="RenameAsync(string, NameCollisionOption)"/>
		public string Name =>
			CheckFile().Name;

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
		/// 
		/// <exception cref="PathTooLongException">
		/// The file path is longer than allowed by the system.
		/// </exception>
		/// <exception cref="UnauthorizedAccessException">
		/// The file is unable to be read, or is a directory.
		/// </exception>
		public string Path {
			get {
				try {
#if UWP
					return CheckFile().Path;
#else
					return CheckFile().FullName;
#endif
				} catch (SecurityException e) {
					// Rewrap to better follow other signatures
					throw new UnauthorizedAccessException(e.Message, e);
				}
			}
		}

		/// <summary>
		/// Gets a description of the properties of the contained content.
		/// </summary>
		/// 
		/// <value>An object listing such properties.</value>
		public StorageItemContentProperties Properties => throw new NotImplementedException();

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
		public Task CopyAndReplaceAsync(IStorageFile fileToReplace) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates a copy of this file in the specified folder.
		/// </summary>
		/// 
		/// <remarks>
		/// The default value of <paramref name="option"/> was chosen for
		/// compatibility with the <c>Windows.Storage</c> implementation.
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
		public Task<StorageFile> CopyAsync(
				IStorageFolder destinationFolder,
				NameCollisionOption option = NameCollisionOption.FailIfExists
			) => CopyAsync(destinationFolder, Name, option);
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
		/// <seealso cref="Name"/>
		/// <seealso cref="MoveAsync(IStorageFolder, string, NameCollisionOption)"/>
		/// <seealso cref="CopyAndReplaceAsync(IStorageFile)"/>
		public Task<StorageFile> CopyAsync(
				IStorageFolder destinationFolder,
				string desiredName,
				NameCollisionOption option = NameCollisionOption.GenerateUniqueName
			) {
			throw new NotImplementedException();
		}

		//TODO: Implement CreateStreamedFileAsync and
		// CreateStreamedFileFromUriAsync once stream handling is more mature

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
		public Task DeleteAsync(StorageDeleteOption option = StorageDeleteOption.Default) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the basic properties of the current item.
		/// </summary>
		/// 
		/// <returns>
		/// The basic properties describing the item, once the
		/// <see cref="Task{TResult}"/> completes.
		/// </returns>
		public Task<BasicProperties> GetBasicPropertiesAsync() {
			throw new NotImplementedException();
		}

		//TODO: Implement GetFileFromApplicationUriAsync once we figure out
		// how to handle application (as opposed to system) resources

		/// <summary>
		/// Gets an instance of <see cref="StorageFile"/> representing the
		/// specified file.
		/// </summary>
		/// 
		/// <remarks>
		/// Unlike the <c>Windows.Storage</c> implementation,
		/// <paramref name="path"/> will correctly handle either <c>\</c> or
		/// <c>/</c> as folder separators.
		/// </remarks>
		/// 
		/// <param name="path">The absolute path of the file.</param>
		/// 
		/// <returns>
		/// A handle to the file, once the <see cref="Task{TResult}"/>
		/// completes.
		/// </returns>
		public static Task<StorageFile> GetFileFromPathAsync(String path) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the folder containing this file.
		/// </summary>
		/// 
		/// <returns>
		/// A handle to the folder, once the <see cref="Task{TResult}"/>
		/// completes.
		/// </returns>
		public Task<StorageFolder> GetParentAsync() {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Indicates whether this storage item and another refer to the same
		/// file, and access it via the same path.
		/// </summary>
		/// 
		/// <param name="item">The storage item to compare against.</param>
		/// 
		/// <returns>
		/// <c>true</c> if the this file is equal to <paramref name="item"/>;
		/// otherwise <c>false</c>.
		/// </returns>
		public bool IsEqual(IStorageItem item) {
			throw new NotImplementedException();
		}

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
		public bool IsOfType(StorageItemTypes type) =>
			type == StorageItemTypes.File;

		/// <summary>
		/// Replaces the specified file with this file, removing the latter
		/// from its current location.
		/// </summary>
		/// 
		/// <remarks>
		/// This has the same result as calling
		/// <see cref="MoveAsync(IStorageFolder, string, NameCollisionOption)"/>
		/// with <c>desiredName</c> set to <see cref="Name"/> and
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
		public Task MoveAndReplaceAsync(IStorageFile fileToReplace) {
			throw new NotImplementedException();
		}

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
		public Task<StorageFile> MoveAsync(
				IStorageFolder destinationFolder,
				NameCollisionOption option = NameCollisionOption.FailIfExists
			) => MoveAsync(destinationFolder, Name, option);
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
		/// <seealso cref="Name"/>
		/// <seealso cref="CopyAsync(IStorageFolder, string, NameCollisionOption)"/>
		/// <seealso cref="RenameAsync(string, NameCollisionOption)"/>
		/// <seealso cref="MoveAndReplaceAsync(IStorageFile)"/>
		public Task<StorageFile> MoveAsync(
				IStorageFolder destinationFolder, string desiredName,
				NameCollisionOption option = NameCollisionOption.GenerateUniqueName
			) {
			throw new NotImplementedException();
		}

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
		public Task<FileStream> OpenAsync(
				FileAccessMode accessMode,
				StorageOpenOptions options = StorageOpenOptions.None
			) {
			throw new NotImplementedException();
		}

		//TODO: Implement OpenReadAsync and OpenSequentialReadAsync once
		// stream support is added

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
		public Task RenameAsync(
				string desiredName,
				NameCollisionOption option = NameCollisionOption.FailIfExists
			) {
			throw new NotImplementedException();
		}

		//TODO: Implement ReplaceWithStreamedFileAsync and
		// ReplaceWithStreamedFileFromUriAsync once stream handling is more
		// mature
	}
}