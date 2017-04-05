namespace AgEitilt.Common.Storage {
	/// <summary>
	/// Specifies what to do if an with the specified name already exists in
	/// the current folder when you create a new file or folder.
	/// </summary>
	public enum CreationCollisionOption {
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
		/// Return the existing item.
		/// </summary>
		OpenIfExists,
		/// <summary>
		/// Replace the existing item.
		/// </summary>
		ReplaceExisting
	}
}