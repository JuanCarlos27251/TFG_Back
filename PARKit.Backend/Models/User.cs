using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;
using PARKit.Backend.Enums;
using PArRKit.Backend.Enums;
using System;

namespace PARKit.Backend.Models
{
    public class User
    {
        [Key]
        public int Id {get;set;}


        [Required]
        public string Name {get;set;}
        [Required]
        public string Email {get;set;}
        [Required]
        public string PasswordHash{get;set;}
        public DateTime CreatedAT {get;set;} = DateTime.UtcNow;

        [Required]
        public UserRole Role {get;set;}
    }
}