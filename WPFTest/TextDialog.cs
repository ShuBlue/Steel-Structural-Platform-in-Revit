using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFTest
{
    public partial class TextDialog
    {
        public TextDialog(string info = "Please Wait...")
        {
            InitializeComponent();

            textBlock.Text = info;
        }
    }
}
