using System.Runtime.InteropServices;

namespace FastYolo.Model;

[StructLayout(LayoutKind.Sequential)]
public struct BboxContainer
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = YoloWrapper.MaxObjects)]
	internal BboxT[] candidates;
}