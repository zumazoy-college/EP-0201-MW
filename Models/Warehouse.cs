using System;
using System.Collections.Generic;

namespace EP_0201_MW.Models;

public partial class Warehouse
{
    public int IdWarehouse { get; set; }

    public string WarehouseNumber { get; set; } = null!;

    public decimal Area { get; set; }

    public decimal MonthlyPrice { get; set; }

    public int StatusId { get; set; }

    public int ObjectId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Lease> Leases { get; set; } = new List<Lease>();

    public virtual Object Object { get; set; } = null!;

    public virtual WarehouseStatus Status { get; set; } = null!;
}
