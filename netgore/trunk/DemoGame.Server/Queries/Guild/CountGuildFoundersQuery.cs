﻿using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DemoGame.Server.DbObjs;
using NetGore.Db;
using NetGore.Features.Guilds;

namespace DemoGame.Server.Queries
{
    [DbControllerQuery]
    public class CountGuildFoundersQuery : DbQueryReader<GuildID>
    {
        static readonly string _queryStr = string.Format("SELECT COUNT(*) FROM `{0}` WHERE `guild_id` = @guildID",
                                                         GuildMemberTable.TableName);

        /// <summary>
        /// Initializes a new instance of the <see cref="CountGuildFoundersQuery"/> class.
        /// </summary>
        /// <param name="connectionPool">DbConnectionPool to use for creating connections to execute the query on.</param>
        public CountGuildFoundersQuery(DbConnectionPool connectionPool) : base(connectionPool, _queryStr)
        {
        }

        public int Execute(GuildID guildID)
        {
            using (var r = ExecuteReader(guildID))
            {
                if (!r.Read())
                    return 0;

                return r.GetInt32(0);
            }
        }

        /// <summary>
        /// When overridden in the derived class, creates the parameters this class uses for creating database queries.
        /// </summary>
        /// <returns>IEnumerable of all the <see cref="DbParameter"/>s needed for this class to perform database queries.
        /// If null, no parameters will be used.</returns>
        protected override IEnumerable<DbParameter> InitializeParameters()
        {
            return CreateParameters("@guildID");
        }

        /// <summary>
        /// When overridden in the derived class, sets the database parameters values <paramref name="p"/>
        /// based on the values specified in the given <paramref name="item"/> parameter.
        /// </summary>
        /// <param name="p">Collection of database parameters to set the values for.</param>
        /// <param name="item">The value or object/struct containing the values used to execute the query.</param>
        protected override void SetParameters(DbParameterValues p, GuildID item)
        {
            p["@guildID"] = (int)item;
        }
    }
}