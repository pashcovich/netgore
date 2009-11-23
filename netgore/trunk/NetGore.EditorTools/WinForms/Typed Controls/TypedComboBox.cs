using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NetGore.EditorTools
{
    /// <summary>
    /// A <see cref="ComboBox"/> with some strong typing support.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    public class TypedComboBox<T> : ComboBox
    {
        public event TypedComboBoxChangeEventHandler<T> TypedSelectedItemChanged;

        public void AddItem(T item)
        {
            Items.Add(new TypedListItem(this, item));
        }

        public void AddItems(IEnumerable<T> items)
        {
            Items.AddRange(items.Select(x => new TypedListItem(this, x)).ToArray());
        }

        /// <summary>
        /// Gets the items to initially populate the <see cref="ComboBox"/> with.
        /// </summary>
        /// <returns>The items to initially populate the <see cref="ComboBox"/> with.</returns>
        protected virtual IEnumerable<T> GetInitialItems()
        {
            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Gets the string to display for an item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The string to display.</returns>
        protected virtual string ItemToString(T item)
        {
            return item.ToString();
        }

        /// <summary>
        /// Raises the <see cref="M:System.Windows.Forms.Control.CreateControl"/> method.
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            // Populate the list
            var items = GetInitialItems();

            if (items != null && !items.IsEmpty())
            {
                Items.Clear();
                AddItems(items);
            }

            if (Items.Count > 0)
                SelectedIndex = 0;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.ListControl.SelectedValueChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnSelectedValueChanged(EventArgs e)
        {
            base.OnSelectedValueChanged(e);

            var i = SelectedItem as TypedListItem;
            if (i != null)
            {
                OnTypedSelectedValueChanged(i.Value);
                if (TypedSelectedItemChanged != null)
                    TypedSelectedItemChanged(this, i.Value);
            }
        }

        protected virtual void OnTypedSelectedValueChanged(T item)
        {
        }

        class TypedListItem
        {
            readonly TypedComboBox<T> _owner;
            readonly T _value;

            /// <summary>
            /// Initializes a new instance of the <see cref="TypedListItem"/> class.
            /// </summary>
            /// <param name="owner">The owner.</param>
            /// <param name="value">The value.</param>
            public TypedListItem(TypedComboBox<T> owner, T value)
            {
                _owner = owner;
                _value = value;
            }

            public T Value
            {
                get { return _value; }
            }

            /// <summary>
            /// Returns a <see cref="System.String"/> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String"/> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return _owner.ItemToString(Value);
            }
        }
    }
}