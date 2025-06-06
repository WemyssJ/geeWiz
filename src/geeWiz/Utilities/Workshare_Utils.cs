﻿// geeWiz utilities
using gFrm = geeWiz.Forms;
using geeWiz.Extensions;

// The class belongs to the utility namespace
// using gWsh = geeWiz.Utilities.Workshare_Utils
namespace geeWiz.Utilities
{
    /// <summary>
    /// Methods of this class generally relate to checking for editability.
    /// </summary>
    public static class Workshare_Utils
    {
        #region Editable processing routine

        /// <summary>
        /// Reviews multiple elements for editability, allowing for further processing.
        /// </summary>
        /// <param name="objects">Elements to process.</param>
        /// <typeparam name="T">The type of object to process.</typeparam>
        /// <returns>A WorksharingResult object.</returns>
        public static WorksharingResult<T> ProcessElements<T>(List<T> objects, Document doc = null)
        {
            // Variables for use later
            var worksharingResults = new WorksharingResult<T>();
            var editable = new List<T>();
            var nonEditable = new List<T>();

            // For each object...
            foreach (var obj in objects)
            {
                // If the object is an Element...
                if (obj is Element element)
                {
                    // Check for editability
                    if (element.Ext_IsEditable(doc))
                    {
                        editable.Add(obj);
                    }
                    else
                    {
                        nonEditable.Add(obj);
                    }
                }
                // Otherwise it is not an element, and is available
                else
                {
                    editable.Add(obj);
                }
                
            }

            // Assign worksharing results
            worksharingResults.Editable = editable;
            worksharingResults.NotEditable = nonEditable;
            worksharingResults.Cancelled = false;

            // If no elements are editable
            if (editable.Count == 0)
            {
                // Message to user if we lost all elements
                if (objects.Count > 0)
                {
                    gFrm.Custom.Cancelled("Elements were found, but all of them are not editable\n\n" +
                        "The task cannot proceed, and has been cancelled.");
                }

                // Results are deemed cancelled
                worksharingResults.Cancelled = true;
            }
            // Catch if some are not editable
            else if (nonEditable.Count > 0)
            {
                // Present form to user
                var formResult = gFrm.Custom.Message(title: "Please choose an option",
                    message: "Not all elements are editable.\n\nProceed with only editable elements?",
                    yesNo: true);

                // Catch and capture cancellation
                if (formResult.Cancelled)
                {
                    worksharingResults.Cancelled = true;
                }
            }

            // Return the worksharing outcome
            return worksharingResults;
        }

        #endregion
    }

    #region WorksharingResult class

    /// <summary>
    /// Class to store and process editable element checks.
    /// </summary>
    /// <typeparam name="T">The type of object to process.</typeparam>
    public class WorksharingResult<T>
    {
        public List<T> Editable { get; set; }
        public List<T> NotEditable { get; set; }
        public bool Cancelled { get; set; }
    }

    #endregion
}
