using System;
using System.Collections.Generic;

namespace EP_0201_MW.Models;

public partial class ProvidedService
{
    public int IdProvidedService { get; set; }

    public DateOnly ServiceDate { get; set; }

    public int? Quantity { get; set; }

    public int LeaseId { get; set; }

    public int ServiceId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Lease Lease { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;

    public decimal CalculatedPrice
    {
        get
        {
            if (Service != null && Quantity.HasValue)
            {
                return Service.Price * Quantity.Value;
            }
            return 0;
        }
    }
}
