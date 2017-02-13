using System.ComponentModel.DataAnnotations;
using Apache.Ignite.Core.Cache.Configuration;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace IgniteEFCacheStore
{
    /// <summary>
    /// The blog.
    /// </summary>
    public class Blog
    {
        /// <summary>
        /// Gets or sets the blog id.
        /// </summary>
        [Key]
        public int BlogId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// The post.
    /// </summary>
    public class Post
    {
        /// <summary>
        /// Gets or sets the post id.
        /// </summary>
        [Key]
        [QuerySqlField(IsIndexed = true)]
        [IgniteCacheParam]
        public int PostId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        [QuerySqlField]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the blog id.
        /// </summary>
        [QuerySqlField]
        public int BlogId { get; set; }

        [QuerySqlField]
        public bool IsRequired { get; set; }
    }
}
