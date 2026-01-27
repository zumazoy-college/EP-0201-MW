using System;
using System.Collections.Generic;

namespace EP_0201_MW.Models;

public partial class Position
{
    public int IdPosition { get; set; }

    public string Title { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
