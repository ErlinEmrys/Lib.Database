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
        /// Advances the data reader to the next result, when reading the results of batch Transact-SQL statements.
        /// </summary>
        /// <returns><see langword="true" /> if there are more result sets; otherwise <see langword="false" />.</returns>
        bool NextResult();

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        byte ReadByte(string fieldName);

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        byte? ReadByteN(string fieldName);

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        short ReadInt16(string fieldName);

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        short? ReadInt16N(string fieldName);

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
        bool ReadBool(string fieldName);

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        bool? ReadBoolN(string fieldName);
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
        string? ReadStringN(string fieldName);

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