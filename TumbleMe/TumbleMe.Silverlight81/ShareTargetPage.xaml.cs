using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.ApplicationModel.DataTransfer;

namespace TumbleMe.Silverlight81
{
    public partial class ShareTargetPage : PhoneApplicationPage
    {
        public ShareTargetPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.shareOp = (Application.Current as App).ShareOperation;

            if (shareOp.Data.Contains(StandardDataFormats.WebLink))
            {
                Uri link = await shareOp.Data.GetWebLinkAsync();
                SharedData.Text = link.ToString();
            }
            else if (shareOp.Data.Contains(StandardDataFormats.Text))
            {
                SharedData.Text = await shareOp.Data.GetTextAsync();
            }
        }

        public Windows.ApplicationModel.DataTransfer.ShareTarget.ShareOperation shareOp { get; set; }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            this.shareOp.ReportCompleted();
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            this.shareOp.ReportCompleted();
        }
    }
}