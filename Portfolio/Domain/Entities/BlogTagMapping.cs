namespace Portfolio.Domain.Entities
{
    public class BlogTagMapping
    {
        public int BlogId { get; set; }
        public Blog Blog { get; set; } = null!;

        public int TagId { get; set; }
        public BlogTag Tag { get; set; } = null!;
    }
}
