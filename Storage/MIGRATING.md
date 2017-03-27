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

### IStorageFolder2
See also `IStorageFolder`
- Async methods return `Task` or `Task<TResult>` objects rather than
  `IAsyncAction` or `IAsyncResult<TResult>`; this affects:
  - **`TryGetItemAsync`**
- **`TryGetItemAsync`** returns `null` if the file is not found
  - This may actually be the original behaviour, but the documentation
    doesn't specify

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

## IStorageItem2
See also `IStorageItem`
- Async methods return `Task` or `Task<TResult>` objects rather than
  `IAsyncAction` or `IAsyncResult<TResult>`; this affects:
  - **`GetParentAsync`**

## IStorageItemProperties
- **`FolderRelativeID`** not yet implemented
- **`GetThumbnailAsync`** not yet implemented

## IStorageItemProperties2
See also `IStorageItemProperties`
- **`GetScaledImageAsThumbnailAsync`** not yet implemented

## IStorageItemPropertiesWithProvider
See also `IStorageItemProperties`
- **`Provider`** not yet implemented

## StorageOpenOption
- Most documentation in `Windows.Storage` is written as if these values
  may be composed (`None | AllowOnlyReaders`); this did not make much
  sense given the semantics, and has not been implemented.
- `AllowReadersAndWriters` documentation in `Windows.Storage` is
  unclear, and this implementation may not accurately reflect that behaviour.
