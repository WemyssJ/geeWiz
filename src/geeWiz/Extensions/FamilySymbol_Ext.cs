﻿// The class belongs to the extensions namespace
// FamilySymbol familySymbol.ExtensionMethod()
namespace geeWiz.Extensions
{
    /// <summary>
    /// Methods of this class generally relate to FamilySymbols (types).
    /// </summary>
    public static class FamilySymbol_Ext
    {
        #region Namekey

        /// <summary>
        /// Constructs a name key based on a Revit FamilySymbol (type).
        /// </summary>
        /// <param name="familySymbol">A Revit FamilySymbol (type).</param>
        /// <param name="includeId">Append the ElementId to the end.</param>
        /// <param name="instanceId">A provided FamilyInstance Id to use.</param>
        /// <returns>A string.</returns>
        public static string Ext_ToFamilySymbolKey(this FamilySymbol familySymbol, bool includeId = false, ElementId instanceId = null)
        {
            // Null catch
            if (familySymbol is null) { return "???"; }

            // Construct the key without Id
            string familySymbolKey = $"{familySymbol.Family.FamilyCategory.Name}: " +
                $"{familySymbol.Family.Name} - {familySymbol.Name}";

            // Set the instanceId if null
            instanceId ??= familySymbol.Id;

            // Return key with Id
            if (includeId)
            {
                return $"{familySymbolKey} [{instanceId.ToString()}]";
            }
            // Return key without Id
            else
            {
                return familySymbolKey;
            }
        }

        #endregion

        #region Get all instances

        /// <summary>
        /// Returns all instances of the provided family type.
        /// </summary>
        /// <param name="familySymbol">The FamilySymbol (extended).</param>
        /// <returns>A list of FamilyInstances.</returns>
        public static List<FamilyInstance> Ext_GetAllInstances(this FamilySymbol familySymbol)
        {
            // Get family symbol Id
            var familySymbolId = familySymbol.Id;
            
            // Get all instances of same symbool
            return familySymbol.Document.Ext_Collector()
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(f => f.Symbol.Id == familySymbolId)
                .ToList();
        }

        #endregion
    }
}