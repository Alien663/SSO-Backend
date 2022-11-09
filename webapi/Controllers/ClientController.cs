using Microsoft.AspNetCore.Mvc;
using System;
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
            string strSql = @"select 1 as Flag from APP where APIKEY = @APIKEY";
            using(var db = new AppDb())
            {
                var flag = db.Connection.QuerySingleOrDefault(strSql, new { payload.APIKEY });
                if(flag is null)
                {
                    return BadRequest("Client Verify Fail");
                }
                else
                {
                    return Ok();
                }
            }
        }

        [HttpPost("login")]
        public IActionResult MemberLogin([FromBody] ClientMemberModel payload)
        {
            try
            {
                using(var db = new AppDb())
                {
                    string sql = @"select 1 Flag from APP where APIKEY = @APIKEY";
                    var Flag = db.Connection.QuerySingleOrDefault(sql, new { payload.APIKEY });
                    if(Flag is null)
                    {
                        throw new Exception("Verify Fail");
                    }
                    else
                    {
                        sql = @"";
                    }




                    return Ok();
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
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
