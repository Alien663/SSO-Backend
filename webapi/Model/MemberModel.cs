using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Model
{
    public class MemberModel
    {
        public Guid UUID { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string EMail { get; set; }
        public string NickName { get; set; }
        public DateTime Since { get; set; }
        public DateTime ModifyDate { get; set; }
    }

    public class LoginData
    {
        public string Account { get; set; }
        public string Password { get; set; }
        public Guid token { get; set; }
        public Guid refreshtoken { get; set; }
    }

    public class ResetPassword
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
    public class ResetPaswwrod2
    {
        public Guid Token { get; set; }
        public Guid UUID { get; set; }
        public string NewPassword { get; set; }
    }

    public class MemberVerifyModel
    {
        public int MID { get; set; }
        public Guid UUID { get; set; }
        public string Account { get; set; }
        public string EMail { get; set; }
        public Boolean Verify { get; set; }
        public int SSID { get; set; }
        public Guid Token { get; set; }
    }
}
