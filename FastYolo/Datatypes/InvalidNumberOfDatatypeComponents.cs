using System;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	public class InvalidNumberOfDatatypeComponents : Exception
	{
		public InvalidNumberOfDatatypeComponents(Type datatype, string datatypeAsString = null)
			: base(datatype.GetNameWithGenericComponents() + (datatypeAsString == null
				       ? string.Empty
				       : $" Components: '{datatypeAsString}'"))
		{
		}
	}

	public class InvalidNumberOfDatatypeComponents<Datatype> : InvalidNumberOfDatatypeComponents
	{
		public InvalidNumberOfDatatypeComponents() : base(typeof(Datatype))
		{
		}

		public InvalidNumberOfDatatypeComponents(string datatypeAsString)
			: base(typeof(Datatype), datatypeAsString)
		{
		}
	}
}