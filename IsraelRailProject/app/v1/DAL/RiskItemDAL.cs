using System.Collections.Generic;
using System.Data.SqlClient;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.app.v1.DAL
{
    public static class RiskItemDAL
    {
        public static List<RiskItemDto> GetAll()
        {
            var list = new List<RiskItemDto>();
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT Id, Name
                FROM RiskItems
                ORDER BY Name", conn))
            {
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new RiskItemDto
                        {
                            Id = rd.GetInt32(0),
                            Name = rd.GetString(1)
                        });
                    }
                }
            }
            return list;
        }
    }
}
