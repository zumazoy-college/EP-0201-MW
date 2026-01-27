using System;
using System.Collections.Generic;

namespace EP_0201_MW.Models;

public partial class Service
{
    public int IdService { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<ProvidedService> ProvidedServices { get; set; } = new List<ProvidedService>();
}
