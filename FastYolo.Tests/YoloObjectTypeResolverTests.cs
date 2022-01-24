using NUnit.Framework;

namespace FastYolo.Tests;

public sealed class YoloObjectTypeResolverTests
{
	[Test]
	public void ResolveObjectType() =>
		Assert.That(
			new YoloObjectTypeResolver(YoloConfigurationTests.YoloClassesFilename).Resolve(0),
			Is.EqualTo("person"));
}