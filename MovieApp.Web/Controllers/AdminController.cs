using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApp.Web.Data;
using MovieApp.Web.Entity;
using MovieApp.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MovieApp.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly MovieContext _context;
        public AdminController(MovieContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult MovieList()
        {
            return View(new AdminMoviesViewModel
            {
                Movies = _context.Movies.Include(m => m.Genres)
                .Select(m => new AdminMovieViewModel // select sorgusunda istediğimiz alanlar olacak.
                {
                    MovieId = m.MovieId,
                    Title = m.Title,
                    ImageUrl = m.ImageUrl,
                    Genres = m.Genres.ToList()
                })
                .ToList()
            });
        }
        public IActionResult MovieUpdate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = _context.Movies.Select(m => new AdminEditMovieViewModel // gelen id ile eşleşen veriyi viewModel e aktarıp view e gönderiyoruz.
            {
                MovieId = m.MovieId,
                Title = m.Title,
                Description = m.Description,
                ImageUrl = m.ImageUrl,
                GenreIds = m.Genres.Select(i=>i.GenreId).ToArray()
            }).FirstOrDefault(m => m.MovieId == id);

            ViewBag.Genres = _context.Genres.ToList();

            if (entity == null)
            {
                return NotFound();
            }
            return View(entity);
        }
        [HttpPost]
        public async Task<IActionResult> MovieUpdate(AdminEditMovieViewModel model, int[] genreIds, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                var entity = _context.Movies.Include("Genres").FirstOrDefault(m => m.MovieId == model.MovieId);
                //Genre bilgilerini de alıyoruz
                if (entity == null)
                {
                    return NotFound();
                }

                entity.Title = model.Title;
                entity.Description = model.Description;
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    var fileName = string.Format($"{Guid.NewGuid()}{extension}");//.jpg, .png
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img", fileName);
                    entity.ImageUrl = fileName;

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
                entity.Genres = genreIds.Select(id => _context.Genres.FirstOrDefault(i => i.GenreId == id)).ToList();
                //genreIds dizisinden gelen her bir id bilgisi için veritabanına select sorgusu göndereceğiz ve bunu Genres listesine dahil edeceğiz.

                _context.SaveChanges();

                return RedirectToAction("MovieList");
            }
            ViewBag.Genres = _context.Genres.ToList();
            return View(model);
        }
        private AdminGenresViewModel GetGenres()
        {
            return new AdminGenresViewModel
            {
                Genres = _context.Genres.Select(g => new AdminGenreViewModel
                {
                    GenreId = g.GenreId,
                    Name = g.Name,
                    Count = g.Movies.Count
                }).ToList()
            };
        }
        public IActionResult GenreList()
        {
            return View(GetGenres());
        }
        [HttpPost]
        public IActionResult GenreCreate(AdminGenresViewModel model)
        {
            if (model.Name!=null&& model.Name.Length<3)
            {
                ModelState.AddModelError("Name", "tür adi minimum 3 karakterli olmalıdır");
            }
            if (ModelState.IsValid)
            {
                _context.Genres.Add(new Genre { Name = model.Name });
                _context.SaveChanges();
                return RedirectToAction("GenreList");
            }
            return View("GenreList", GetGenres());
        }
        public IActionResult GenreUpdate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = _context.Genres
                .Select(g => new AdminGenreEditViewModel
                {
                    GenreId = g.GenreId,
                    Name = g.Name,
                    Movies = g.Movies.Select(i => new AdminMovieViewModel // tür ile o türe ait olan filmleri de getiriyoruz
                    {
                        MovieId = i.MovieId,
                        Title = i.Title,
                        ImageUrl = i.ImageUrl
                    }).ToList()
                })
                .FirstOrDefault(g => g.GenreId == id);

            if (entity == null)
            {
                return NotFound();
            }

            return View(entity);
        }
        [HttpPost]
        public IActionResult GenreUpdate(AdminGenreEditViewModel model, int[] movieIds)
        {
            if (ModelState.IsValid)
            {
                var entity = _context.Genres.Include("Movies").FirstOrDefault(i => i.GenreId == model.GenreId);
                if (entity == null)
                {
                    return NotFound();
                }

                entity.Name = model.Name;
                foreach (var id in movieIds)
                {
                    entity.Movies.Remove(entity.Movies.FirstOrDefault(m => m.MovieId == id)); //türe ait film ilişkisini kesiyor.
                }
                _context.SaveChanges();

                return RedirectToAction("GenreList");
            }
            return View(model);
        }
        [HttpPost]
        public IActionResult GenreDelete(int genreId)
        {
            var entity = _context.Genres.Find(genreId);
            if (entity != null)
            {
                _context.Genres.Remove(entity);
                _context.SaveChanges();
            }

            return RedirectToAction("GenreList");
        }
        [HttpPost]
        public IActionResult MovieDelete(int movieId)
        {
            var entity = _context.Movies.Find(movieId);
            if (entity != null)
            {
                _context.Movies.Remove(entity);
                _context.SaveChanges();
            }

            return RedirectToAction("MovieList");
        }
        public IActionResult MovieCreate()
        {
            ViewBag.Genres = _context.Genres.ToList();
            return View(new AdminCreateMovieModel());
        }
        [HttpPost]
        public IActionResult MovieCreate(AdminCreateMovieModel model)
        {
            if (model.Title != null && model.Title.Contains("@"))
            {
                //ModelState.AddModelError("Title", "Film Başlığı @ ,işareti içeremez.");//hata title alanının altında yazar.
                ModelState.AddModelError("Title", "Film Başlığı @ ,işareti içeremez.");
            }
            //if (model.GenreIds==null)
            //{
            //    ModelState.AddModelError("GenreIds", "En az bir tür seçmelisiniz.");
            //}
            if (ModelState.IsValid)
            {
                var entity = new Movie
                {
                    Title = model.Title,
                    Description = model.Description,
                    ImageUrl = "no-image.png"
                };
                //entity.Genres = new List<Genre>();
                foreach (var id in model.GenreIds)
                {
                    entity.Genres.Add(_context.Genres.FirstOrDefault(i => i.GenreId == id));// movie tablosuna ilişkili olan tür bilgisini de ekliyoruz.
                }
                _context.Movies.Add(entity);
                _context.SaveChanges();

                return RedirectToAction("MovieList", "Admin");
            }

            ViewBag.Genres = _context.Genres.ToList();
            return View(model);
        }
    }
}
