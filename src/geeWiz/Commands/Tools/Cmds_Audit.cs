﻿// Revit API
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using View = Autodesk.Revit.DB.View;
// geeWiz
using geeWiz.Extensions;
using gFrm = geeWiz.Forms;
using gWsh = geeWiz.Utilities.Workshare_Utils;

// The class belongs to the Commands namespace
namespace geeWiz.Cmds_Audit
{
    #region Cmd_DeletePatterns

    /// <summary>
    /// Deletes an Fill/Line Patterns beginning with the word IMPORT.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Cmd_DeletePatterns : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get the document
            var uiApp = commandData.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            var doc = uiDoc.Document;

            // Collect fill and line patterns
            var deletePatterns = doc.Ext_Collector()
                .OfClass(typeof(FillPatternElement))
                .Concat(
                    doc.Ext_Collector()
                    .OfClass(typeof(LinePatternElement))
                    )
                .Cast<Element>()
                .Where(e => e.Name.ToUpper().StartsWith("IMPORT"))
                .ToList();

            // Keep editable elements only
            if (doc.IsWorkshared)
            {
                var worksharingResults = gWsh.ProcessElements<Element>(deletePatterns, doc);
                deletePatterns = worksharingResults.Editable;
            }

            // Deletion routine
            return doc.Ext_DeleteElementsRoutine<Element>(deletePatterns, typeName: "Fill/Line Pattern");
        }
    }

    #endregion

    #region Cmd_PurgeRooms

    /// <summary>
    /// Purges unplaced rooms based on user selection.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Cmd_PurgeRooms : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get the document
            var uiApp = commandData.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            var doc = uiDoc.Document;

            // Collect unplaced rooms
            var rooms = doc.Ext_GetRooms(
                includePlaced: false,
                includeUnplaced: true,
                sorted: true);

            // Check if we have no unplaced rooms
            if (rooms.Count == 0)
            {
                return gFrm.Custom.Completed("No unplaced Rooms found in the current document.");
            }

            // Select rooms from a list
            var formResult = doc.Ext_SelectRooms(rooms: rooms, title: "Select rooms to delete");
            if (formResult.Cancelled) { return Result.Cancelled; }
            var deleteRooms = formResult.Objects;

            // Keep editable elements only
            if (doc.IsWorkshared)
            {
                var worksharingResults = gWsh.ProcessElements<SpatialElement>(deleteRooms, doc);
                deleteRooms = worksharingResults.Editable;
            }

            // Deletion routine
            return doc.Ext_DeleteElementsRoutine<SpatialElement>(deleteRooms, typeName: "unplaced Room");
        }
    }

    #endregion

    #region Cmd_PurgeTemplates

    /// <summary>
    /// Purges unused View Templates from the model.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Cmd_PurgeTemplates : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get the document
            var uiApp = commandData.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            var doc = uiDoc.Document;

            // Get used view templates Id strings
            var usedIdStrings = doc.Ext_GetViewFamilyTypes()
                .Select(vft => vft.DefaultTemplateId.ToString())
                .Concat(
                    doc.Ext_GetViews()
                    .Select(v => v.ViewTemplateId.ToString())
                    )
                .Distinct()
                .ToList();

            // Get unused view templates
            var unusedTemplates = doc.Ext_GetViewTemplates(sorted: true)
                .Where(vt => !usedIdStrings.Contains(vt.Id.ToString()))
                .ToList();

            // Check if we have no unused templates
            if (unusedTemplates.Count == 0)
            {
                return gFrm.Custom.Completed("No unused View Templates found in the current document.");
            }

            // Select view templates from a list
            var formResult = doc.Ext_SelectViewTemplates(unusedTemplates, title: "Select templates to delete");
            if (formResult.Cancelled) { return Result.Cancelled; }
            var deleteTemplates = formResult.Objects;

            // Keep editable elements only
            if (doc.IsWorkshared)
            {
                var worksharingResults = gWsh.ProcessElements<View>(deleteTemplates, doc);
                deleteTemplates = worksharingResults.Editable;
            }

            // Deletion routine
            return doc.Ext_DeleteElementsRoutine<View>(deleteTemplates, typeName: "View Template");
        }
    }

    #endregion

    #region Cmd_PurgeFilters

    /// <summary>
    /// Purges unused View Templates from the model.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Cmd_PurgeFilters : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get the document
            var uiApp = commandData.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            var doc = uiDoc.Document;

            // Get used view filter Id strings
            var usedIdStrings = doc.Ext_GetViews()
                .Concat(doc.Ext_GetViewTemplates())
                .SelectMany(v => v.GetFilters())
                .Select(i => i.ToString())
                .Distinct()
                .ToList();

            // Get unused view filters
            var unusedFilters = doc.Ext_Collector()
                .OfClass(typeof(ParameterFilterElement))
                .Where(f => !usedIdStrings.Contains(f.Id.ToString()))
                .OrderBy(f => f.Name)
                .ToList();

            // Check if we have no unused templates
            if (unusedFilters.Count == 0)
            {
                return gFrm.Custom.Completed("No unused View Filters found in the current document.");
            }

            // Construct keys
            var keys = unusedFilters.Select(f => f.Name).ToList();

            // Select view filters from a list
            var formResult = gFrm.Custom.SelectFromList<Element>(keys, unusedFilters, "Select view filters to delete");
            if (formResult.Cancelled) { return Result.Cancelled; }
            var deleteFilters = formResult.Objects;

            // Keep editable elements only
            if (doc.IsWorkshared)
            {
                var worksharingResults = gWsh.ProcessElements<Element>(deleteFilters, doc);
                deleteFilters = worksharingResults.Editable;
            }

            // Deletion routine
            return doc.Ext_DeleteElementsRoutine<Element>(deleteFilters, typeName: "View Filter");
        }
    }

    #endregion
}