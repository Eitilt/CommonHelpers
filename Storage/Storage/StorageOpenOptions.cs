namespace AgEitilt.Common.Storage {
	/// <summary>
	/// Specify the relative permissions if a file is accessed by both input
	/// and output streams (both read and write).
	/// </summary>
	public enum StorageOpenOptions {
		/// <summary>
		/// The default behaviour: readers will have lower priority than any
		/// writers, and will fail if writes occur.
		/// </summary>
		None,
		/// <summary>
		/// No writers are allowed on the file, and attempting to open a new
		/// writer will fail; likewise, opening a reader with this option will
		/// fail if a writer is already open.
		/// </summary>
		AllowOnlyReaders,
		/// <summary>
		/// A writer with the same priority as a writer, and so is not
		/// invalidated by other streams writing to the same file.
		/// </summary>
		/// 
		/// <remarks>
		/// The documentation is unclear on the behaviour; this is my best
		/// guess at its meaning.
		/// </remarks>
		AllowReadersAndWriters
	}
}