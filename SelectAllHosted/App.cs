#region Namespaces
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
#endregion

namespace SelectAllHosted
{
    class App : IExternalApplication
    {
        /// <summary>
        /// Tooltip
        /// </summary>
        public const string Message = "Create views and sheets from excel file";
        /// <summary>
        /// Get absolute path to this assembly
        /// </summary>
        static string path = Assembly.GetExecutingAssembly().Location;
        /// <summary>
        /// Use embedded image to load as an icon for the ribbon
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static private BitmapSource GetEmbeddedImage(string name)
        {
            try
            {
                Assembly a = Assembly.GetExecutingAssembly();
                Stream s = a.GetManifestResourceStream(name);
                return BitmapFrame.Create(s);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Add ribbon panel 
        /// </summary>
        /// <param name="a"></param>
        private void AddRibbonPanel(UIControlledApplication a)
        {
            Autodesk.Revit.UI.RibbonPanel rvtRibbonPanel = a.CreateRibbonPanel("Archilizer");
            PulldownButtonData data = new PulldownButtonData("Options", "Archilizer");

            BitmapSource img32 = GetEmbeddedImage("SelectAllHosted.A-logo.ico");

            data.Image = img32;
            data.LargeImage = img32;
            data.ToolTip = Message;

            RibbonItem item = rvtRibbonPanel.AddItem(data);
            PulldownButton optionsBtn = item as PulldownButton;

            optionsBtn.AddPushButton(new PushButtonData("Test", "Test and stuff", path,
                "SelectAllHosted.SelectAH"));
            //optionsBtn.AddPushButton(new PushButtonData("Automatic Dimensions", "AutoDim", path,
            //    "AutomaticDimensions.AutoDim"));
            //optionsBtn.AddPushButton(new PushButtonData("CAD|BIM", "CAD|BIM", path,
            //    "BimpowAddIn.BimToCad"));
        }
        public Result OnStartup(UIControlledApplication a)
        {
            AddRibbonPanel(a);
            a.DialogBoxShowing
            += new EventHandler<DialogBoxShowingEventArgs>(
                a_DialogBoxShowing);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
        /// <summary>
        /// event handler that auto-rejects renaming of views
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void a_DialogBoxShowing(
            object sender,
            DialogBoxShowingEventArgs e)
        {
            TaskDialogShowingEventArgs e2
                = e as TaskDialogShowingEventArgs;

            string s = string.Empty;

            if (null != e2)
            {
                s = string.Format(", dialog id {0}, message '{1}'",
                    e2.DialogId, e2.Message);

                bool isConfirm = e2.Message.Contains("Would you like to rename corresponding level and views?");

                if (isConfirm)
                {
                    e2.OverrideResult(
                        (int)System.Windows.Forms.DialogResult.No);

                    s += ", auto-confirmed.";
                }
            }
        }
    }
}
