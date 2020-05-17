using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Npgsql;

namespace Erlin.Lib.Database.PgSql
{
    /// <summary>
    /// Postgresql data reader
    /// </summary>
    public class PgSqlDataReader : IDbDataReader
    {
        /// <summary>
        /// Underlying db reader
        /// </summary>
        public NpgsqlDataReader UnderlyingReader { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="underlyingReader">Underlying db reader</param>
        public PgSqlDataReader(NpgsqlDataReader underlyingReader)
        {
            UnderlyingReader = underlyingReader;
        }

        /// <summary>
        /// Advances the data reader to the next result, when reading the results of batch Transact-SQL statements.
        /// </summary>
        /// <returns><see langword="true" /> if there are more result sets; otherwise <see langword="false" />.</returns>
        public bool NextResult()
        {
            return UnderlyingReader.NextResult();
        }

        /// <summary>
        /// Reads record from DB
        /// </summary>
        /// <typeparam name="T">Runtime type of record</typeparam>
        /// <returns>Readed record or null</returns>
        public T? Read<T>()
            where T : class, IDbReadable, new()
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
            string? readed = ReadStringN(fieldName);
            if (readed == null)
            {
                throw new NullReferenceException(fieldName);
            }

            return readed;
        }

        /// <summary>
        /// Reads string from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        public string? ReadStringN(string fieldName)
        {
            if (UnderlyingReader.IsDBNull(fieldName))
            {
                return null;
            }

            return UnderlyingReader.GetString(fieldName);
        }

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        public byte ReadByte(string fieldName)
        {
            byte? readed = ReadByteN(fieldName);
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
        public byte? ReadByteN(string fieldName)
        {
            if (UnderlyingReader.IsDBNull(fieldName))
            {
                return null;
            }

            return UnderlyingReader.GetByte(fieldName);
        }

        /// <summary>
        /// Reads value from db result
        /// </summary>
        /// <param name="fieldName">FieldName</param>
        /// <returns>Readed value</returns>
        public short ReadInt16(string fieldName)
        {
            short? readed = ReadInt16N(fieldName);
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
        public short? ReadInt16N(string fieldName)
        {
            if (UnderlyingReader.IsDBNull(fieldName))
            {
                return null;
            }

            return UnderlyingReader.GetInt16(fieldName);
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
        public bool ReadBool(string fieldName)
        {
            bool? readed = ReadBoolN(fieldName);
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
        public bool? ReadBoolN(string fieldName)
        {
            if (UnderlyingReader.IsDBNull(fieldName))
            {
                return null;
            }

            return UnderlyingReader.GetBoolean(fieldName);
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
