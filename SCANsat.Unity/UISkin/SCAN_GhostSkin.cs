using System;
using UnityEngine;

namespace SCANsat.Unity.UISkin
{
	public class SCAN_GhostSkin : MonoBehaviour
	{
		[SerializeField]
		private SCAN_UISkinDef m_GhostSkin = new SCAN_UISkinDef();

		public SCAN_UISkinDef GhostSkin
		{
			get { return m_GhostSkin; }
		}
	}
}
