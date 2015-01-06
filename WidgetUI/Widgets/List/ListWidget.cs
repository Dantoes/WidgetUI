using UnityEngine.EventSystems;


namespace WidgetUI
{
	public abstract class ListWidget<T, WidgetType>
		: Detail.ListWidgetBase<T, WidgetType, ILayout>
		where WidgetType : UIBehaviour, IWidget<T>
	{
		public override T this[int p_index]
		{
			get
			{
				return m_items[p_index];
			}
			set
			{
				m_items[p_index] = value;

				// update the widget
				WidgetType widget = m_widgets[p_index];
				widget.Disable();
				widget.Enable(value);
			}
		}

		public override void Insert(int p_index, T p_item)
		{
			base.Insert(p_index, p_item);
			this.CreateWidgetAt(p_index);
		}

		public override void RemoveAt(int p_index)
		{
			this.RemoveWidgetAt(p_index);
			base.RemoveAt(p_index);
		}

		public override void Clear()
		{
			for(int i = 0; i < this.Count; ++i)
			{
				this.RemoveWidgetAt(i);
			}

			base.Clear();
		}
	}
}
