namespace Erlin.Lib.Database
{
    /// <summary>
    /// Types of database object
    /// </summary>
    public enum DbObjectType
    {
        /// <summary>
        /// Unknow type or error
        /// </summary>
        Error = 0,
        /// <summary>
        /// Complete database catalog object
        /// </summary>
        DatabaseCatalog = 1,
        /// <summary>
        /// User defined table
        /// </summary>
        Table = 2,
        /// <summary>
        /// Function
        /// </summary>
        Function = 3,
        /// <summary>
        /// Stored procedure
        /// </summary>
        StoredProcedure = 4,
        /// <summary>
        /// Input/output parameter
        /// </summary>
        Parameter = 5,
        /// <summary>
        /// Table trigger
        /// </summary>
        Trigger = 6,
        /// <summary>
        /// Runtime database data type
        /// </summary>
        DatabaseType = 7,
        /// <summary>
        /// View
        /// </summary>
        View = 8,
    }
}