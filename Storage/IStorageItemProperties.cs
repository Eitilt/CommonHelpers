namespace AgEitilt.Common.Storage {
	interface IStorageItemProperties {
		/// <summary>
		/// Gets a user-friendly name for the file.
		/// </summary>
		/// 
		/// <remarks>
		/// This will typically be <see cref="IStorageItem.Name"/> stripped of
		/// any file extension.
		/// </remarks>
		/// 
		/// <value>
		/// The user-friendly name.
		/// </value>
		string DisplayName { get; }

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
		string DisplayType { get; }

		//TODO: Implement FolderRelativeID if this implementation of
		// StorageFolder allows a means of determining this

		/// <summary>
		/// Gets a description of the properties of the contained content.
		/// </summary>
		/// 
		/// <value>An object listing such properties.</value>
		StorageItemContentProperties Properties { get; }

		//TODO: Implement GetThumbnailAsync once thumbnail support is added
	}
}
