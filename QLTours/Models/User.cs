using System;
using System.Collections.Generic;

namespace QLTours.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }
    public string PasswordResetCode { get; set; } // Mã reset mật khẩu
    public DateTime? PasswordResetExpiry { get; set; } // Thời hạn mã reset

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
