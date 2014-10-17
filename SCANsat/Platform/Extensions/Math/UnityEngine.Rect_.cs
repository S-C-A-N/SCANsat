using System;

namespace UnityEngine
{
	public static class Rect_ {
		
		public static Rect ClampToScreen(this Rect r) { return r.ClampToScreen(new RectOffset(0, 0, 0, 0)); }
		
		public static Rect ClampToScreen(this Rect r, RectOffset ScreenBorder) {
			r.x = Mathf.Clamp(r.x, ScreenBorder.left, Screen.width - r.width - ScreenBorder.right);
			r.y = Mathf.Clamp(r.y, ScreenBorder.top, Screen.height - r.height - ScreenBorder.bottom);
			return r;
		}
		
		public static string WxH(this Rect r) { return string.Format ("[{0}x{1}]",(int)r.width,(int)r.height); }
	}
}

