using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ArchiteGatherers
{
    public class FloatMenuOptionProvider_ExtractArchite : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            Pawn pawn = context?.FirstSelectedPawn;
            return pawn != null && pawn.IsColonistPlayerControlled && !pawn.Downed && ArchiteUtility.HasArchiteSyringe(pawn);
        }

        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            if (!(clickedThing is Corpse corpse) || corpse.Destroyed)
                return null;

            if (corpse.InnerPawn?.RaceProps?.Humanlike != true)
                return null;

            Pawn pawn = context.FirstSelectedPawn;
            if (pawn == null || pawn.Map == null)
                return null;

            FloatMenuOption opt;

            if (!ArchiteUtility.CanExtractFrom(corpse, out string failReason))
            {
                opt = new FloatMenuOption("AG_ExtractMenuDisabled".Translate(corpse.LabelShort, failReason), null);
                opt.Disabled = true;
            }
            else if (!pawn.CanReach(corpse, PathEndMode.Touch, Danger.Deadly))
            {
                opt = new FloatMenuOption("AG_ExtractMenuDisabled".Translate(corpse.LabelShort, "NoPath".Translate()), null);
                opt.Disabled = true;
            }
            else
            {
                Pawn localPawn = pawn;
                opt = new FloatMenuOption("AG_ExtractMenuOption".Translate(corpse.LabelShort), () =>
                {
                    Job job = JobMaker.MakeJob(AG_DefOf.AG_ExtractArchiteFromCorpse, corpse);
                    localPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                });
                opt.Disabled = false;
            }

            opt.extraPartWidth = 32f;
            opt.extraPartOnGUI = (Rect rect) =>
            {
                Rect iconRect = new Rect(rect.x + 4f, rect.y + 4f, 24f, 24f);
                Widgets.ThingIcon(iconRect, AG_DefOf.AG_ArchiteSyringe);
                return false;
            };

            return FloatMenuUtility.DecoratePrioritizedTask(opt, pawn, corpse);
        }
    }
}
