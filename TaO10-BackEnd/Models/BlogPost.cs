using System;
using System.Collections.Generic;

namespace TaO10_BackEnd.Models;

public partial class BlogPost
{
    public Guid BlogPostId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public string? Meta { get; set; }

    public int? ViewsCount { get; set; }

    public Guid StatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public virtual Status Status { get; set; } = null!;
}
