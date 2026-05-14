using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PractApp.Models
{
    public class User
    {
        public string? Login {  get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }

    }
}
