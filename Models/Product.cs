using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace belt_exam.Models {
    public class Product {
        [Required]
        [MinLength(3)]
        [Display(Name = "Product Name")]
        public string Name {get; set;}
        [Required]
        [MinLength(10)]
        public string Description {get; set;}
        [Required(ErrorMessage = "Bid must be a postive number")]
        [Display(Name = "Starting Bid")]
        [Range(1, Int32.MaxValue)]
        public int Bid {get; set;}
        [Required]
        [DateValid(ErrorMessage = "Date must be in future")]
        [Display(Name = "End Date")]
        public DateTime EndDate {get; set;}
    }
    public class DateValid: ValidationAttribute {
        public override bool IsValid(object value) {
            return value != null && (DateTime)value >= DateTime.Now;
        }
    }
}