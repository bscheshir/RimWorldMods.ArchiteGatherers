using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ArchiteGatherers
{
    public class JobDriver_ExtractArchite : JobDriver
    {
        private const int TicksPerCapsule = 180; // 3 секунды за каждую капсулу

        private Corpse TargetCorpse => (Corpse)job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetCorpse, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !ArchiteUtility.HasArchiteSyringe(pawn));
            this.FailOn(() => !ArchiteUtility.CanExtractFrom(TargetCorpse, out _));

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            int initialCount = Mathf.Max(1, ArchiteUtility.CountArchites(TargetCorpse?.InnerPawn));
            Toil extract = Toils_General.Wait(initialCount * TicksPerCapsule);
            extract.WithProgressBarToilDelay(TargetIndex.A);
            extract.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            extract.WithEffect(EffecterDefOf.Surgery, TargetIndex.A);
            extract.PlaySustainerOrSound(() => SoundDefOf.Recipe_Surgery);
            yield return extract;

            yield return Toils_General.Do(() =>
            {
                Corpse corpse = TargetCorpse;
                if (corpse?.InnerPawn == null) return;

                int totalCount = ArchiteUtility.ExtractAndRemoveArchiteGenes(corpse.InnerPawn);
                if (totalCount <= 0) return;

                // Записываем событие в журнал (Log) обеих пешек
                Find.PlayLog.Add(new PlayLogEntry_Interaction(
                    AG_DefOf.AG_ExtractArchiteInteraction,
                    pawn,
                    corpse.InnerPawn,
                    null));

                int remaining = totalCount;
                int maxStack = Mathf.Max(1, ThingDefOf.ArchiteCapsule.stackLimit);
                Thing lastPlaced = null;

                while (remaining > 0)
                {
                    int toSpawn = Mathf.Min(remaining, maxStack);
                    Thing capsule = ThingMaker.MakeThing(ThingDefOf.ArchiteCapsule);
                    capsule.stackCount = toSpawn;
                    GenPlace.TryPlaceThing(capsule, corpse.Position, corpse.Map, ThingPlaceMode.Near);
                    lastPlaced = capsule;
                    remaining -= toSpawn;
                }

                Messages.Message(
                    "AG_ExtractionSuccess".Translate(pawn.LabelShort, corpse.InnerPawn.LabelShort, totalCount),
                    lastPlaced ?? (Thing)corpse,
                    MessageTypeDefOf.PositiveEvent);
            });
        }
    }
}
