using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SessionStateDemo.Models
{
    [Serializable]
    public class Users
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }
}