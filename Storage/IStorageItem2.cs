using System.Threading.Tasks;

namespace AgEitilt.Common.Storage {
	/// <summary>
	/// Provides additional information about files and folders.
	/// </summary>
	public interface IStorageItem2 : IStorageItem {
		/// <summary>
		/// Gets the folder containing this file.
		/// </summary>
		/// 
		/// <returns>
		/// A handle to the folder, once the <see cref="Task{TResult}"/>
		/// completes.
		/// </returns>
		Task<StorageFolder> GetParentAsync();

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
		bool IsEqual(IStorageItem item);
	}
}
