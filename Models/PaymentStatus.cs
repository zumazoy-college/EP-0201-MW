using System;
using System.Collections.Generic;

namespace EP_0201_MW.Models;

public partial class PaymentStatus
{
    public int IdPstatus { get; set; }

    public string Title { get; set; } = null!;

    public virtual ICollection<Lease> Leases { get; set; } = new List<Lease>();
}
