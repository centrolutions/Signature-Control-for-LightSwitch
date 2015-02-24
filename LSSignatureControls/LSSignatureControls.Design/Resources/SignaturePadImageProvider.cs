using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Windows.Media.Imaging;

using Microsoft.LightSwitch.BaseServices.ResourceService;

namespace LSSignatureControls.Resources
{
    [Export(typeof(IResourceProvider))]
    [ResourceProvider("LSSignatureControls.SignaturePad")]
    internal class SignaturePadImageProvider : IResourceProvider
    {
        #region IResourceProvider Members

        public object GetResource(string resourceId, CultureInfo cultureInfo)
        {
            return new BitmapImage(new Uri("/LSSignatureControls.Design;component/Resources/ControlImages/SignaturePad.png", UriKind.Relative));
        }

        #endregion
    }
}
