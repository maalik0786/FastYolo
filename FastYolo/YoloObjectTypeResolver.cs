using System.Collections.Generic;
using System.IO;

namespace FastYolo
{
	public class YoloObjectTypeResolver
	{
		public Dictionary<int, string> ObjectType = new Dictionary<int, string>();

		public YoloObjectTypeResolver(string namesFilename)
		{
			var lines = File.ReadAllLines(namesFilename);
			Initialize(lines);
		}

		private void Initialize(string[] objectTypes)
		{
			for (var i = 0; i < objectTypes.Length; i++) ObjectType.Add(i, objectTypes[i]);
		}

		public string Resolve(int objectId) => !this.ObjectType.TryGetValue(objectId, out var objectType) ? "unknown key" : objectType;
	}
}