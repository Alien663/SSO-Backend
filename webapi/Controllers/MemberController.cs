using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebAPI.Lib;
using WebAPI.Model;
using WebAPI.Filter;
using System;
using System.Dynamic;

namespace WebAPI.Controllers 
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemberController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody]LoginData req_data)
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = @"xp_loginWithPassword";
                    var p = new DynamicParameters();
                    p.Add("@acc", req_data.Account);
                    p.Add("@pwd", req_data.Password);
                    p.Add("@token", req_data.token, direction: ParameterDirection.Output);
                    p.Add("@refreshtoken", req_data.refreshtoken, direction: ParameterDirection.Output);
                    db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                    CookieOptions options = createCookie();
                    Response.Cookies.Append("Token", p.Get<Guid>("token").ToString(), options);
                    Response.Cookies.Append("RefreshToken", p.Get<Guid>("refreshtoken").ToString(), options);
                    string Message = "Login Success";
                    return Ok(new { Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("login")]
        [AuthorizationFilter]
        public IActionResult AutoLogin()
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

        [HttpPost("logout")]
        [AuthorizationFilter]
        public IActionResult Logout()
        {
            using(var db = new AppDb())
            {
                string sql = @"xp_logout";
                var p = new DynamicParameters();
                p.Add("@mid", (int)HttpContext.Items["MID"]);
                p.Add("@token", HttpContext.Request.Cookies.Where(x => x.Key == "Token").FirstOrDefault().Value);
                p.Add("@refreshtoken", HttpContext.Request.Cookies.Where(x => x.Key == "RefreshToken").FirstOrDefault().Value);
                db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
            }
            Response.Cookies.Delete("Token");
            Response.Cookies.Delete("RefreshToken");
            return Ok();
        }

        [HttpGet]
        [AuthorizationFilter]
        public IActionResult GetMe()
        {
            using(var db = new AppDb())
            {
                string sql = @"select UUID, Account, EMail, NickName, Since, ModifyDate from vd_Member where MID = @mid";
                var mid = HttpContext.Items["MID"];
                MemberModel data = db.Connection.QueryFirstOrDefault<MemberModel>(sql, new { mid });
                return Ok(new { data.UUID, data.Account, data.EMail, data.NickName, data.Since, data.ModifyDate });
            }
        }
        
        [HttpPost]
        [AuthorizationFilter]
        public IActionResult UpdateMe([FromBody] MemberModel payload)
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = "xp_MemberUpdate ";
                    var p = new DynamicParameters();
                    var mid = HttpContext.Items["MID"];

                    p.Add("@mid", HttpContext.Items["MID"]);
                    p.Add("@uuid", payload.UUID);
                    p.Add("@acc", payload.Account);
                    p.Add("@nickname", payload.NickName);
                    p.Add("email", payload.EMail);

                    db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                }
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public IActionResult CreateNewUser([FromBody] MemberModel req_data)
        {
            try
            {
                int new_mid;
                using (var db = new AppDb())
                {
                    string sql = @"xp_createNewUser";
                    var p = new DynamicParameters();
                    p.Add("@acc", req_data.Account);
                    p.Add("@pwd", req_data.Password);
                    p.Add("@email", req_data.EMail);
                    p.Add("@nickname", req_data.NickName);
                    p.Add("@mid", DbType.Int32, direction: ParameterDirection.Output);
                    db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                    new_mid = int.Parse(p.Get<object>("mid").ToString());
                    HttpContext.Items.Add("MID", new_mid);


                    sql = @"xp_MemberSendVerify";
                    p = new DynamicParameters();
                    p.Add("@mid", new_mid);
                    db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                }

                MemberVerifyModel data = new MemberVerifyModel();
                // data = ResendVerifyCode();

                // send verify email
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("verify/send")]
        [AuthorizationFilter]
        public IActionResult ResendVerifyCode()
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = "xp_MemberSendVerify";
                    var p = new DynamicParameters();
                    int MID = int.Parse(HttpContext.Items["MID"].ToString());
                    p.Add("@mid", MID);

                    List<MemberVerifyModel> data = new List<MemberVerifyModel>();
                    var reader = db.Connection.ExecuteReader(sql, p, commandType: CommandType.StoredProcedure);
                    //reader.Read();
                    data = SqlMapper.Parse<MemberVerifyModel>(reader).ToList();
                    reader.Close();
                    string result = "member/verify/" + data[0].Token.ToString() + "/" + data[0].UUID.ToString();
                    return Ok(new { result });
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }

        [HttpPost("verify")]
        public IActionResult VerifyNewAccount([FromBody] MemberVerifyModel payload)
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = "xp_MemberVerify";
                    var p = new DynamicParameters();

                    p.Add("@token", payload.Token);
                    p.Add("@guid", payload.UUID);

                    db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                }
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("password/renew")]
        [AuthorizationFilter]
        public IActionResult RenewPassword([FromBody] ResetPassword payload)
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = "xp_MemberRenewPassword";
                    var p = new DynamicParameters();
                    p.Add("@mid", HttpContext.Items["MID"]);
                    p.Add("@oldpassword", payload.OldPassword);
                    p.Add("@newpassword", payload.NewPassword);
                    db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("password/renew/verify/{token}/{uuid}")]
        public IActionResult VerifyRenewPassword(string token, string uuid)
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = @"select 1 as Flag from vd_ForgePasswordSession where UUID = @uuid and Token = @token and Expired >= getdate()";
                    var p = new DynamicParameters();

                    var result = db.Connection.QuerySingleOrDefault(sql, new { token, uuid });
                    
                    if(result == null)
                    {
                        return BadRequest("Invalidate Request");
                    }
                    else
                    {
                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("password/renew/forget")]
        public IActionResult RenewForgetPassword([FromBody] ResetPaswwrod2 payload)
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = @"xp_MemberRenewForgetPassword";
                    var p = new DynamicParameters();
                    p.Add("@token", payload.Token);
                    p.Add("@uuid", payload.UUID);
                    p.Add("@newpassword", payload.NewPassword);
                    db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                    return Ok();
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

            
        }

        [HttpPost("password/forget")]
        public IActionResult VerifyForgetPassword([FromBody] MemberVerifyModel payload)
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = "xp_PasswordSendVerify";
                    var p = new DynamicParameters();
                    p.Add("@email", payload.EMail);
                    p.Add("@account", payload.Account);
                    var reader = db.Connection.ExecuteReader(sql, p, commandType: CommandType.StoredProcedure);
                    List<MemberVerifyModel> data = SqlMapper.Parse<MemberVerifyModel>(reader).ToList();
                    string result = "member/resetpassword/" + data[0].Token.ToString() + "/" + data[0].UUID.ToString();
                    return Ok(new { result });
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("dupicate/account/{acc}")]
        public IActionResult DuplicateAccount(string acc)
        {
            string strSql = @"
                if exists(select 1 from Member where Account = @acc) select 1 as result
                else select 0 as result";
            using(var db = new AppDb())
            {
                var result = db.Connection.QuerySingle(strSql, new { acc });
                if (result["result"] == 1)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("This Account had been regeisted, please change another one");
                }
            }
        }

        [HttpGet("dupicate/email/{email}")]
        public IActionResult DuplicateEMail(string email)
        {
            string strSql = @"
                if exists(select 1 from Member where EMail = @email) select 1 as result
                else select 0 as result";
            using (var db = new AppDb())
            {
                var result = db.Connection.QuerySingle(strSql, new { email });
                if (result["result"] == 1)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("This EMail had been regeisted, please change another one");
                }
            }
        }

        [HttpPost("grant")]
        public IActionResult UpdateGrant()
        {
            /* <summary>
             *  user's page to maintain grant
             * </summary>
            */
            return Ok();
        }

        private CookieOptions createCookie()
        {
            CookieOptions options = new CookieOptions();
            options.HttpOnly = true;
            options.Secure = true;
            options.SameSite = SameSiteMode.None;
            options.Expires = DateTimeOffset.Now.AddDays(1);
            options.Path = "/";

            return options;
        }

    }
}
