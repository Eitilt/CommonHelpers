namespace AgEitilt.Common.Storage {
	/// <summary>
	/// Specifies what to do if a file or folder with the desired name already
	/// exists in the destination when you copy, move, or rename an item.
	/// </summary>
	public enum NameCollisionOption {
		/// <summary>
		/// Throw an exception.
		/// </summary>
		/// 
		/// <remarks>
		/// <c>Windows.Storage</c> specifies that the exception will be a
		/// <see cref="System.Exception"/>, but we can be more specific in the
		/// exception type.
		/// </remarks>
		FailIfExists,
		/// <summary>
		/// Automatically append a number to the base name of the new file or
		/// folder.
		/// </summary>
		/// 
		/// <remarks>
		/// For example, if the folder <c>MyFolder (2)</c> already exists, the
		/// resulting folder will be <c>MyFolder (3)</c>, and if the file
		/// <c>MyFile.xaml.cs</c> does, the new one will be named
		/// <c>MyFile (2).xaml.cs</c>.
		/// </remarks>
		GenerateUniqueName,
		/// <summary>
		/// Replace the existing item. This will delete any contents if the
		/// existing item is a folder.
		/// </summary>
		ReplaceExisting,
		/// <summary>
		/// If both the existing item and the one being moved or copied are
		/// folders, merge the children of both (replacing any files that
		/// share the same name). Otherwise, this functions identically to
		/// <see cref="ReplaceExisting"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// This is an extension on the <c>Windows.System</c> enum.
		/// </remarks>
		ReplaceOrMerge,
		/// <summary>
		/// If both the existing item and the one being moved or copied are
		/// folders, merge the children of both (generating new names for any
		/// files that share the same name according to the rules described
		/// for <see cref="GenerateUniqueName"/>). Otherwise, this functions
		/// identically to <see cref="ReplaceExisting"/>.
		/// </summary>
		/// 
		/// <remarks>
		/// This is an extension on the <c>Windows.System</c> enum.
		/// </remarks>
		MergeAlongside
	}
}