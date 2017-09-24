using System;
using System.Collections.Generic;

namespace CoursesAPI.Models
{
	public class Envelope<T>
	{
		public IEnumerable<T> Items  { get; set; }
		public int PageSize { get; set; }
		public int CurrentPage { get; set; }
		public int TotalPages { get; set; }
		
	}
}