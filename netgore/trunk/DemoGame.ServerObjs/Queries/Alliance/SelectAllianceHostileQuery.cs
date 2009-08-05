using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using NetGore.Db;

namespace DemoGame.Server.Queries
{
    [DBControllerQuery]
    public class SelectAllianceHostileQuery : DbQueryReader<AllianceID>
    {
        static readonly string _queryString = string.Format("SELECT * FROM `{0}` WHERE `alliance_id`=@id",
                                                            DBTables.AllianceHostile);

        public SelectAllianceHostileQuery(DbConnectionPool connectionPool) : base(connectionPool, _queryString)
        {
        }

        public SelectAllianceHostileQueryValues Execute(AllianceID id)
        {
            var hostileIDs = new List<AllianceID>();

            using (IDataReader r = ExecuteReader(id))
            {
                while (r.Read())
                {
                    AllianceID hostileID = r.GetAllianceID("hostile_id");
                    hostileIDs.Add(hostileID);
                }
            }

            SelectAllianceHostileQueryValues ret = new SelectAllianceHostileQueryValues(id, hostileIDs);
            return ret;
        }

        protected override IEnumerable<DbParameter> InitializeParameters()
        {
            return CreateParameters("@id");
        }

        protected override void SetParameters(DbParameterValues p, AllianceID id)
        {
            p["@id"] = (int)id;
        }
    }
}