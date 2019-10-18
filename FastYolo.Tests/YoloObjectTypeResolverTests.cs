using System;
using System.Collections.Generic;
using FastYolo;
using NUnit.Framework;

namespace FastYolo.Tests
{
	public class YoloObjectTypeResolverTests
	{
#if WIN64
		private const string YoloServerDirectory = @"\\DeltaServer\Shared\yolo-v3-tiny\";
#else
		private const string YoloServerDirectory = "/home/dev/Documents/yolo-v3-tiny/";
#endif
		private const string YoloClassesFilename = YoloServerDirectory + "classes.names";

		[Test]
		public void CreateDictObject()
		{
			var dict = new Dictionary<int, string>();
			new YoloObjectTypeResolver(YoloClassesFilename).ObjectType = dict;
		}

		[Test]
		public void ResolveObjectType() => new YoloObjectTypeResolver(YoloClassesFilename).Resolve(0);
	}
}
