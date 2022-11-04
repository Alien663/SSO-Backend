using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using WebAPI.Lib;
using Dapper;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace WebAPI.Filter
{
    public class AuthorizationFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string _token;
            string _refresh;
            bool isDev = true; // this is for quickly develop, need to remote before pro
            if (isDev)
            {
                context.HttpContext.Items.Add("MID", 1);
            }
            else
            {
                _token = context.HttpContext.Request.Cookies.Where(x => x.Key == "Token").FirstOrDefault().Value;
                _refresh = context.HttpContext.Request.Cookies.Where(x => x.Key == "RefreshToken").FirstOrDefault().Value;
                if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(_refresh))
                {
                    throw new Exception("Token or Refresh Token is Empty");
                }

                try
                {
                    string new_token = "", new_refresh = "";
                    using (var db = new AppDb())
                    {
                        string sql = @"xp_loginWithToken";
                        var p = new DynamicParameters();
                        p.Add("@token", _token);
                        p.Add("@mid", DbType.Int32, direction: ParameterDirection.Output);
                        db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                        int MID = int.Parse(p.Get<object>("mid").ToString());
                        if(MID == -1)
                        {
                            sql = @"xp_refreshToken";
                            p = new DynamicParameters();
                            p.Add("@token", _token);
                            p.Add("@refresh_token", _refresh);
                            p.Add("@new_token", new_token, direction: ParameterDirection.Output);
                            p.Add("@new_refresh_token", new_refresh, direction: ParameterDirection.Output);
                            p.Add("@mid", DbType.Int32, direction: ParameterDirection.Output);
                            db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                            context.HttpContext.Items.Add("MID", p.Get<object>("mid"));
                        }
                        else
                        {
                            context.HttpContext.Items.Add("MID", p.Get<object>("mid"));
                        }
                    }
                    CookieOptions options = new CookieOptions();
                    options.HttpOnly = true;
                    options.Secure = true;
                    context.HttpContext.Response.Cookies.Append("Token", new_token, options);
                    context.HttpContext.Response.Cookies.Append("RefreshToken", new_refresh, options);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
