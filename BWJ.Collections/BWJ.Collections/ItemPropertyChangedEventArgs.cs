using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWJ.Collections
{
    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public ItemPropertyChangedEventArgs(object item, string propertyName) : base(propertyName)
        {
            Item = item;
        }

        /// <summary>
        /// The item containing the property which changed
        /// </summary>
        public object Item { get; private set; }
    }
}
