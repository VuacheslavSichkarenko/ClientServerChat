﻿using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public class AuthResponse : Response
    {
        [JsonProperty("user")]
        public User @User { get; set; }

        public AuthResponse(User user) : base("auth")
        {
            User = user;
        }
    }
}