using System;
using UnityEngine;

namespace WidgetUI
{
	public class WindowBuilder
		: IWidgetAllocator<Window>
	{
		GameObject m_windowPrefab;
		GameObject m_contentPrefab;
		RectTransform m_parent;

		public WidgetSize WindowSize { get; set; }
		public WidgetSize ContentSize { get; set; }
		public bool CenterOnScreen { get; set; }
		public WindowParams Params { get; set; }

		public WindowBuilder(Window p_windowPrefab, GameObject p_contentPrefab = null, RectTransform p_parent = null)
		{
#if UNITY_EDITOR
			if (p_windowPrefab == null)
			{
				throw new ArgumentNullException("WindowAllocator: Window prefab must not be null", "p_windowPrefab");
			}
#endif
			m_windowPrefab = p_windowPrefab.gameObject;
			m_contentPrefab = p_contentPrefab;
			m_parent = p_parent;
		}

		public virtual Window Construct()
		{
			// create a new window instance and get components
			GameObject windowObject = GameObject.Instantiate(m_windowPrefab) as GameObject;
			RectTransform windowTransform = windowObject.GetComponent<RectTransform>();
			Window windowComponent = windowObject.GetComponent<Window>();

			// set window parent and size
			if(m_parent != null)
			{
				windowTransform.SetParent(m_parent, false);
			}
			windowTransform.SetSize(this.WindowSize);

			// assign content and set size
			if(m_contentPrefab != null)
			{
				GameObject contentInstance = (GameObject)GameObject.Instantiate(m_contentPrefab);
				try
				{
					RectTransform contentTransform = windowComponent.SetContent(contentInstance, this.Params);
					contentTransform.SetSize(this.ContentSize);
				}
				catch
				{
					GameObject.Destroy(contentInstance);
					throw;
				}
			}
			return windowComponent;
		}

		public virtual void Destroy(Window p_widget)
		{
			p_widget.Close();
		}
	}
}
