using UnityEngine;

namespace WidgetUI
{
	public class HelpBox : PropertyAttribute
	{
		public readonly string message;

		public HelpBox(string p_message)
		{
			this.message = p_message;
		}
	}
}
