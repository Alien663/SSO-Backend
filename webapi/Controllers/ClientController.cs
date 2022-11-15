using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Lib;
using WebAPI.Model;
using Dapper;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        [HttpPost]
        public IActionResult Update([FromBody] ClientModel payload)
        {
            string strSql = @"xp_ClientUpdate";
            var p = new DynamicParameters();
            using (var db = new AppDb())
            {
                db.Connection.Execute(strSql);
            }
            return Ok();
        }


        [HttpPut]
        public IActionResult Create([FromBody] ClientModel payload)
        {
            string strSql = @"xp_ClientCreate";
            var p = new DynamicParameters();
            using(var db = new AppDb())
            {
                db.Connection.Execute(strSql);
            }
            return Ok();
        }


        [HttpPost("verify")]
        public IActionResult verify([FromBody] ClientModel payload)
        {
            string strSql = @"declare @aid int = (select AID from APP where APIKEY = @APIKEY)
                if @aid is not null
                    select [Name], [URL], RedirectURL from APP where AID = @aid";
            using(var db = new AppDb())
            {
                ClientModel data = db.Connection.QueryFirstOrDefault<ClientModel>(strSql, new { payload.APIKEY });
                if(data is null)
                {
                    return BadRequest("Client Verify Fail");
                }
                else
                {
                    return Ok(data);
                }
            }
        }


        [HttpPost("grant")]
        public IActionResult UpdateGrant()
        {
            try
            {
                using(var db = new AppDb())
                {
                    string sql = "insert into Grants(MID, AID)";
                    var p = new DynamicParameters();

                    db.Connection.Execute(sql, p, commandType: System.Data.CommandType.StoredProcedure);
                    return Ok();
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
