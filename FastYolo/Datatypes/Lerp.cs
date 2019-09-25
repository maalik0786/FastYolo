using System.Diagnostics.Contracts;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Base interface to use the generic Lerp, do not use this interface on its own. Used to find
	///   out if some object implements the generic Lerp without having to go through reflection.
	/// </summary>
	public interface Lerp
	{
	}

	/// <summary>
	///   Forces datatypes derived from this to implement a Lerp method to interpolate between values.
	///   Most importantly used in EntityInstance to interpolate any lerpable component inclusive the
	///   parent component data (e.g. a 3D entity can change its position and updates its children)
	/// </summary>
	public interface Lerp<T> : Lerp
	{
		[Pure]
		T Lerp(T other, float interpolation);
	}
}