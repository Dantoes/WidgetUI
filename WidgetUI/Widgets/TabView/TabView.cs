using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WidgetUI
{
	[SelectionBase]
	[RequireComponent(typeof(RectTransform))]
	public class TabView
		: UIBehaviour
		, IWidget
	{
		[Serializable]
		public struct Tab
		{
			public Button triggerButton;
			public GameObject prefab;
			public WidgetSize contentSize;
		}

		[Serializable]
		public class TabChangedEvent : UnityEvent<int>
		{
		}

		[SerializeField]
		protected RectTransform m_contentArea;

		[SerializeField]
		protected List<Tab> m_tabs;

		[SerializeField]
		protected int m_defaultTab;

		[SerializeField]
		protected TabChangedEvent m_onChangeTab = new TabChangedEvent();

		protected GameObject m_contentInstance;
		protected int m_activeTabId = -1;


		protected RectTransform ContentArea
		{
			get { return m_contentArea; }
			set { m_contentArea = value; }
		}

		public List<Tab> Tabs
		{
			get { return m_tabs; }
			set { m_tabs = value; }
		}

		public int DefaultTab
		{
			get { return m_defaultTab; }
			set { m_defaultTab = value; }
		}

		public TabChangedEvent OnChangeTab
		{
			get { return m_onChangeTab; }
			set { m_onChangeTab = value; }
		}

		public int ActiveTab
		{
			get
			{
				return m_activeTabId;
			}
			set
			{
				this.SetActiveTab(value);
			}
		}


		protected override void Awake()
		{
			base.Awake();

			for (int i = 0; i < m_tabs.Count; ++i)
			{
				Button button = m_tabs[i].triggerButton;
				if(button != null)
				{
					int index = i; // copy required for delegate
					button.onClick.AddListener(() => this.SetActiveTab(index));
				}
			}
		}

		protected override void Start()
		{
			base.Start();

			if (m_tabs.Count > 0)
			{
				if (m_defaultTab >= 0 && m_defaultTab < m_tabs.Count)
				{
					this.SetActiveTab(m_defaultTab);
				}
				else
				{
					Debug.LogWarning(String.Format("{0}: Default tab index is out of range", this.name));
				}
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.UnsetActiveTab();
		}

		public void SetActiveTab(int p_index)
		{
			if(p_index < 0 || p_index >= m_tabs.Count)
			{
				throw new ArgumentOutOfRangeException("p_index");
			}

			if(m_activeTabId == p_index)
			{
				return;
			}

			if (m_activeTabId >= 0)
			{
				this.ResetContent();
			}

			Tab activeTab = m_tabs[p_index];
			m_contentInstance = this.InstantiateTabContent(activeTab);
			m_activeTabId = p_index;
			m_onChangeTab.Invoke(m_activeTabId);
		}

		public void UnsetActiveTab()
		{
			if (m_activeTabId >= 0)
			{
				this.ResetContent();
				m_onChangeTab.Invoke(-1);
			}
		}

		public Tab GetTab(int p_index)
		{
			if (p_index < 0 || p_index >= m_tabs.Count)
			{
				throw new ArgumentOutOfRangeException("p_index");
			}

			return m_tabs[p_index];
		}

		protected virtual void ResetContent()
		{
			if (m_contentInstance != null)
			{
				GameObject.Destroy(m_contentInstance);
				m_contentInstance = null;
			}
			m_activeTabId = -1;
		}

		protected virtual GameObject InstantiateTabContent(Tab p_tab)
		{
			if(p_tab.prefab == null)
			{
				return null;
			}

			GameObject instance = (GameObject)GameObject.Instantiate(p_tab.prefab);

			RectTransform contentTransform = instance.GetComponent<RectTransform>();
			if (instance == null)
			{
				instance.transform.SetParent(m_contentArea, false);
			}
			else
			{
				contentTransform.SetParent(m_contentArea, false);
				contentTransform.SetSize(p_tab.contentSize);
			}

			return instance;
		}
	}
}
