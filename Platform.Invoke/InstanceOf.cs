namespace Platform.Invoke
{
    /// <summary>
    /// Provides a standard implementation of the interface using default values and attributes.
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    public static class Implementation<TInterface>
        where TInterface : class
    {
        private static readonly TInterface instance;

        static Implementation()
        {
            instance = LibraryInterfaceFactory.Implement<TInterface>();
        }

        /// <summary>
        /// Gets an implementation of TInterface using default parameters.
        /// </summary>
        public static TInterface Instance { get { return instance; }  }
    }
}
