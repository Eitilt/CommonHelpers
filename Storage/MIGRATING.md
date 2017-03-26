﻿# Considerations for migrating existing code
A core goal for this package is to provide a drop-in replacement for
the UWP-only `Windows.Storage` IO interactions. Even so, some changes
have been made to better integrate with `System` namespaces or where
the official implementation seems restrictive. This document lists any
such known differences that aren't backwards-compatible, and which may
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