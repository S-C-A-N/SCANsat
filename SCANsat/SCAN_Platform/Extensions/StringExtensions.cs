using KSP.Localization;
using SCANsat.SCAN_Map;

namespace SCANsat
{
	public static class StringExtensions
	{
		public static string LocalizeBodyName(this string input)
		{
			return Localizer.Format("<<1>>", input);
		}

		public static string LocalizeMapType(this mapType input)
		{
			return Localizer.Format("#autoLOC_SCANsat_MapType_" + input);
		}
	}
}
