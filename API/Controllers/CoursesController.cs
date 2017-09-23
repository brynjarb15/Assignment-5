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

        [HttpGet]
        public IActionResult GetCoursesBySemester(string semester = null)
        {
            // TODO: figure out the requested language (if any!)
            // and pass it to the service provider!
            return Ok(_service.GetCourseInstancesBySemester(semester));
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
