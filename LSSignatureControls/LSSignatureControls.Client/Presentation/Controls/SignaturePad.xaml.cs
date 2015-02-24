using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Resources;

using Microsoft.LightSwitch.Presentation;
using Microsoft.LightSwitch.Presentation.Extensions;
using Microsoft.LightSwitch.Threading;
using System.Windows.Ink;

namespace LSSignatureControls.Presentation.Controls
{
    public partial class SignaturePad : UserControl, IContentVisual
    {
        #region Is Read Only DP
        /// <summary>
        /// The Boolean value bound to the control that indicates if the control is in read-only mode.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(SignaturePad), new PropertyMetadata(OnIsReadOnlyChanged));
        /// <summary>
        /// A Boolean value indicating if the control is in read-only mode or not.
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        static void OnIsReadOnlyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((SignaturePad)sender).OnIsReadOnlyChanged((bool)e.OldValue, (bool)e.NewValue);
        }
        void OnIsReadOnlyChanged(bool oldValue, bool newValue)
        {
            //do nothing for now
            if (oldValue != newValue)
                SigPad.IsReadOnly = newValue;
        }
        #endregion

        IContentItem ContentItem { get; set; }

        ///// <summary>
        ///// Indicates the Source property of this instance has changed
        ///// </summary>
        //public event EventHandler SourceChanged = (s, e) => { };

        public SignaturePad()
        {
            InitializeComponent();
        }

        public object Control
        {
            get { return this.SigPad; }
        }

        public void Show()
        {
        }

        private void statesControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ContentItem = DataContext as IContentItem;
            var formatString = this.ContentItem.Properties["LSSignatureControls:SignaturePad/ImageFormat"] as string;
            ImageEncoding enc;
            if (Enum.TryParse(formatString, true, out enc))
                ((ImageConverter)Resources["imageConverter"]).ImageType = enc;

            SetSignaturePadSizes();
#if TRIAL
            var trialDialog = TrialModalDialog.CreateInstance();
            trialDialog.Show();
#endif
        }

        private void SetSignaturePadSizes()
        {
            var widthSizingMode = this.ContentItem.Properties["Microsoft.LightSwitch:RootControl/WidthSizingMode"] as string;
            if (widthSizingMode == "Pixels")
            {
                var width = (double)this.ContentItem.Properties["Microsoft.LightSwitch:RootControl/Width"];
                this.SigPad.Width = width;
            }
            else
            {
                var maxWidth = (double)this.ContentItem.Properties["Microsoft.LightSwitch:RootControl/MaxWidth"];
                var minWidth = (double)this.ContentItem.Properties["Microsoft.LightSwitch:RootControl/MinWidth"];
                this.SigPad.MinHeight = minWidth;
                this.SigPad.MaxWidth = maxWidth;
            }

            var heightSizingMode = this.ContentItem.Properties["Microsoft.LightSwitch:RootControl/HeightSizingMode"] as string;
            if (heightSizingMode == "Pixels")
            {
                var height = (double)this.ContentItem.Properties["Microsoft.LightSwitch:RootControl/Height"];
                this.SigPad.Height = height;
            }
            else
            {
                var maxHeight = (double)this.ContentItem.Properties["Microsoft.LightSwitch:RootControl/MaxHeight"];
                var minHeight = (double)this.ContentItem.Properties["Microsoft.LightSwitch:RootControl/MinHeight"];
                this.SigPad.MinHeight = minHeight;
                this.SigPad.MaxHeight = maxHeight;
            }
        }

        private void SigPad_SignatureClearing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.ShowMessageBox(LSSignatureControls.Presentation.Resources.Strings.ClearCurrentSignatureQuestion
                                , LSSignatureControls.Presentation.Resources.Strings.ClearCurrentSignatureTitle
                                , MessageBoxOption.YesNo
                                , (r => { if (r == MessageBoxResult.Yes) { this.ContentItem.Value = null; } }));
        }

        void ShowMessageBox(string message, string caption, MessageBoxOption options, Action<MessageBoxResult> resultAction)
        {
            var dispatcher = this.ContentItem.Screen.Details.Dispatcher;

            if (!dispatcher.CheckAccess())
            {
                //call ourselves on the proper thread
                Action action = delegate { this.ShowMessageBox(message, caption, options, resultAction); };
                dispatcher.BeginInvoke(action);
            }
            else
            {
                //we're on the UI thread, so show the message box and execute the result action
                var result = this.ContentItem.Screen.ShowMessageBox(message, caption, options);
                Action action = delegate { resultAction(result); };
                Dispatchers.Main.BeginInvoke(action);
            }
        }
    }

    [Export(typeof(IControlFactory))]
    [ControlFactory("LSSignatureControls:SignaturePad")]
    internal class SignaturePadFactory : IControlFactory
    {
        #region IControlFactory Members

        public DataTemplate DataTemplate
        {
            get
            {
                if (null == this.dataTemplate)
                {
                    this.dataTemplate = XamlReader.Load(SignaturePadFactory.ControlTemplate) as DataTemplate;
                }
                return this.dataTemplate;
            }
        }

        public DataTemplate GetDisplayModeDataTemplate(IContentItem contentItem)
        {
            if (null == this.displayModeDataTemplate)
            {
                this.displayModeDataTemplate = XamlReader.Load(SignaturePadFactory.ReadOnlyControlTemplate) as DataTemplate;
            }
            return this.displayModeDataTemplate;
        }

        #endregion

        #region Private Fields

        private DataTemplate dataTemplate;
        private DataTemplate displayModeDataTemplate;

        #endregion

        #region Constants

        private const string ControlTemplate =
            "<DataTemplate" +
            " xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"" +
            " xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"" +
            " xmlns:ctl=\"clr-namespace:LSSignatureControls.Presentation.Controls;assembly=LSSignatureControls.Client\">" +
            "<ctl:SignaturePad/>" +
            "</DataTemplate>";

        private const string ReadOnlyControlTemplate =
            "<DataTemplate" +
            " xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"" +
            " xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"" +
            " xmlns:ctl=\"clr-namespace:LSSignatureControls.Presentation.Controls;assembly=LSSignatureControls.Client\">" +
            "<ctl:SignaturePad IsReadOnly=\"True\" />" +
            "</DataTemplate>";
        #endregion
    }
}
