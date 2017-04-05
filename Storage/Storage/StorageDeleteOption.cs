namespace AgEitilt.Common.Storage {
	/// <summary>
	/// Specifies whether a deleted item is moved to the Recycle Bin (or the
	/// platform equivalent) or permanently deleted.
	/// </summary>
	public enum StorageDeleteOption {
		/// <summary>
		/// Use the default behaviour.
		/// </summary>
		/// 
		/// <remarks>
		/// TODO: Describe the decisions made to determine behaviour.
		/// For reference, on Windows 10:
		/// <list type="bullet">
		/// <item>
		/// If the item is in application storage (accessed through
		/// <c>ApplicationData</c>), then permanently deleted
		/// </item>
		/// <item>
		/// Delete according to default behaviour of File Explorer for the
		/// particular save location
		/// </item>
		/// </list>
		/// </remarks>
		Default,
		/// <summary>
		/// Permanently deletes the item. The item is not moved to the
		/// Recycle Bin.
		/// </summary>
		PermanentDelete
	}
}