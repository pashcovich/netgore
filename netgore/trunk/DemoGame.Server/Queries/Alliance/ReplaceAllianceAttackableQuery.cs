using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DemoGame.DbObjs;
using DemoGame.Server.DbObjs;
using NetGore.Db;

namespace DemoGame.Server.Queries
{
    [DbControllerQuery]
    public class ReplaceAllianceAttackableQuery : DbQueryNonReader<IAllianceAttackableTable>
    {
        static readonly string _queryStr = string.Format("REPLACE INTO `{0}` {1}", AllianceAttackableTable.TableName,
                                                         FormatParametersIntoValuesString(AllianceAttackableTable.DbColumns));

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceAllianceAttackableQuery"/> class.
        /// </summary>
        /// <param name="connectionPool"><see cref="DbConnectionPool"/> to use for creating connections to
        /// execute the query on.</param>
        public ReplaceAllianceAttackableQuery(DbConnectionPool connectionPool) : base(connectionPool, _queryStr)
        {
        }

        public int Execute(AllianceID allianceID, AllianceID attackableID)
        {
            return Execute(new AllianceAttackableTable(allianceID, attackableID));
        }

        public int Execute(AllianceID allianceID, IEnumerable<AllianceID> attackableIDs)
        {
            var sum = 0;
            foreach (var attackableID in attackableIDs)
            {
                sum += Execute(allianceID, attackableID);
            }
            return sum;
        }

        /// <summary>
        /// When overridden in the derived class, creates the parameters this class uses for creating database queries.
        /// </summary>
        /// <returns>
        /// IEnumerable of all the <see cref="DbParameter"/>s needed for this class to perform database queries.
        /// If null, no parameters will be used.
        /// </returns>
        protected override IEnumerable<DbParameter> InitializeParameters()
        {
            return CreateParameters(AllianceAttackableTable.DbColumns.Select(x => "@" + x));
        }

        /// <summary>
        /// When overridden in the derived class, sets the database parameters values <paramref name="p"/>
        /// based on the values specified in the given <paramref name="item"/> parameter.
        /// </summary>
        /// <param name="p">Collection of database parameters to set the values for.</param>
        /// <param name="item">The value or object/struct containing the values used to execute the query.</param>
        protected override void SetParameters(DbParameterValues p, IAllianceAttackableTable item)
        {
            item.CopyValues(p);
        }
    }
}