﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace EPAM_BusinessLogicLayer.DTO
{
    public class RegistrationDTO
    {
        [JsonPropertyName("username")]
        [Required(ErrorMessage = "An username is required"),
         RegularExpression("^[a-zA-Z][a-zA-Z0-9]{3,9}$",
             ErrorMessage =
                 "Username must contain only characters and numbers and be from 4 to 10 characters"),]
        public string Username { get; set; }
        [JsonPropertyName("email")]
        [Required(ErrorMessage = "An email is required"), EmailAddress]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        [Required(ErrorMessage = "A password is required"), 
         RegularExpression("^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,}$",
             ErrorMessage =
                 "Password must be at least 8 characters, and must include at least one upper case letter, one lower case letter, and one numeric digit"),]
        public string Password { get; set; }
    }
}
