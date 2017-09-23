using System;
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

		private readonly string ICELANDIC = "IS";
		private readonly string ENGLISH = "EN";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="languageHeader">The header to parse. Example: "en-US, en; q=0.8, is; q=0.6"</param>
		/// <returns></returns>
		private string parseLanguage(string languageHeader)
		{
			// If head is null then we use Icelandic
			if (languageHeader == null)
			{
				return ICELANDIC;
			}
			// Remove all white space and split string upp on ;
			languageHeader = languageHeader.Replace(" ", "");
			char[] semi = { ';' };
			string[] list = languageHeader.Split(semi);

			// Make sorted list to sort langues after the q values
			SortedList<string, string> languagesSorted = new SortedList<string, string>();

			for (int i = 0; i < list.Length; i++)
			{
				var language = "";
				var q = "";
				if (list[i].StartsWith("q"))
				{
					// Get the language code if there is one
					if (list[i].Contains(","))
					{
						language = list[i].Split(',')[1];
						list[i] = list[i].Split(',')[0];
					}
					// Get the nubmer value of the q
					if (list[i].Contains("="))
					{
						q = list[i].Split('=')[1];
					}
					// q is put in as key and the language is put in as value
					languagesSorted.Add(q, language);
				}
			}
			// The sorted list is checked in reverse order because the lowest value of q is first
			for (int i = languagesSorted.Count - 1; i >= 0; i--)
			{
				// If the value is IS we return Icelandic
				if (languagesSorted.ElementAt(i).Value.ToUpper() == "IS")
				{
					return ICELANDIC;

				}
				// If the value is EN we return English
				else if (languagesSorted.ElementAt(i).Value.ToUpper() == "EN")
				{
					return ENGLISH;
				}
			}

			// If we did not find English or Icelandic before then we return Icelandic as default
			return ICELANDIC;
		}

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
			if (isTeacherInCourse != null)
			{
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
		public List<CourseInstanceDTO> GetCourseInstancesBySemester(string semester = null, string languageHeader = null)
		{
			string language;
			if (string.IsNullOrEmpty(semester))
			{
				semester = "20153";
			}
			language = parseLanguage(languageHeader);

			Console.WriteLine(language);


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
			foreach (var c in courses)
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
