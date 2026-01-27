using System;
using System.Collections.Generic;

namespace EP_0201_MW.Models;

public partial class Role
{
    public int IdRole { get; set; }

    public string Title { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
