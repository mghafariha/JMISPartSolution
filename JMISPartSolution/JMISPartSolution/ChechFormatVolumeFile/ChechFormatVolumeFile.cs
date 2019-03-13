using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace JMISPartSolution.ChechFormatVolumeFile
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class ChechFormatVolumeFile : SPItemEventReceiver
    {
        /// <summary>
        /// An item is being added.
        /// </summary>
        public override void ItemAdding(SPItemEventProperties properties)
        {
            base.ItemAdding(properties);
            string str = "";
            SPList list = properties.List;
            SPWeb web = properties.Web;
            bool flag = this.CheckFormat(properties);
            if (!this.CheckFormat(properties))
            {
                str = str + "قابل بارگذاری هست pdf فایل فقط در قالب";
            }
            if (!this.checkSize(properties))
            {
                str = str + " فایل با اندازه بیشتر از MB 5 قابل بارگذاری نیست";
            }
            if (str != "")
            {
                properties.Status = SPEventReceiverStatus.CancelWithError;
                properties.Cancel = true;
                properties.ErrorMessage = str;

            }
        }

        /// <summary>
        /// An item is being updated.
        /// </summary>
        public override void ItemUpdating(SPItemEventProperties properties)
        {
            base.ItemUpdating(properties);
        }

        /// <summary>
        /// An item is being checked in.
        /// </summary>
        public override void ItemCheckingIn(SPItemEventProperties properties)
        {
            base.ItemCheckingIn(properties);
        }
        private bool CheckFormat(SPItemEventProperties properties)
        {
            string beforeUrl = properties.BeforeUrl;
            if (beforeUrl.Substring(beforeUrl.LastIndexOf('.') + 1) != "pdf")
            {
                return false;
            }
            return true;
        }

        private bool checkSize(SPItemEventProperties properties)
        {
            long num = 5000000;
            long totalLength = 0;
            if (properties.ListItem == null)
            {
                totalLength = long.Parse(properties.AfterProperties["vti_filesize"].ToString());
            }
            else
            {
                totalLength = properties.ListItem.File.TotalLength;
            }
            if (totalLength > num)
            {
                return false;
            }
            return true;
        }

 

 


    }
}