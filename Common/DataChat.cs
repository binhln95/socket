using System;
using System.Collections.Generic;

namespace Common
{
    [Serializable]
    public class DataChat
    {
        public string User;
        public string Message;
        public string IpEndPoint;
        public bool isGetUser = false;
        public List<string> lstUser;
    }
}
