using System.IO;

namespace FastYolo.Extensions
{
	/// <summary>
	///   Non-cryptographic hash function suitable for general hash-based lookup. Unlike cryptographic
	///   hash functions, it is not specifically designed to be difficult to reverse by an adversary,
	///   making it unsuitable for cryptographic purposes.
	///   https://github.com/aappleby/smhasher/wiki/MurmurHash3
	///   https://en.wikipedia.org/wiki/MurmurHash
	/// </summary>
	public class MurmurHash
	{
		private const int DefaultSeed = 486187739;

		private const int ChunkSize = 4;

		private const uint C1 = 0xcc9e2d51;
		private const uint C2 = 0x1b873593;
		private const byte R1 = 15;
		private const byte R2 = 13;
		private const byte M = 5;
		private const uint N = 0xe6546b64;
		private readonly uint seed;

		public MurmurHash(int seed = DefaultSeed)
		{
			this.seed = (uint) seed;
		}

		public int ComputeHash(byte[] data)
		{
			using var stream = new MemoryStream(data);
			return ComputeHash(stream);
		}

		public int ComputeHash(Stream stream)
		{
			var hash = seed;
			uint streamLength = 0;
			using (var reader = new BinaryReader(stream))
			{
				var chunk = reader.ReadBytes(ChunkSize);
				while (chunk.Length > 0)
				{
					streamLength += (uint) chunk.Length;
					hash = ComputeChunkHash(chunk, hash);
					chunk = reader.ReadBytes(ChunkSize);
				}
			}

			return MixFinal(streamLength, hash);
		}

		// ReSharper disable once MethodTooLong
		private static uint ComputeChunkHash(byte[] chunk, uint hash)
		{
			uint key;
			switch (chunk.Length)
			{
				case 4:
					// ReSharper disable once ComplexConditionExpression
					key = (uint) (chunk[0] | (chunk[1] << 8) | (chunk[2] << 16) | (chunk[3] << 24));
					hash = MixBody(key, hash);
					break;
				case 3:
					key = (uint) (chunk[0] | (chunk[1] << 8) | (chunk[2] << 16));
					hash = MixTail(key, hash);
					break;
				case 2:
					key = (uint) (chunk[0] | (chunk[1] << 8));
					hash = MixTail(key, hash);
					break;
				case 1:
					key = chunk[0];
					hash = MixTail(key, hash);
					break;
			}

			return hash;
		}

		private static uint MixBody(uint key, uint hash)
		{
			key *= C1;
			key = RotateLeft(key, R1);
			key *= C2;
			hash ^= key;
			hash = RotateLeft(hash, R2);
			hash = hash * M + N;
			return hash;
		}

		private static uint RotateLeft(uint value, byte bits)
		{
			return (value << bits) | (value >> (sizeof(uint) * 8 - bits));
		}

		private static uint MixTail(uint key, uint hash)
		{
			key *= C1;
			key = RotateLeft(key, R1);
			key *= C2;
			hash ^= key;
			return hash;
		}

		private static int MixFinal(uint streamLength, uint hash)
		{
			hash ^= streamLength;
			hash ^= hash >> 16;
			hash *= 0x85ebca6b;
			hash ^= hash >> 13;
			hash *= 0xc2b2ae35;
			hash ^= hash >> 16;
			unchecked
			{
				return (int) hash;
			}
		}
	}
}