using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PARKit.Backend.Enums;

namespace PARKit.Backend.Models
{
    public class Role
    {
        [Key]
        public int Id {get;set;}
        [Required]
        public UserRole RoleName {get;set;}
    }

}