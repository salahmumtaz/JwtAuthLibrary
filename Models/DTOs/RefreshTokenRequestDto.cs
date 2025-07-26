using System;
using System.ComponentModel.DataAnnotations;

namespace JwtAuthLibrary.Models.DTOs
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public required string RefreshToken { get; set; }
    }
}
