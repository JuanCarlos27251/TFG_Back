using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;
using PARKit.Backend.Enums;
using System;

namespace PARKit.Backend.Models
{
    public class User
    {
        [Key]
        public int Id {get;set;}
        
        [Required]
        public string Name {get;set;} = string.Empty;
        [Required]
        public string Email {get;set;} = string.Empty;
        [Required]
        public string PasswordHash{get;set;} = string.Empty;
        public DateTime CreatedAT {get;set;} = DateTime.UtcNow;
        public bool IsActive{get;set;} = true;
        public string? Phone {get;set;}

        [Required]
        public string Role {get;set;} =string.Empty;

        [ForeignKey("Company")]
        public int? CompanyId {get;set;}
        
    }
}