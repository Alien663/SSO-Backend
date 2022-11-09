using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Model
{
    public class ClientModel
    {
        public Guid APIKEY { get; set; }
        public string URL { get; set; }
        public string RedirectURL { get; set; }
        public string scope { get; set; }
        public string State { get; set; }
    }

    public class ClientMemberModel
    {
        public string APIKEY { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }

    }

    public class ClientMemberLoginResultModel
    {
        public string Domain { get; set; }
        public bool isGrant { get; set; }
        public Guid AccessToken { get; set; }
        public Guid RefreshToken { get; set; }
    }
}
