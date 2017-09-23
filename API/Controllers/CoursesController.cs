using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoursesAPI.Models;
using CoursesAPI.Services.CoursesServices;
using CoursesAPI.Services.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers
{
	[Route("api/courses")]
	public class CoursesController : Controller
	{
		private readonly CoursesServiceProvider _service;

		public CoursesController(IUnitOfWork uow)
		{
			_service = new CoursesServiceProvider(uow);
		}

		/// <summary>
		/// Returns list of all courses for the given semester
		/// </summary>
		/// <param name="semester"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("")]
		public IActionResult GetCoursesBySemester(string semester = null)
		{
			var languageHeader = Request.Headers["Accept-Language"];
			// TODO: figure out the requested language (if any!)
			// and pass it to the service provider!
			return Ok(_service.GetCourseInstancesBySemester(semester, languageHeader));
		}

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("{id}/teachers")]
		public IActionResult AddTeacher(int id, AddTeacherViewModel model)
		{
			var result = _service.AddTeacherToCourse(id, model);
			return Created("TODO", result);
		}
	}
}
