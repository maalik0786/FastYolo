using System;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Default implementation of Randomizer, returns quick random integers and floats (faster than
	///   System.Random). See http://www.codeproject.com/Articles/25172/Simple-Random-Number-Generation
	///   which is based on: http://www.bobwheeler.com/statistics/Password/MarsagliaPost.txt
	/// </summary>
	public class PseudoRandom : Randomizer
	{
		private uint w;
		private uint z;

		public PseudoRandom(long seed = 0)
		{
			if (seed == 0)
				seed = Environment.TickCount;
			var u = (uint) (seed >> 16);
			var v = (uint) (seed % 4294967296);
			w = u == 0 ? 521288629 : u;
			z = v == 0 ? 362436069 : v;
		}

		// ReSharper disable once MethodNameNotMeaningful
		public override int Get(int min, int max)
		{
			return (int) (GetNextDouble() * ((uint) max - (uint) min) + min);
		}

		private double GetNextDouble()
		{
			return (GetUint() + 1.0) * 2.328306435454494e-10;
		}

		private uint GetUint()
		{
			z = 36969 * (z & 65535) + (z >> 16);
			w = 18000 * (w & 65535) + (w >> 16);
			return (z << 16) + w;
		}

		// ReSharper disable once MethodNameNotMeaningful
		public override float Get(float min = 0.0f, float max = 1.0f)
		{
			return (float) (GetNextDouble() * (max - min) + min);
		}
	}
}