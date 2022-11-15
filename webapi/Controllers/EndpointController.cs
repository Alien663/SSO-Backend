using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Lib;
using WebAPI.Model;
using WebAPI.Filter;
using Dapper;
using NLog.Time;
using System.Net.Http;

namespace WebAPI.Controllers
{

    [Route("api/[controller]")]
    [ClientVerifyFilter]
    public class EndpointController : ControllerBase
    {
        [HttpPost]
        public IActionResult Verify()
        {
            try
            {
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] MemberLoginModel2 payload)
        {
            try
            {
                int aid = int.Parse(HttpContext.Items["AID"].ToString());
                using(var db = new AppDb())
                {
                    string authCode = "";
                    bool isGrant = false;
                    string sql = @"xp_EndpointLoginWithPassword";
                    DynamicParameters p = new DynamicParameters();
                    p.Add("@acc", payload.Account);
                    p.Add("@pwd", payload.Password);
                    p.Add("@aid", aid);
                    p.Add("@isGrant", isGrant, direction: ParameterDirection.Output);
                    p.Add("@authcode", authCode, direction: ParameterDirection.Output);
                    db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                    authCode = p.Get<Guid>("authcode").ToString();
                    isGrant = p.Get<bool>("isGrant");
                    return Ok(new { isGrant });
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("grant/confirm")]
        [AuthorizationFilter]
        public IActionResult Confirm([FromBody] ClientModel payload)
        {
            int aid = int.Parse(HttpContext.Items["AID"].ToString());
            int mid = int.Parse(HttpContext.Items["MID"].ToString());
            try
            {
                string sql = @"insert into Grants(MID, AID) values(@mid, @aid)";
                using (var db = new AppDb())
                {
                    db.Connection.Execute(sql, new { mid, aid });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("grant/check/{apikey}")]
        [AuthorizationFilter]
        public IActionResult IsGrant(string apikey)
        {
            int aid = int.Parse(HttpContext.Items["AID"].ToString());
            string sql = @"select 1 from Grants G where G.MID = @mid and G.AID = @aid";
            using (var db = new AppDb())
            {
                int mid = int.Parse(HttpContext.Items["MID"].ToString());
                var flag = db.Connection.QuerySingleOrDefault(sql, new { mid, aid });    
                if(flag == null)
                {
                    return BadRequest();
                }
                else
                {
                    return Ok();
                }
            }
        }
        
        [HttpGet("Member/{authcode}")]
        [AuthorizationFilter]
        public IActionResult GetMemberData(string authcode)
        {
            try
            {
                int aid = int.Parse(HttpContext.Items["AID"].ToString());
                string sql = @"declare @_authcode uniqueidentifier = @authcode
                    declare @ssid int = (select SSID from vd_AuthorizationCodeSession where AuthorizationCode = @_authcode and Expired >= GETDATE())
                    if @ssid is null begin
                        delete SystemSession from AuthCode A where SystemSession.SSID = A.SSID and A.AuthorizationCode = @_authcode
                    end else begin
                        select Account, EMail, NickName from vd_LoginSession where SSID = @ssid
                    end";
                using(var db = new AppDb())
                {
                    var data = db.Connection.QueryFirstOrDefault(sql, new { authcode });
                    if(data == null)
                    {
                        throw new Exception("Unknow error happend, please try again or connect IT support");
                    }
                    else
                    {
                        return Ok(data);
                    }
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
