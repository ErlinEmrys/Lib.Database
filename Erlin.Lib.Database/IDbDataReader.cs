using System;
using System.Collections.Generic;
using System.Text;

namespace Erlin.Lib.Database
{
    /// <summary>
    /// Interface of databse data reader
    /// </summary>
    public interface IDbDataReader : IDisposable
    {
        /// <summary>
        /// Reads record from DB
        /// </summary>
        /// <typeparam name="T">Runtime type of record</typeparam>
        /// <returns>Readed record or null</returns>
        T? Read<T>()
            where T : class?, IDbReadable, new();

        /// <summary>
        /// Reads multiple records from DB
        /// </summary>
        /// <typeparam name="T">Runtime type of records</typeparam>
        /// <returns>Readed records or empty</returns>
        List<T> ReadList<T>()
            where T : class, IDbReadable, new();

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        int ReadInt32(string fieldName);

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        int? ReadInt32N(string fieldName);

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        string ReadString(string fieldName);

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        DateTime ReadDateTime(string fieldName);

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        DateTime? ReadDateTimeN(string fieldName);
    }
}