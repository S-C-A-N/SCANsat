using UnityEngine;

namespace SCANsat.Unity.Interfaces
{
	public interface ISCAN_Toolbar
	{
		Canvas TooltipCanvas { get; }

		bool TooltipsOn { get; }

		bool MainMap { get; set; }

		bool BigMap { get; set; }

		bool ZoomMap { get; set; }

		bool Overlay { get; set; }

		bool Instruments { get; set; }

		bool Settings { get; set; }

		bool InMenu { get; set; }

		float Scale { get; }
	}
}
