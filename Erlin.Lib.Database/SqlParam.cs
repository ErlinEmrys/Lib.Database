using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erlin.Lib.Database
{
    /// <summary>
    /// Represent one SQL parameter
    /// </summary>
    [Serializable]
    public class SqlParam
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Sql name of the parameter
        /// </summary>
        public string SqlName { get { return $"@{Name}"; } }

        /// <summary>
        /// Current value of the parameter
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Direction of the parameter
        /// </summary>
        public ParameterDirection Direction { get; private set; }

        /// <summary>
        /// Manually selected data-type of the parameter
        /// </summary>
        public DbType? SqlType { get; private set; }

        /// <summary>
        /// Size of value type
        /// </summary>
        public int? Size { get; private set; }

        /// <summary>
        /// Associated SQL object
        /// </summary>
        internal IDbDataParameter? SqlParameter { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name of the parameter (without rollmop)</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="direction">Direction of the parameter</param>
        /// <param name="sqlType">Sql type of the parametr</param>
        /// <param name="size">Size of the sql type</param>
        public SqlParam(string name, object value, DbType? sqlType = null, int? size = null, ParameterDirection direction = ParameterDirection.Input)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Sql parameter must have a name!", nameof(name));
            }

            Name = name.TrimStart('@');
            Value = value;
            Direction = direction;
            SqlType = sqlType;
            Size = size;
        }
    }
}