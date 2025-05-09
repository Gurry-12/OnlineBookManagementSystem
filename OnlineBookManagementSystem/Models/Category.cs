﻿using System;
using System.Collections.Generic;

namespace OnlineBookManagementSystem.Models;

public partial class Category
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
