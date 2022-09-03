
using Bloggie.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Bloggie.Data
{
    [Index("Slug", IsUnique = true)]
    public class Category :ISlug
	{
		public int Id { get; set; }
		[Required, MaxLength(200)]
		public string Name { get; set; }
		public List<Post> Posts { get; set; }
        [Required, MaxLength(200)]
        public string Slug { get; set; } = Guid.NewGuid().ToString();

        public string GetSlugText()
        {
            return Name;
        }
    }
}
