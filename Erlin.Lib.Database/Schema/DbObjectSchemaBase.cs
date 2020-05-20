using System;

using Erlin.Lib.Common;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Erlin.Lib.Common.Serialization;

namespace Erlin.Lib.Database.Schema
{
    /// <summary>
    /// Base class for any database schema object
    /// </summary>
    public abstract class DbObjectSchemaBase : IDeSerializable
    {
        /// <summary>
        /// Name of the object
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Type of database object
        /// </summary>
        public abstract DbObjectType ObjectType { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name of the object</param>
        protected DbObjectSchemaBase(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        protected DbObjectSchemaBase()
        {
            Name = IDeSerializable.DUMMY_STRING;
        }

        /// <summary>
        /// (De)Serialize this object
        /// </summary>
        /// <param name="rw">Reader/writer</param>
        public abstract void DeSerialize(IObjectReadWriter rw);

        /// <summary>
        /// Returns full database name/identifier
        /// </summary>
        /// <returns></returns>
        public abstract string GetFullName();

        /// <summary>
        /// Compare collection of SbSchema objects to another collection of same type
        /// </summary>
        /// <typeparam name="T">Runtime type of SbSchema object</typeparam>
        /// <param name="result">Compare result object</param>
        /// <param name="leftList">Master collection</param>
        /// <param name="rightList">Check collection</param>
        /// <param name="equalityMethod">Method to identify "same" objects</param>
        /// <param name="compareMethod">Custom copmarison method</param>
        protected static void CompareCollection<T>(DbSchemaCompareResult result, IEnumerable<T> leftList, IEnumerable<T> rightList, Func<T, T, bool> equalityMethod,
                                                   Func<T, T, DbSchemaCompareResult> compareMethod)
            where T : DbObjectSchemaBase
        {
            leftList.Indiferente(rightList, equalityMethod,
                                 left =>
                                 {
                                     DbSchemaCompareResult leftResult = new DbSchemaCompareResult(left, null);
                                     leftResult.CompareResultType = DbSchemaCompareResultType.Missing;
                                     result.InnerResults.Add(leftResult);
                                 },
                                 right =>
                                 {
                                     DbSchemaCompareResult rightResult = new DbSchemaCompareResult(null, right);
                                     rightResult.CompareResultType = DbSchemaCompareResultType.Redudant;
                                     result.InnerResults.Add(rightResult);
                                 },
                                 (left, right) =>
                                 {
                                     DbSchemaCompareResult bothResult = compareMethod(left, right);
                                     result.InnerResults.Add(bothResult);
                                 });
        }
    }
}