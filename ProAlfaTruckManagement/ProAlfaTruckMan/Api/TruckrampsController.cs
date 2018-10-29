using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProAlfaTruckMan.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProAlfaTruckMan
{
    [Route("api/[controller]")]
    [Authorize]
    public class TruckrampsController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Truckramp> Get()
        {
            return TruckrampDataManagement.GetData("");
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public IEnumerable<Truckramp> Get(string id)
        {
            return TruckrampDataManagement.GetData(id);
        }

        // GET: /api/<controller>/AddUpdateDelete/NEW/4/rampa_4/1/05:00
//        [Route("AddUpdateDelete/{mode}/{id}/{dsc}/{vendres:int}/{paltime}")]
        [Route("AddUpdateDelete/{mode}")]
        public int AddUpdateDelete(string mode, string id, string dsc, int vendres, string paltime)
        {
            int result=0;
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            try
            {
                id = id.Trim();
                dsc = dsc.Trim();
                paltime = paltime.Trim();
                switch (mode.ToUpper())
                {
                    case "NEW":
                        cmd.CommandText = "INSERT INTO truckramps (id,dsc,vendres,paltime) VALUES (@Id, @Dsc, @Vendres, @Paltime)";
                        break;
                    case "MODIFY":
                        cmd.CommandText = "UPDATE truckramps SET dsc=@Dsc, vendres=@Vendres, paltime=@Paltime WHERE id=@Id";
                        break;
                    case "DELETE":
                        cmd.CommandText = "DELETE FROM truckramps WHERE id=@Id";
                        break;
                    default:
                        break;
                }
                cmd.Parameters.Add("@Id", SqlDbType.NVarChar).Value = id;
                cmd.Parameters.Add("@Dsc", SqlDbType.NVarChar).Value = dsc;
                cmd.Parameters.Add("@Vendres", SqlDbType.TinyInt).Value = vendres;
                cmd.Parameters.Add("@Paltime", SqlDbType.NVarChar).Value = paltime;
                cmd.Connection = conn;
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                if (mode=="DELETE")
                {
                    cmd.CommandText = "DELETE FROM truckramps_block WHERE rampid='" + id + "'";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM truckramps_cal WHERE rampid='" + id + "'";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM sapvendors_ramp WHERE rampid='" + id + "'";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM sapvendors_res WHERE rampid='" + id + "'";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM trucks_unloads WHERE rampid='" + id + "'";
                    cmd.ExecuteNonQuery();
                }
                cmd.Dispose();
            }
            catch
            {
                result = -1;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

    }
}
