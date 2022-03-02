using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalNote.Dto
{
    public class EditLineModel
    {
        public int Index { get; set; }

        public string Name { get { return this.Index.ToString(); } }

        public EditLineModel(int index)
        {
            this.Index = index;
        }
    }
}
