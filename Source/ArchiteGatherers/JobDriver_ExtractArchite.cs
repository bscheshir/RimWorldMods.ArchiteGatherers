using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchiteGatherers
{
    public class JobDriver_ExtractArchite : JobDriver
    {
        private const int ExtractionDurationTicks = 300;

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

            Toil extract = Toils_General.Wait(ExtractionDurationTicks);
            extract.WithProgressBarToilDelay(TargetIndex.A);
            extract.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            extract.WithEffect(EffecterDefOf.Surgery, TargetIndex.A);
            extract.PlaySustainerOrSound(() => SoundDefOf.Recipe_Surgery);
            yield return extract;

            yield return Toils_General.Do(() =>
            {
                Corpse corpse = TargetCorpse;
                if (corpse?.InnerPawn == null) return;

                corpse.InnerPawn.health.AddHediff(AG_DefOf.AG_ArchiteDrained);

                Thing capsule = ThingMaker.MakeThing(ThingDefOf.ArchiteCapsule);
                GenPlace.TryPlaceThing(capsule, corpse.Position, corpse.Map, ThingPlaceMode.Near);

                Messages.Message(
                    "AG_ExtractionSuccess".Translate(pawn.LabelShort, corpse.InnerPawn.LabelShort),
                    capsule,
                    MessageTypeDefOf.PositiveEvent);
            });
        }
    }
}
