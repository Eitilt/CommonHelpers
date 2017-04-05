namespace AgEitilt.Common.Storage {
	/// <summary>
	/// Describes whether a storage item is a file or a folder.
	/// </summary>
	/// 
	/// <remarks>
	/// This method is useful for processing the results from a method that
	/// returns <see cref="IStorageItem"/> instances that can be files or
	/// folders. To work with the returned items, call the
	/// <see cref="IStorageItem.IsOfType(StorageItemTypes)"/> method to
	/// determine whether each item is a file or a folder, then cast the item
	/// to the proper implementation.
	/// </remarks>
	public enum StorageItemTypes {
		/// <summary>
		/// A file that is represented as a <see cref="StorageFile"/>
		/// instance.
		/// </summary>
		File,
		/// <summary>
		/// A file that is represented as a <see cref="StorageFolder"/>
		/// instance.
		/// </summary>
		Folder,
		/// <summary>
		/// A storage item that is neither a file nor a folder.
		/// </summary>
		None
	}
}