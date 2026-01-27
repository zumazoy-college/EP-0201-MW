using System;
using System.Collections.Generic;

namespace EP_0201_MW.Models;

public partial class WarehouseStatus
{
    public int IdStatus { get; set; }

    public string Title { get; set; } = null!;

    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
}
