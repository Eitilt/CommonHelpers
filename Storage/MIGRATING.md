# Considerations for migrating existing code
A core goal for this package is to provide a drop-in replacement for
the UWP-only `Windows.Storage` IO interactions. Even so, some changes
have been made to better integrate with `System` namespaces or to take
advantage of .NET-specific features that weren't added to maintain
compatibility with, for example, C++. This document lists any such
known differences that aren't backwards-compatible, and which may
require the including code to be edited.

## IStorageFile
See also `IStorageItem`
- Async methods return `Task` or `Task<TResult>` objects rather than
  `IAsyncAction` or `IAsyncResult<TResult>`; this affects:
  - **`CopyAndReplaceAsync`**
  - **`CopyAsync`** (all overrides)
  - **`MoveAndReplaceAsync`**
  - **`MoveAsync`** (all overrides)
  - **`OpenAsync`**
- **`FileType`** returns `null` if no extension can be retrieved from
  the file name
  - This may actually be the original behaviour, but the documentation
    doesn't make it clear
- **`OpenAsync`** returns `System.IO.FileStream` rather than an
  implementation of `Windows.Storage.IRandomAccessStream`
- **`OpenTransactedWriteAsync`** is not yet implemented

## IStorageFile2
See also `IStorageFile`
- Async methods return `Task` or `Task<TResult>` objects rather than
  `IAsyncAction` or `IAsyncResult<TResult>`; this affects:
  - **`OpenAsync`**
- **`OpenAsync`** returns `System.IO.FileStream` rather than an
  implementation of `Windows.Storage.IRandomAccessStream`
- **`OpenTransactedWriteAsync`** is not yet implemented

## IStorageFolder
See also `IStorageItem`
- Async methods return `Task` or `Task<TResult>` objects rather than
  `IAsyncAction` or `IAsyncResult<TResult>`; this affects:
  - **`CreateFileAsync`**
  - **`CreateFolderAsync`**
  - **`GetFileAsync`**
  - **`GetFilesAsync`**
  - **`GetFolderAsync`**
  - **`GetFoldersAsync`**
  - **`GetItemAsync`**
  - **`GetItemsAsync`**

## IStorageItem
- Async methods return `Task` or `Task<TResult>` objects rather than
  `IAsyncAction` or `IAsyncResult<TResult>`; this affects:
  - **`DeleteAsync`**
  - **`GetBasicPropertiesAsync`**
  - **`RenameAsync`**
- **`Attributes`** returns `System.IO.FileAttributes` rather than
  `Windows.Storage.FileAttributes`
  - This has nearly all the same options (and more), *except for*
    lacking the option `LocallyIncomplete`
- **`DateCreated`** returns `null` if the file doesn't list its date of
  creation rather than a zeroed `DateTimeOffset`; the returned type is
  therefore now `Nullable<DateTimeOffset>`
- **`RenameAsync`** declares an exception for each of those described in
  the `Windows.Storage` documentation, but as that doesn't use the
  proper names for each exception, the types thrown may be different

## StorageFile
See also `IStorageFile`
- Many members still only throw a `NotImplementedException`
- Many members are still not even implemented to that extent, namely:
  - **`FolderRelativeID`**
  - **`CreateStreamedFileAsync`**
  - **`CreateStreamedFileFromUriAsync`**
  - **`GetFileFromApplicationUriAsync`**
  - **`GetScaledImageAsThumbnailAsync`**
  - **`GetThumbnailAsync`**
  - **`OpenReadAsync`**
  - **`OpenSequentialReadAsync`**
  - **`Provider`**
  - **`ReplaceWithStreamedFileAsync`**
  - **`ReplaceWithStreamedFileFromUriAsync`**
- Async methods return `Task` or `Task<TResult>` objects rather than
  `IAsyncAction` or `IAsyncResult<TResult>`; this affects:
  - **`GetFileFromPathAsync`**
  - **`GetParentAsync`**