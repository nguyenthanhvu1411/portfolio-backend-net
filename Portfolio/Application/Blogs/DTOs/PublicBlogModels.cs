namespace Portfolio.Application.Blogs.DTOs;

public sealed class PublicBlogFilterRequest
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public string? Tag { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public sealed class PublicBlogTagDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
}

public sealed class PublicBlogCategoryDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }

    public int PublishedBlogCount { get; init; }
}

public class PublicBlogCardDto
{
    public int Id { get; init; }

    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string CategorySlug { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;

    public string? Summary { get; init; }
    public string? ThumbnailUrl { get; init; }

    public DateTime? PublishedAt { get; init; }
    public int ViewCount { get; init; }
    public bool IsFeatured { get; init; }

    public IReadOnlyList<PublicBlogTagDto> Tags { get; init; } =
        Array.Empty<PublicBlogTagDto>();
}

public sealed class PublicBlogDetailDto : PublicBlogCardDto
{
    public string Content { get; init; } = string.Empty;
}

