using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class NoiseTest {

	[Test]
	public void NoiseTestSimplePasses() {
		// Given the same seed and same coordinates, simplex noise should return the same values always.
		// var noise = new SimplexNoiseGenerator("My testing seed");
		// var res = (int) (noise.coherentNoise(10, 0, 10) * 1000);
		// Assert.AreEqual(16, res);
	}
}
