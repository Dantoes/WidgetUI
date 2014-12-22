using UnityEngine;
using UnityEngine.EventSystems;

namespace WidgetUI
{
	public abstract class Tooltip<T, WidgetType> 
		: UIBehaviour
		, IPointerEnterHandler
		, IPointerExitHandler
		where WidgetType : UIBehaviour, IWidget<T>
	{
		protected abstract T GetContent();
		protected abstract IWidgetAllocator<WidgetType> GetWidgetAllocator();

		protected WidgetType m_widget;
		protected IWidgetAllocator<WidgetType> m_allocator;

		[SerializeField]
		protected float m_delay = 0.8F;

		public float Delay
		{
			get
			{
				return m_delay;
			}
			set
			{
				m_delay = value;
			}
		}


		protected virtual void Construct()
		{
			m_widget = null;
			m_allocator = this.GetWidgetAllocator();
		}

		protected virtual void Destruct()
		{
			if (m_widget != null)
			{
				this.Hide();
			}
		}

		protected override void Awake()
		{
			base.Awake();
			this.Construct();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.Destruct();
		}

		protected virtual void Show()
		{
			if(m_widget != null)
			{
				this.Hide();
			}

			m_widget = this.CreateTooltipWidget();
			m_widget.transform.SetParent(this.GetTooltipParent(), false);

			this.AddDefaultComponents(m_widget);
		}

		protected virtual void Hide()
		{
			if (m_widget != null)
			{
				m_widget.Disable();
				m_allocator.Destroy(m_widget);
				m_widget = null;
			}
		}

		protected virtual WidgetType CreateTooltipWidget()
		{
			T content = this.GetContent();
			WidgetType widget = m_allocator.Construct();
			widget.Enable(content);

			return widget;
		}

		protected virtual void AddDefaultComponents(WidgetType p_widget)
		{
			GameObject widgetObject = p_widget.gameObject;

			// If the widget has no FollowMouse component attached, add it
			FollowMouse mouseFollower = widgetObject.GetComponent<FollowMouse>();
			if(mouseFollower == null)
			{
				widgetObject.AddComponent<FollowMouse>();
			}
		}

		protected virtual RectTransform GetTooltipParent()
		{
			Transform transform = this.transform.parent;
			if (transform == null)
			{
				// this will most likely never be the desired behaviour (but avoids returning null)
				transform = this.transform;
			}
			return (RectTransform)transform;
		}


		public void OnPointerEnter(PointerEventData eventData)
		{
			if(this.Delay > 0)
			{
				this.Invoke("Show", this.Delay);
			}
			else
			{
				this.Show();
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			this.CancelInvoke("Show");
			this.Hide();
		}
	}
}
