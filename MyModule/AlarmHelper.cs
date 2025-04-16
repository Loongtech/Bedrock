namespace Net.LoongTech.OmniCoreX
{
    /// <summary>
    /// 消息通知事件处理类
    /// </summary>
    public class AlarmHelper
    {
        //定义一个私有变量instance
        private static AlarmHelper _instance;

        //定义一个静态方法Instance，返回AlarmHelper实例
        public static AlarmHelper Instance
        {
            get
            {
                //如果instance为空，则实例化一个AlarmHelper实例
                if (_instance == null)
                {
                    _instance = new AlarmHelper();
                }
                //返回AlarmHelper实例
                return _instance;
            }
        }

        //定义一个抽象方法AlarmEvent，用于发送消息
        public delegate void AlarmEventHandler(object sender, AlermEventArgs e);

        //定义一个静态方法SendEvent，用于发送消息
        public event AlarmEventHandler AlarmEvent = delegate { };

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="e">消息对象</param>
        public void SendEvent(AlermEventArgs e)
        {
            if (AlarmEvent != null)
                //如果AlarmEvent不为空，则调用AlarmEvent的Invoke方法
                if (AlarmEvent != null)
                    AlarmEvent.Invoke(this, e);
        }

        //(Job Methods & Logics Goes Here)

        /// <summary>
        /// 消息事件参数
        /// </summary>
        public class AlermEventArgs : EventArgs
        {
            /// <summary>
            /// 消息事件的名称
            /// </summary>
            public string AlermName { set; get; }

            /// <summary>
            /// 消息事件的内容
            /// </summary>
            public string AlermMsg { set; get; }

            /// <summary>
            /// 消息内容是否是错误
            /// True： 错误事件
            /// False: 非错误
            /// </summary>
            public bool AlermIsErr { set; get; }

            /// <summary>
            /// 消息事件中的消息对象
            /// </summary>
            /// <param name="_alermName">消息的名称</param>
            /// <param name="_alermMsg">消息的内容</param>
            /// <param name="_alermIsErr">消息内容是否是错误</param>
            public AlermEventArgs(string _alermName, string _alermMsg, bool _alermIsErr)
            {
                //将参数赋值给AlermName，AlermMsg，AlermIsErr
                this.AlermName = _alermName;
                this.AlermMsg = _alermMsg;
                this.AlermIsErr = _alermIsErr;
            }
        }
    }
}
