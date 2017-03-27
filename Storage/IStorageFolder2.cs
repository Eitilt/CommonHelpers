using System;
using System.Threading.Tasks;

namespace AgEitilt.Common.Storage {
	/// <summary>
	/// Provides additional methods for manipulating folders and their
	/// contents.
	/// </summary>
	public interface IStorageFolder2 : IStorageFolder {
		/// <summary>
		/// Try to get a single storage item from the current folder by using
		/// the name of the item.
		/// </summary>
		/// 
		/// <param name="name">
		/// The name (or path relative to the current folder) of the storage
		/// item to try to retrieve.
		/// </param>
		/// 
		/// <returns>
		/// A handle to the requested item, or <c>null</c> if it does not
		/// exist, once the task is complete.
		/// </returns>
		/// 
		/// <seealso cref="IStorageFolder.GetItemAsync(string)"/>
		Task<IStorageItem> TryGetItemAsync(String name);
	}
}
