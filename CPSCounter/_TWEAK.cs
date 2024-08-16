using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CPSCounter
{
	#region shall not touch if not a programmer

	public class ClickingMethod
	{
		public int min_ms = 0;
		public int max_ms = 0;

		public int min_cps = 0;
		public int max_cps = 0;

		public string name = string.Empty;

		public ClickingMethod(
			string name = "",
			int min_ms = 0,	 		// minimum milliseconds betweent 2 last clicks
			int max_ms = 0,	 		// maximum
			int min_cps = 0,		// minimum clicks per second
			int max_cps = 0         // maximum
		)
		{
			this.name = name;
			this.min_ms = min_ms;
			this.max_ms = max_ms;
			this.min_cps = min_cps;
			this.max_cps = max_cps;
		}
	}

	#endregion


	public static class click_method_detection
    {

        #region CUSTOMIZE HERE


        public static ClickingMethod[] methods = {
			new ClickingMethod(
				name: "Normal click",
				min_ms: 125,
				max_ms: 1000,
				min_cps: 1,
				max_cps: 8
			),

			new ClickingMethod(
				name: "Double click",
				min_ms: 8,
				max_ms: 250,
				min_cps: 1,
				max_cps: 2
			),

			new ClickingMethod(
				name: "Triple click",
				min_ms: 8,
				max_ms: 250,
				min_cps: 2,
				max_cps: 3
			),

			new ClickingMethod(
				name: "Short drag click",
				min_ms: 5,
				max_ms: 40,
				min_cps: 4,
				max_cps: 12
			),

			new ClickingMethod(
				name: "Medium drag click",
				min_ms: 6,
				max_ms: 60,
				min_cps: 13,
				max_cps: 25
			),

			new ClickingMethod(
				name: "Long drag click",
				min_ms: 6,
				max_ms: 50,
				min_cps: 26,
				max_cps: 128
			),

			new ClickingMethod(
				name: "Butterfly click",
				min_ms: 50,
				max_ms: 150,
				min_cps: 8,
				max_cps: 20
			),

			new ClickingMethod(
				name: "Bolt click / unknown",
				min_ms: 0,
				max_ms: 15,
				min_cps: 66,
				max_cps: 32767
			),

			// template

			new ClickingMethod(
				name: "pjaty click",

				min_ms: 1,		// minimum milliseconds between 2 last clicks
				max_ms: 0,		// maximum

				min_cps: 1,		// minimum clicks per second
				max_cps: 0		// maximum
			),
		};

        #endregion

        #region shall not touch if not a programmer

        // Function to find the first ClickingMethod object that complies with the provided min/max values

        public static string FindClickingMethod(List<long> list, Stopwatch millis, string default_return = "")
        {
			int cps = list.Count;
            foreach (var method in methods)
            {
                if (list.Any(ms => millis.ElapsedMilliseconds - ms >= method.min_ms && millis.ElapsedMilliseconds - ms <= method.max_ms) &&
                    cps >= method.min_cps && cps <= method.max_cps)
                {
                    return method.name;
                }
            }
            return default_return;
        }



        #endregion
    }
}
