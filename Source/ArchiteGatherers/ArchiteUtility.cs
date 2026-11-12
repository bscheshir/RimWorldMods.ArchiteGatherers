using System.Linq;
using RimWorld;
using Verse;

namespace ArchiteGatherers
{
    public static class ArchiteUtility
    {
        public static bool HasArchiteSyringe(Pawn pawn)
        {
            if (pawn == null) return false;

            if (pawn.equipment?.Primary?.def == AG_DefOf.AG_ArchiteSyringe)
                return true;

            if (pawn.inventory?.innerContainer != null)
            {
                for (int i = 0; i < pawn.inventory.innerContainer.Count; i++)
                {
                    if (pawn.inventory.innerContainer[i].def == AG_DefOf.AG_ArchiteSyringe)
                        return true;
                }
            }

            return false;
        }

        public static bool CanExtractFrom(Corpse corpse, out string failReason)
        {
            failReason = null;
            Pawn innerPawn = corpse?.InnerPawn;

            if (innerPawn == null || !innerPawn.RaceProps.Humanlike)
            {
                failReason = "AG_NotHumanlike".Translate();
                return false;
            }

            if (corpse.GetRotStage() != RotStage.Fresh)
            {
                failReason = "AG_CorpseRotted".Translate();
                return false;
            }

            if (innerPawn.health?.hediffSet?.HasHediff(AG_DefOf.AG_ArchiteDrained) == true)
            {
                failReason = "AG_AlreadyDrained".Translate();
                return false;
            }

            bool hasArchiteGenes = innerPawn.genes?.GenesListForReading?.Any(g => g.def.biostatArc > 0) == true;
            if (!hasArchiteGenes)
            {
                failReason = "AG_NoArchiteGenes".Translate();
                return false;
            }

            return true;
        }
    }
}
