using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace LSSignatureControls.Presentation.Controls
{
    public partial class TrialModalDialog : ChildWindow
    {
        static TrialModalDialog _DialogInstance;
        private TrialModalDialog()
        {
            InitializeComponent();
        }
        public static TrialModalDialog CreateInstance()
        {
            if (_DialogInstance == null)
                _DialogInstance = new TrialModalDialog();
            return _DialogInstance;
        }
    }
}

