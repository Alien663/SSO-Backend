using System;
using WebAPI.Model;

namespace WebAPI.Model
{
    public class EndpointModel
    {
    }

    public class MemberLoginModel2 : LoginData
    {
        public Guid APIKEY { get; set; }
        public bool Confirm { get; set; }
    }
}
