using System;
using System.Collections.Generic;

namespace EP_0201_MW.Models;

public partial class Object
{
    public int IdObject { get; set; }

    public string Address { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
}
