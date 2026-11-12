using System.Collections.Generic;
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

        // Суммируем все вложенные в гены пешки архиты (включая подавленные)
        public static int CountArchites(Pawn pawn)
        {
            List<Gene> genes = pawn?.genes?.GenesListForReading;
            if (genes == null) return 0;

            int totalArchites = 0;
            for (int i = 0; i < genes.Count; i++)
            {
                Gene gene = genes[i];
                if (gene != null && gene.def.biostatArc > 0)
                {
                    totalArchites += gene.def.biostatArc;
                }
            }

            return totalArchites;
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

            if (CountArchites(innerPawn) <= 0)
            {
                failReason = "AG_NoArchiteGenes".Translate();
                return false;
            }

            return true;
        }
    }
}
