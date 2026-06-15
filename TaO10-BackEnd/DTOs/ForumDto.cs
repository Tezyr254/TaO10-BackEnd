using System;
using System.Collections.Generic;

namespace TaO10_BackEnd.DTOs.Forum
{
    public class ForumCategoryResponse
    {
        public Guid ForumCategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? ThreadsCount { get; set; }
        public int? RepliesCount { get; set; }
        public string? Badge { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class ForumThreadResponse
    {
        public Guid ForumThreadId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Excerpt { get; set; }
        public bool IsPinned { get; set; }
        public bool IsHot { get; set; }
        public List<string> Tags { get; set; } = new();
        public int ViewsCount { get; set; }
        public int RepliesCount { get; set; }
        public string Status { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public Guid AuthorId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ForumReplyResponse
    {
        public Guid ForumReplyId { get; set; }
        public Guid ForumThreadId { get; set; }
        public string Content { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public Guid AuthorId { get; set; }
        public string? AuthorAvatar { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateThreadRequest
    {
        public Guid ForumCategoryId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public List<string> Tags { get; set; } = new();
    }

    public class CreateReplyRequest
    {
        public string Content { get; set; } = null!;
    }
}
