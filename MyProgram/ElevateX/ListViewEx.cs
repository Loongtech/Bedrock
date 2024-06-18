using System.ComponentModel;
using System.Windows.Forms;

namespace Net.LoongTech.ElevateX
{
    public partial class ListViewEx : System.Windows.Forms.ListView
    {
        public ListViewEx()
        {
            // 开启双缓冲
            this.SetStyle(ControlStyles.DoubleBuffer |
              ControlStyles.UserPaint |
              ControlStyles.AllPaintingInWmPaint,
              true);

            // Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
        }

        public ListViewEx(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
        protected override void OnNotifyMessage(Message m)
        {
            //Filter out the WM_ERASEBKGND message
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }
    }
}
