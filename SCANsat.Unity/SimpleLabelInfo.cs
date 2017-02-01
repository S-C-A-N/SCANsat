using UnityEngine;

namespace SCANsat.Unity
{
	public class SimpleLabelInfo
	{
		public Sprite image;
		public int width;
		public Vector2 pos;
		public Color color;
		public bool show;

		public SimpleLabelInfo(int w, Sprite img)
		{
			image = img;
			width = w;
		}
	}
}
