using System;
using System.Collections.Generic;

namespace EP_0201_MW.Models;

public partial class Lease
{
    public int IdLease { get; set; }

    public string ContractNumber { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal TotalPrice { get; set; }

    public int ClientId { get; set; }

    public int WarehouseId { get; set; }

    public int ManagerId { get; set; }

    public int PstatusId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Employee Manager { get; set; } = null!;

    public virtual ICollection<ProvidedService> ProvidedServices { get; set; } = new List<ProvidedService>();

    public virtual PaymentStatus Pstatus { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
