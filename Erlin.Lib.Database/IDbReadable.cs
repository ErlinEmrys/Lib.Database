using System;
using System.Collections.Generic;
using System.Text;

namespace Erlin.Lib.Database
{
    /// <summary>
    /// Interface of object, taht can be readed from DB
    /// </summary>
    public interface IDbReadable
    {
        /// <summary>
        /// Read data from db result
        /// </summary>
        /// <param name="reader">DB reader</param>
        void DbRead(IDbDataReader reader);
    }
}