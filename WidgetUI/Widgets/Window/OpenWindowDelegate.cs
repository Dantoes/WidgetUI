using UnityEngine;
using UnityEngine.EventSystems;

namespace WidgetUI
{
	[ExecuteInEditMode]
	public class OpenWindowDelegate
		: UIBehaviour
	{
		[SerializeField]
		private Window m_windowPrefab;
		private Window m_windowInstance = null;

		[SerializeField]
		private WidgetSize m_windowSize;

		[Space(20)]
		[SerializeField]
		private GameObject m_contentPrefab;

		[SerializeField]
		private WidgetSize m_contentSize;

		[Space(20)]
		[SerializeField]
		private RectTransform m_parent;

		[SerializeField]
		private bool m_centerOnScreen;

		protected override void Awake()
		{
			base.Awake();

			if(m_parent == null)
			{
				// try to find the main canvas
				Canvas parentCanvas = this.transform.GetComponentInParent<Canvas>();
				if(parentCanvas != null)
				{
					m_parent = parentCanvas.GetComponent<RectTransform>();
				}
				else
				{
					m_parent = this.GetComponent<RectTransform>();
				}
			}
		}

		public void Open()
		{
			WindowBuilder allocator = new WindowBuilder(m_windowPrefab, m_contentPrefab, m_parent);
			allocator.WindowSize = m_windowSize;
			allocator.ContentSize = m_contentSize;

			m_windowInstance = allocator.Construct();

			if(m_centerOnScreen)
			{
				m_windowInstance.CenterOnScreen();
			}
		}

		public void Close()
		{
			if(m_windowInstance != null)
			{
				m_windowInstance.Close();
				m_windowInstance = null;
			}
		}

		public void Toggle()
		{
			if(m_windowInstance == null)
			{
				this.Open();
			}
			else
			{
				this.Close();
			}
		}
	}
}
