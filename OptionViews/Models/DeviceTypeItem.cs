using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionViews.Models
{
    public enum CameraType
    {
        IpCamera = 1,
        UsbCamera = 2,
    }

    class CameraTypeItem
    {
        public string Name { get; set; }

        public CameraType Type { get; set; }

        /// <summary>
        /// Bind時の表示項目
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
