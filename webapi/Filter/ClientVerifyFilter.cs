using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using WebAPI.Lib;


namespace WebAPI.Filter
{
    public class ClientVerifyFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var data = context.HttpContext.Request.Body; // need to fix it
            context.HttpContext.Items.Add("AID", 1);
        }
    }
}
