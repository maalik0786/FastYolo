using System.Collections.Generic;
using NUnit.Framework;

namespace FastYolo.Tests
{
	public class YoloObjectTypeResolverTests
	{
		[Test]
		public void CreateDictObject()
		{
			var dict = new Dictionary<int, string>();
			new YoloObjectTypeResolver(YoloConfigurationTests.YoloClassesFilename).ObjectType = dict;
		}

		[Test]
		public void ResolveObjectType() =>
			new YoloObjectTypeResolver(YoloConfigurationTests.YoloClassesFilename).Resolve(0);
	}
}
