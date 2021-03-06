﻿using System;

using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public class RegistrationRequest : Request
    {
        [JsonProperty("user_name")]
        public String UserName { get; set; }

        [JsonProperty("password")]
        public String Password { get; set; }

        public RegistrationRequest(String user_name, String password) : base("registration")
        {
            UserName = user_name;
            Password = password;
        }
    }
}
