using Portfolio.Domain.Enums;

namespace Portfolio.Application.Blogs.DTOs;

public sealed class BlogCategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int BlogCount { get; init; }
}

public class BlogCategoryCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class BlogCategoryUpdateRequest : BlogCategoryCreateRequest
{
}

public sealed class BlogFilterRequest
{
    public string? Keyword { get; set; }
    public BlogStatus? Status { get; set; }
    public int? CategoryId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class BlogDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string? ThumbnailUrl { get; init; }
    public BlogStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public DateTime? PublishedAt { get; init; }
    public int ViewCount { get; init; }
    public bool IsFeatured { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

public sealed class BlogDetailDto : BlogDto
{
    public string Content { get; init; } = string.Empty;
}

public class BlogCreateRequest
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public IReadOnlyCollection<string> Tags { get; set; } = Array.Empty<string>();
}

public sealed class BlogUpdateRequest : BlogCreateRequest
{
}
