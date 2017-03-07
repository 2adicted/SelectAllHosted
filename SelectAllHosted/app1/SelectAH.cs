#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace SelectAllHosted
{
    /// <summary>
    /// This add-in will filter the doors and windows in the project and will check their Ids against 
    /// the hosted element Ids of a selecte wall.
    /// 
    /// Hint: use this one when you want to select multiple hosted elements of a single wall 
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class SelectAH : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //filter for doors
            FilteredElementCollector myDoors
                = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Doors);
            //filter for windows
            FilteredElementCollector myWindows 
               = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Windows);

            //Selection
            Reference refElement = null;
            Selection sel = uidoc.Selection;
            WallFilter selFilter = new WallFilter();
            refElement = sel.PickObject(ObjectType.Element,selFilter,"Please select a wall");
            if (refElement == null)
            {
                return Result.Failed;
            }
            Element elem = doc.GetElement(refElement); //fucking hell, 10 hours until I find it
            HostObject hostObj = elem as HostObject;
            IList<ElementId> elId = hostObj.FindInserts(false, false, false, false);
            if (elId.Count == 0)
            {
                TaskDialog.Show("No hosted objects.", "Select host wall");
                return Result.Failed;
            }
            List<ElementId> filterIds = new List<ElementId>();
            foreach (ElementId id in elId)
            {
                foreach (Element d in myDoors)
                {
                    if (d.Id.Compare(id) == 0)
                    {
                        filterIds.Add(d.Id);
                    }
                } 
                foreach (Element w in myWindows)
                {
                    if (w.Id.Compare(id) == 0)
                    {
                        filterIds.Add(w.Id);
                    }
                }
            }

            uidoc.Selection.SetElementIds(filterIds);
            uidoc.RefreshActiveView();

            // Modify document within a transaction

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Transaction Name");
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }

    public class WallFilter : ISelectionFilter
    {
        public bool AllowElement(Element e)
        {
            return (e.Category.Id.IntegerValue.Equals(
                (int)BuiltInCategory.OST_Walls));
        }
        public bool AllowReference(Reference r, XYZ p)
        {
            return false;
        }
    }
}
