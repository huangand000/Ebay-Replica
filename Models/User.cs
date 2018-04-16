using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace belt_exam.Models
{
    public class User : BaseEntity
    {
        [Required(ErrorMessage = "Enter your Name")]
        [MinLength(3, ErrorMessage = "Name must be more than 3 characters long")]
        [MaxLength(20, ErrorMessage = "Name cannot be more than 20 characters long")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First name must be letters only")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Enter your Username")]
        [MinLength(2, ErrorMessage = "Username must be more than 2 characters long")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last name must be letters only")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Enter your password")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Password Confirmation")]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }

        public int Balance {get; set;}
    }
}