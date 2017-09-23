using System.Collections.Generic;
using System.Linq;
using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;

namespace CoursesAPI.Services.CoursesServices
{
	public class CoursesServiceProvider
	{
		private readonly IUnitOfWork _uow;

		private readonly IRepository<CourseInstance> _courseInstances;
		private readonly IRepository<TeacherRegistration> _teacherRegistrations;
		private readonly IRepository<CourseTemplate> _courseTemplates;
		private readonly IRepository<Person> _persons;

		public CoursesServiceProvider(IUnitOfWork uow)
		{
			_uow = uow;

			_courseInstances = _uow.GetRepository<CourseInstance>();
			_courseTemplates = _uow.GetRepository<CourseTemplate>();
			_teacherRegistrations = _uow.GetRepository<TeacherRegistration>();
			_persons = _uow.GetRepository<Person>();
		}

		/// <summary>
		/// You should implement this function, such that all tests will pass.
		/// </summary>
		/// <param name="courseInstanceID">The ID of the course instance which the teacher will be registered to.</param>
		/// <param name="model">The data which indicates which person should be added as a teacher, and in what role.</param>
		/// <returns>Should return basic information about the person.</returns>
		public PersonDTO AddTeacherToCourse(int courseInstanceID, AddTeacherViewModel model)
		{
			var course = (from c in _courseInstances.All()
						  where c.ID == courseInstanceID
						  select c).SingleOrDefault();
			if (course == null)
			{
				throw new AppObjectNotFoundException();
			}
			var newTeacher = (from t in _persons.All()
							  where t.SSN == model.SSN
							  select new PersonDTO
							  {
								  SSN = t.SSN,
								  Name = t.Name,
							  }).SingleOrDefault();
			if (newTeacher == null)
			{
				throw new AppObjectNotFoundException();
			}
			var isTeacherInCourse = (from t in _teacherRegistrations.All()
									 where t.SSN == newTeacher.SSN &&
									 	   t.CourseInstanceID == courseInstanceID
									select t).SingleOrDefault();
			if (isTeacherInCourse != null){
				throw new AppValidationException("The teacher is already registered to this course");
			}
			var isThereAMainTeacher = (from t in _teacherRegistrations.All()
									   where t.CourseInstanceID == courseInstanceID &&
											 t.Type == TeacherType.MainTeacher
									   select t).SingleOrDefault();
			if (isThereAMainTeacher != null && model.Type == TeacherType.MainTeacher)
			{
				throw new AppValidationException("There already is a main theacher in this course");
			}
			var teacherEntity = new TeacherRegistration
			{
				SSN = model.SSN,
				CourseInstanceID = courseInstanceID,
				Type = model.Type
			};
			_teacherRegistrations.Add(teacherEntity);
			_uow.Save();


			return newTeacher;
		}

		/// <summary>
		/// You should write tests for this function. You will also need to
		/// modify it, such that it will correctly return the name of the main
		/// teacher of each course.
		/// </summary>
		/// <param name="semester"></param>
		/// <returns></returns>
		public List<CourseInstanceDTO> GetCourseInstancesBySemester(string semester = null)
		{
			if (string.IsNullOrEmpty(semester))
			{
				semester = "20153";
			}

			var courses = (from c in _courseInstances.All()
						   join ct in _courseTemplates.All() on c.CourseID equals ct.CourseID
						   where c.SemesterID == semester
						   select new CourseInstanceDTO
						   {
							   Name = ct.NameIS,
							   TemplateID = ct.CourseID,
							   CourseInstanceID = c.ID,
							   MainTeacher = (from t in _persons.All()
										  join tr in _teacherRegistrations.All() on t.SSN equals tr.SSN
										  where tr.CourseInstanceID == c.ID &&
												tr.Type == TeacherType.MainTeacher
										  select t.Name).SingleOrDefault()
						   }).ToList();
			foreach(var c in courses)
			{
				if (c.MainTeacher == null)
				{
					c.MainTeacher = "";
				}
			}
			return courses;
		}
	}
}
