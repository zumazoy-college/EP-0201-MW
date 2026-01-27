using System;
using System.Collections.Generic;

namespace EP_0201_MW.Models;

public partial class Client
{
    public int IdClient { get; set; }

    public string CompanyName { get; set; } = null!;

    public string LastNamePerson { get; set; } = null!;

    public string FirstNamePerson { get; set; } = null!;

    public string? MiddleNamePerson { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Requisites { get; set; }

    public DateOnly? ContractDate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Lease> Leases { get; set; } = new List<Lease>();
}
