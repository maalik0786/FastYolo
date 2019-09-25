using System.Diagnostics.Contracts;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Interpolates RenderData with parent renderable. Can also be used to simulate a simple Add
	///   method callable from any generic type implementing it. Passing in value as other and 0 for
	///   interpolation increases the value like the += operator does, used in RenderDataArray.
	/// </summary>
	public interface LerpWithParent<T> : Lerp<T>
	{
		[Pure]
		T Lerp(T other, float interpolation, T parentOffset);
	}
}