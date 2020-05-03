using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Erlin.Lib.Common.Serialization;

namespace Erlin.Lib.Database
{
    /// <summary>
    /// Database related exception
    /// </summary>
    [Serializable]
    public class DbException : Exception
    {
        /// <summary>
        /// Original query string sended to DB
        /// </summary>
        public string Query { get; }

        /// <summary>
        /// Original query paramaters
        /// </summary>
        public List<SqlParam> Parameters { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="query">Original query string sended to DB</param>
        /// <param name="prms">Original query paramaters</param>
        public DbException(string query, List<SqlParam> prms)
        {
            Query = query;
            Parameters = prms;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="query">Original query string sended to DB</param>
        /// <param name="prms">Original query paramaters</param>
        /// <param name="originalException">Original exception object</param>
        public DbException(string query, List<SqlParam> prms, Exception originalException) : base(query, originalException)
        {
            Query = query;
            Parameters = prms;
        }

        /// <summary>
        /// Serialization ctor
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="context">Context</param>
        protected DbException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Query = info.GetString(nameof(Query)) ?? IDeSerializable.DUMMY_STRING;
            Parameters = info.GetValue(nameof(Parameters), typeof(List<SqlParam>)) as List<SqlParam> ?? new List<SqlParam>();
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown. </param><param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination. </param><exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic). </exception><PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/></PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(Query), Query);
            info.AddValue(nameof(Parameters), Parameters, typeof(List<SqlParam>));

            base.GetObjectData(info, context);
        }
    }
}