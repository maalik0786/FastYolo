using System;
using FastYolo.Model;

namespace FastYolo
{
	public sealed class FloatArray
	{
		public float[] GetYoloFloatArray(ColorImage imageData, in int channels)
		{
			var sizeInBytes = imageData.Width * imageData.Height * channels * sizeof(float);
			if (sizeInBytes != floatArray.Length)
				floatArray = new float[sizeInBytes];
			ForLoops(imageData, channels);
			return floatArray;
		}

		private float[] floatArray = Array.Empty<float>();
		private int counter;

		private void ForLoops(ColorImage imageData, int channels)
		{
			counter = 0;
			for (var channel = 0; channel < channels; channel++)
			for (var y = 0; y < imageData.Height; y++)
			for (var x = 0; x < imageData.Width; x++)
			{
				var color = imageData.Colors[x + y * imageData.Width];
				floatArray[counter++] = channel switch
				{
					0 => color.RedValue, 1 => color.GreenValue, 2 => color.BlueValue, _ => color.AlphaValue
				};
			}
		}
	}
}