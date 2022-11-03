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
            string strSql = @"select 1 from vd_AppGrant";
            using(var db = new AppDb())
            {
                var flag = db.Connection.QuerySingleOrDefault(strSql);
                if(flag is null)
                {
                    return BadRequest();
                }
                else
                {
                    return Ok();
                }
            }
        }
    }
}
