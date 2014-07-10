using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace TumbleMe.Silverlight81
{
    internal class ShareUriMapper : UriMapperBase
    {
        public override Uri MapUri(Uri uri)
        {
            if ((Application.Current as App).ShareOperation != null)
            {
                return new Uri(uri.ToString().Replace("MainPage", "ShareTargetPage"), UriKind.Relative);
            }

            return uri;
        }
    }
}
