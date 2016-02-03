using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Users.Models;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace Contrib.Mod.ChangePassword.ViewModels
{
    public class ChangePasswordViewModel
    {
        public UserPart UserPart { get; set; }

        [Required]
        public int UserId { get; set; }
        
        [Required]
        public string Password { get; set; }

        [Required, EqualTo("Password")]
        public string ConfirmPassword { get; set; }
    }
}