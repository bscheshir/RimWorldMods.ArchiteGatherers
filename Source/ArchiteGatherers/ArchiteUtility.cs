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

        public static List<Gene> GetArchiteGenes(Pawn pawn)
        {
            List<Gene> result = new List<Gene>();
            List<Gene> genes = pawn?.genes?.GenesListForReading;
            if (genes == null) return result;

            for (int i = 0; i < genes.Count; i++)
            {
                Gene gene = genes[i];
                if (gene != null && gene.def.biostatArc > 0)
                {
                    result.Add(gene);
                }
            }

            return result;
        }

        public static int CountArchites(Pawn pawn)
        {
            List<Gene> architeGenes = GetArchiteGenes(pawn);
            int totalArchites = 0;

            for (int i = 0; i < architeGenes.Count; i++)
            {
                totalArchites += architeGenes[i].def.biostatArc;
            }

            return totalArchites;
        }

        public static int ExtractAndRemoveArchiteGenes(Pawn pawn)
        {
            if (pawn?.genes == null) return 0;

            List<Gene> architeGenes = GetArchiteGenes(pawn);
            int totalArchites = 0;

            for (int i = 0; i < architeGenes.Count; i++)
            {
                Gene gene = architeGenes[i];
                totalArchites += gene.def.biostatArc;
                pawn.genes.RemoveGene(gene);
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

            if (CountArchites(innerPawn) <= 0)
            {
                failReason = "AG_NoArchiteGenes".Translate();
                return false;
            }

            return true;
        }
    }
}
