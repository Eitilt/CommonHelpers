namespace AgEitilt.Common.Storage {
	/// <summary>
	/// Provides access to the file's availability.
	/// </summary>
	public interface IStorageFilePropertiesWithAvailability {
		/// <summary>
		/// Indicates whether the file is located in an accessible location.
		/// </summary>
		/// 
		/// <value>
		/// <c>true</c> if the file is local (and all parent folders can be
		/// read by this process), if it is cached locally, or if it can be
		/// downloaded; otherwise, <c>false</c>.
		/// </value>
		bool IsAvailable { get; }
	}
}
