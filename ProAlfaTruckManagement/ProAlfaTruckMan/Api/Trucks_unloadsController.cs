using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProAlfaTruckMan.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProAlfaTruckMan
{
    [Route("api/[controller]")]
    [Authorize]
    public class Trucks_unloadsController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public string Get()
        {
            return "value";
        }

        // GET: /api/<controller>/GetByRampIdDateRange/1/2018-05-15/2018-05-21
        [Route("GetByRampIdDateRange/{rampid}/{datefrom:datetime}/{dateto:datetime}")]
        public IEnumerable<Truck_unload> GetByRampIdDateRange(string rampid, DateTime datefrom, DateTime dateto)
        {
            Tuple<List<Truck_unload>, string> retval = Truck_unloadDataManagement.GetData(rampid, "", datefrom, dateto);
            return retval.Item1;
        }

        // GET: /api/<controller>/AddUpdateDelete/MODIFY/25/1/652123/2018-04-20 08:00:00/2018-04-31 09:00:00/0/0/1/Poznamka/0/mklempa
//        [Route("AddUpdateDelete/{mode}/{rowid:int}/{rampid}/{vendnum}/{datefrom:datetime}/{dateto:datetime}/{palqnty:int}/{src:int}/{blocktype:int}/{remark}/{startrowid:int}/{inetname}")]
        [Route("AddUpdateDelete/{mode}")]
        public int AddUpdateDelete(string mode, int rowid, string rampid, string vendnum, DateTime datefrom, DateTime dateto, int palqnty, int src, int blocktype, string remark, int startrowid, string inetname)
        {
            remark = remark.Trim() != "-" ? remark.Trim() : "";
            inetname = inetname.Trim() != "-" ? inetname.Trim() : "";
            Tuple<int, string> retval = Truck_unloadDataManagement.AddUpdateDelete(mode, rowid, rampid, vendnum, datefrom, dateto, palqnty, src, blocktype, remark, startrowid, inetname, true);
            return retval.Item1;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
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
