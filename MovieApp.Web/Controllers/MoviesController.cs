using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieApp.Web.Data;
using MovieApp.Web.Entity;
using MovieApp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieApp.Web.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MovieContext _context;
        public MoviesController(MovieContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult List(int? id, string q)
        {
            var controller = RouteData.Values["controller"];
            var action = RouteData.Values["action"];
            var genreId = RouteData.Values["id"];

            var kelime = HttpContext.Request.Query["q"].ToString();

            //var movies = MovieRepository.Movies;
            var movies = _context.Movies.AsQueryable();
            if (id != null)
            {
                movies = movies
                    .Include(m => m.Genres)
                    .Where(m => m.Genres.Any(g=>g.GenreId==id));
            }

            if (!string.IsNullOrEmpty(q))
            {
                movies = movies.Where(i => i.Title.ToLower().Contains(q.ToLower()) || i.Description.ToLower().Contains(q.ToLower()));
            }

            var model = new MoviesViewModel()
            {
                Movies = movies.ToList()
            };
            return View("List", model);
        }

        public IActionResult Details(int id)
        {
            return View(_context.Movies.Find(id));
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Genres = new SelectList(_context.Genres.ToList(), "GenreId", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult Create(Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Movies.Add(movie);
                _context.SaveChanges();
                //  MovieRepository.Add(movie);
                TempData["Message"] = $"{movie.Title} isimli film eklendi.";

                return RedirectToAction("List");
            }
            ViewBag.Genres = new SelectList(_context.Genres.ToList(), "GenreId", "Name");
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {

            ViewBag.Genres = new SelectList(_context.Genres.ToList(), "GenreId", "Name");
            return View(_context.Movies.Find(id));
        }

        [HttpPost]
        public IActionResult Edit(Movie movie)
        {
            if (ModelState.IsValid)
            {
                //MovieRepository.Edit(movie);
                //movies/details/1
                _context.Movies.Update(movie);
                _context.SaveChanges();
                TempData["Message"] = $"{movie.Title} isimli film güncellendi.";

                return RedirectToAction("Details", "Movies", new { @id = movie.MovieId });
            }
            ViewBag.Genres = new SelectList(_context.Genres.ToList(), "GenreId", "Name");
            return View(movie);
        }

        [HttpPost]
        public IActionResult Delete(int MovieId, string Title)
        {
            //MovieRepository.Delete(MovieId);
            var entity = _context.Movies.Find(MovieId);
            _context.Movies.Remove(entity);
            _context.SaveChanges();
            TempData["Message"] = $"{Title} isimli film silindi.";
            return RedirectToAction("List");
        }
    }
}
