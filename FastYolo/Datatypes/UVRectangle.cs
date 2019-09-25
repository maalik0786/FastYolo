using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Encapsulates a user-specified UV for rendering. Values are in UV space (0-1). Rectangles are
	///   centered around their center position and use screen space top/bottom (which is inverted here)
	///   Used for Image.AtlasUV and SpriteSheetAnimation to hold each entities current animation sub UV
	///   rectangle. Can be animated as well for UV Moving, ProgressBars and various other effects, but
	///   be careful when AtlasUV (most images and fonts) is used as moving will go into neighboring UVs.
	/// </summary>
	[DebuggerDisplay("UV(Left={Left}, Top={Top}, Width={Width}, Height={Height})")]
	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	public struct UVRectangle : IEquatable<UVRectangle>, LerpWithParent<UVRectangle>
	{
		public UVRectangle(float left, float top, float width, float height) : this()
		{
			Left = left;
			Top = top;
			Width = width;
			Height = height;
		}

		[Pure] public float Left { get; }
		[Pure] public float Top { get; }
		[Pure] public float Width { get; }
		[Pure] public float Height { get; }

		public UVRectangle(string uvAsString) : this()
		{
			var components = uvAsString.SplitIntoFloats();
			if (components.Length != 4)
				throw new InvalidNumberOfDatatypeComponents<UVRectangle>();
			Left = components[0];
			Top = components[1];
			Width = components[2];
			Height = components[3];
		}

		[Pure] public Vector2D TopLeft => new Vector2D(Left, Top);
		[Pure] public Vector2D TopRight => new Vector2D(Left + Width, Top);
		[Pure] public Vector2D BottomLeft => new Vector2D(Left, Top + Height);
		[Pure] public Vector2D BottomRight => new Vector2D(Left + Width, Top + Height);
		[Pure] public Size Size => new Size(Width, Height);
		public static readonly UVRectangle One = new UVRectangle(0, 0, 1, 1);
		public static readonly UVRectangle HalfCentered = new UVRectangle(0.25f, 0.25f, 0.5f, 0.5f);

		/// <summary>
		///   Converts UVs from pixel space to 0-1 UV space. For the imagePixelSize it is very important
		///   to use the actual image pixel size for correct scaling and not the full atlas, which will be
		///   applied later in SpriteRenderer.SetOptionalDataWithSpriteParent.
		/// </summary>
		public static UVRectangle CreateUVFromPixelUVAndPixelSize(UVRectangle uvInPixels,
			Size imagePixelSize)
		{
			return new UVRectangle(uvInPixels.Left / imagePixelSize.Width,
				uvInPixels.Top / imagePixelSize.Height,
				Math.Min(1.0f, uvInPixels.Width / imagePixelSize.Width),
				Math.Min(1.0f, uvInPixels.Height / imagePixelSize.Height));
		}

		[Pure]
		public UVRectangle GetInnerUV(UVRectangle innerUV)
		{
			return new UVRectangle(Left + innerUV.Left * Width, Top + innerUV.Top * Height,
				Width * innerUV.Width, Height * innerUV.Height);
		}

		[Pure]
		public UVRectangle Lerp(UVRectangle other, float interpolation, UVRectangle parentOffset)
		{
			return new UVRectangle(Left.Lerp(other.Left, interpolation) + parentOffset.Left,
				Top.Lerp(other.Top, interpolation) + parentOffset.Top,
				Width.Lerp(other.Width, interpolation) + parentOffset.Width,
				Height.Lerp(other.Height, interpolation) + parentOffset.Height);
		}

		[Pure]
		public UVRectangle Lerp(UVRectangle other, float interpolation)
		{
			return new UVRectangle(Left.Lerp(other.Left, interpolation),
				Top.Lerp(other.Top, interpolation), Width.Lerp(other.Width, interpolation),
				Height.Lerp(other.Height, interpolation));
		}

		[Pure]
		public UVRectangle Flip(bool flipHorizontal, bool flipVertical)
		{
			return flipHorizontal && flipVertical
				? new UVRectangle(Left + Width, Top + Height, -Width, -Height)
				: flipHorizontal
					? new UVRectangle(Left + Width, Top, -Width, Height)
					: flipVertical
						? new UVRectangle(Left, Top + Height, Width, -Height)
						: this;
		}

		/// <summary>
		///   Move the UV rectangle by the given offset, Y offset value is inverted because textures are
		///   top down (positive goes down) and our screenspace is bottom up (positive goes up).
		/// </summary>
		[Pure]
		public UVRectangle Move(Vector2D movePositionBy)
		{
			return new UVRectangle(Left + movePositionBy.X, Top - movePositionBy.Y, Width, Height);
		}

		[Pure]
		public bool Equals(UVRectangle other)
		{
			return Left.IsNearlyEqual(other.Left) && Top.IsNearlyEqual(other.Top) &&
			       Width.IsNearlyEqual(other.Width) && Height.IsNearlyEqual(other.Height);
		}

		[Pure]
		public override bool Equals(object other)
		{
			return other is UVRectangle ? Equals((UVRectangle) other) : base.Equals(other);
		}

		[Pure]
		public static bool operator ==(UVRectangle uv1, UVRectangle uv2)
		{
			return uv1.Equals(uv2);
		}

		[Pure]
		public static bool operator !=(UVRectangle uv1, UVRectangle uv2)
		{
			return !uv1.Equals(uv2);
		}

		[Pure]
		public static UVRectangle operator *(UVRectangle uv, Size sizeToScaleTo)
		{
			return new UVRectangle(uv.Left * sizeToScaleTo.Width, uv.Top * sizeToScaleTo.Height,
				uv.Width * sizeToScaleTo.Width, uv.Height * sizeToScaleTo.Height);
		}

		[Pure]
		public static UVRectangle operator /(UVRectangle uv, Size sizeToDivide)
		{
			return new UVRectangle(uv.Left / sizeToDivide.Width, uv.Top / sizeToDivide.Height,
				uv.Width / sizeToDivide.Width, uv.Height / sizeToDivide.Height);
		}

		[Pure]
		public Vector2D GetInnerPoint(Vector2D relativePoint)
		{
			return new Vector2D(Left + relativePoint.X * Width, Top + relativePoint.Y * Height);
		}

		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				return Left.GetHashCode() * 631 + Top.GetHashCode() * 127 + Width.GetHashCode() * 19 +
				       Height.GetHashCode();
			}
		}

		[Pure]
		public override string ToString()
		{
			return "(" + Left.ToInvariantString() + ", " + Top.ToInvariantString() + ", " +
			       Width.ToInvariantString() + ", " + Height.ToInvariantString() + ")";
		}
	}
}