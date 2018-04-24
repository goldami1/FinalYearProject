﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace InnoviApiProxy
{
    public class Account : InnoviObject
    {
        [JsonProperty("id")]
        private string accountId { get;  set; }
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public eAccountStatus Status { get; private set; }
        //       public InnoviObjectCollection<CustomerFolder> CustomerFolders { get; set; }
//        [JsonProperty]
 //       public List<Folder> Folders { get; private set; }

        public enum eAccountStatus
        {
            Active = 0,
            Undefined,
            Suspended
        }

        internal Account() { }
    }
}