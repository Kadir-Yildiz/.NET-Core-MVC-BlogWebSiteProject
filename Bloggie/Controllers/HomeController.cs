using Bloggie.Data;
using Bloggie.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Bloggie.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
        }
        [Route("")]
        [Route("c/{slug}")]
        public IActionResult Index(string slug, int pageNo=1)
        {
            IQueryable<Post> posts = _db.Posts.Include(x => x.Author).Where(x => slug == null || x.Category.Slug == slug).Where(x => !x.IsDraft);
            int postCount = posts.Count();
            int pageCount = (int)Math.Ceiling((double)postCount / Constants.POSTS_PER_PAGE);

            var vm = new HomeViewModel()
            {
                Categories = _db.Categories.OrderBy(x => x.Name).ToList(),
                Posts = posts.OrderByDescending(x => x.CreatedTime).Skip((pageNo-1)*Constants.POSTS_PER_PAGE).Take(Constants.POSTS_PER_PAGE).ToList(),
                PageNo = pageNo,
                HasNewer = pageNo >1,
                HasOlder = pageNo < pageCount,
            };
           
            return View(vm);
        }
        [Route("p/{slug}")]
        public IActionResult Post(string slug)
        {
            
            var post = _db.Posts.Include(x=> x.Author).Where(x => !x.IsDraft).FirstOrDefault(x => x.Slug == slug);

            ViewBag.Comments = _db.Comments.Where(c => c.Post == post).ToList();
            if (post == null) return NotFound();
            
            return View(post);
        }
        

        [HttpPost]
        public async Task<ActionResult> AddComment(Post post)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var applicationUser = await _userManager.GetUserAsync(User);
            Comment comment = new Comment();
            var posttemp = _db.Posts
                 .Include(x => x.Author)
                 .Where(x => !x.IsDraft)
                 .FirstOrDefault(x => x.Id == post.Id);
            comment.Post = posttemp;
            comment.Content = post.CommentContent;
            comment.Author = applicationUser;
            _db.Comments.Add(comment);
            _db.SaveChanges();
            ViewBag.Comments = _db.Comments.Where(c => c.Post == post).Include(x=> x.Author.DisplayName).ToList();
            return RedirectToAction("Post", new {slug=comment.Post.Slug});
            

        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}