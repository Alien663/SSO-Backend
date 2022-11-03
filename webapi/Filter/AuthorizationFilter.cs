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

namespace WebAPI.Filter
{
    public class AuthorizationFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string token = context.HttpContext.Request.Cookies.Where(x => x.Key == "Token").FirstOrDefault().Value;
            string refresh = context.HttpContext.Request.Cookies.Where(x => x.Key == "RefreshToken").FirstOrDefault().Value;
            if (true)
            {
                try
                {
                    using (var db = new AppDb())
                    {
                        string sql = @"xp_loginWithToken";
                        var p = new DynamicParameters();
                        p.Add("@token", token);
                        p.Add("@mid", DbType.Int32, direction: ParameterDirection.Output);
                        db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                        context.HttpContext.Items.Add("MID", p.Get<object>("mid"));
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Session time out")
                    {
                        using (var db = new AppDb())
                        {
                            string new_token = "", new_refresh = "";
                            string sql = @"xp_refreshToken";
                            var p = new DynamicParameters();
                            p.Add("@token", token);
                            p.Add("@refresh_token", refresh);
                            p.Add("@new_token", new_token, direction: ParameterDirection.Output);
                            p.Add("@new_refresh_token", new_refresh, direction: ParameterDirection.Output);
                            p.Add("@mid", DbType.Int32, direction: ParameterDirection.Output);
                            db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);

                            CookieOptions options = new CookieOptions();
                            options.HttpOnly = true;
                            options.Secure = true;
                            options.Domain = "an990154054";
                            //options.Domain = "localhost";
                            context.HttpContext.Response.Cookies.Append("Token", new_token, options);
                            context.HttpContext.Response.Cookies.Append("RefreshToken", new_refresh, options);
                            context.HttpContext.Items.Add("MID", p.Get<object>("mid"));
                        }
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
            else
            {
                context.HttpContext.Items.Add("MID", 1);
            }
        }
    }
}
