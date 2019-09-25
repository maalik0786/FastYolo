using System.Collections.Generic;
using System.IO;

namespace FastYolo
{
	internal class YoloObjectTypeResolver
	{
		private readonly Dictionary<int, string> _objectType = new Dictionary<int, string>();

		public YoloObjectTypeResolver(string namesFilename)
		{
			var lines = File.ReadAllLines(namesFilename);
			Initialize(lines);
		}

		public YoloObjectTypeResolver(string[] objectTypes)
		{
			Initialize(objectTypes);
		}

		private void Initialize(string[] objectTypes)
		{
			for (var i = 0; i < objectTypes.Length; i++) _objectType.Add(i, objectTypes[i]);
		}

		public string Resolve(int objectId)
		{
			if (!_objectType.TryGetValue(objectId, out var objectType)) return "unknown key";

			return objectType;
		}
	}
}