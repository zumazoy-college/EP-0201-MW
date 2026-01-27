using System;
using System.Collections.Generic;

namespace EP_0201_MW.Models;

public partial class Employee
{
    public int IdEmployee { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string Email { get; set; } = null!;

    public int PositionId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Lease> Leases { get; set; } = new List<Lease>();

    public virtual Position Position { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
