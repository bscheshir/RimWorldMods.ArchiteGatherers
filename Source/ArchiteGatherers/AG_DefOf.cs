using RimWorld;
using Verse;

namespace ArchiteGatherers
{
    [DefOf]
    public static class AG_DefOf
    {
        public static ThingDef AG_ArchiteSyringe;
        public static JobDef AG_ExtractArchiteFromCorpse;
        public static HediffDef AG_ArchiteDrained;

        static AG_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AG_DefOf));
        }
    }
}
