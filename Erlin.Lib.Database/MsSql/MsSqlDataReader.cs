using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

using Microsoft.Data.SqlClient;

namespace Erlin.Lib.Database.MsSql
{
    /// <summary>
    /// MS SQL data reader
    /// </summary>
    public class MsSqlDataReader : IDbDataReader
    {
        /// <summary>
        /// Underlying db reader
        /// </summary>
        public SqlDataReader UnderlyingReader { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="underlyingReader">Underlying db reader</param>
        public MsSqlDataReader(SqlDataReader underlyingReader)
        {
            UnderlyingReader = underlyingReader;
        }

        /// <summary>
        /// Reads record from DB
        /// </summary>
        /// <typeparam name="T">Runtime type of record</typeparam>
        /// <returns>Readed record or null</returns>
        public T? Read<T>()
            where T : class?, IDbReadable, new()
        {
            T? item = null;
            if (UnderlyingReader.Read())
            {
                item = new T();
                item.DbRead(this);
            }

            return item;
        }

        /// <summary>
        /// Reads multiple records from DB
        /// </summary>
        /// <typeparam name="T">Runtime type of records</typeparam>
        /// <returns>Readed records or empty</returns>
        public List<T> ReadList<T>()
            where T : class, IDbReadable, new()
        {
            List<T> result = new List<T>();
            T? item;
            do
            {
                item = Read<T>();
                if (item != null)
                {
                    result.Add(item);
                }
            } while (item != null);

            return result;
        }

        /// <summary>
        /// Reads string from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        public string ReadString(string fieldName)
        {
            return UnderlyingReader.GetString(fieldName);
        }

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        public int ReadInt32(string fieldName)
        {
            int? readed = ReadInt32N(fieldName);
            if (!readed.HasValue)
            {
                throw new NullReferenceException(fieldName);
            }

            return readed.Value;
        }

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        public int? ReadInt32N(string fieldName)
        {
            if (UnderlyingReader.IsDBNull(fieldName))
            {
                return null;
            }

            return UnderlyingReader.GetInt32(fieldName);
        }

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        public DateTime ReadDateTime(string fieldName)
        {
            DateTime? readed = ReadDateTimeN(fieldName);
            if (!readed.HasValue)
            {
                throw new NullReferenceException(fieldName);
            }
            return readed.Value;
        }

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        public DateTime? ReadDateTimeN(string fieldName)
        {
            if (UnderlyingReader.IsDBNull(fieldName))
            {
                return null;
            }

            return UnderlyingReader.GetDateTime(fieldName);
        }

        /// <summary>
        /// Release all resources
        /// </summary>
        public void Dispose()
        {
            UnderlyingReader.DisposeAsync();
        }
    }
}