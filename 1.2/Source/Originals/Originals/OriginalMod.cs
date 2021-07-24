using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
namespace Originals
{
    public class OriginalMod : Mod
    {
        OriginalSettings settings;
        private Vector2 scrollPos = Vector2.zero;
        private Rect scrollRect;
        public OriginalMod(ModContentPack content):base(content)
        {
            this.settings = GetSettings<OriginalSettings>();

        }

        public override void DoSettingsWindowContents(Rect inRect)
        {


            Listing_Standard listingStandard = new Listing_Standard();
            Widgets.BeginScrollView(inRect, ref scrollPos, scrollRect);
            inRect.height = 100000f;
            inRect.width -= 20;
            listingStandard.Begin(inRect.AtZero());
            listingStandard.Label("Living Settings");
            listingStandard.CheckboxLabeled("Immortal needs heart", ref OriginalSettings.needHeart, "If checked, when the heart is destroyed, an immortal dies for good");
            listingStandard.CheckboxLabeled("Resurrection sickness", ref OriginalSettings.resSickness, "Should a resurrected pawn have resurrection sickness after coming back");
            listingStandard.CheckboxLabeled("Heal Scars", ref OriginalSettings.healScars, "Should Originals heal scars (brain damage, gunshot scars, etc)");
            listingStandard.Label("Body Part Regen Time: " + OriginalSettings.originalRegenPartTime, -1, "Ticks to fully regenerate a body part.");
            listingStandard.Label("Default: 60000 ticks (1 Day)");
            OriginalSettings.originalRegenPartTime = (int)listingStandard.Slider(OriginalSettings.originalRegenPartTime, 0, 60000);
            listingStandard.Label("Original Healing Rate: " + OriginalSettings.ticksTillHeal + " ticks",-1,"Ticks between an Original's passive healing, 1800 Default");
            OriginalSettings.ticksTillHeal = (int)listingStandard.Slider(OriginalSettings.ticksTillHeal, 0, 10000);

            listingStandard.GapLine(6);
            listingStandard.Label("Resurrection Settings");
            listingStandard.Label("Original Chance: " + OriginalSettings.originalChance);
            listingStandard.Label("Default: 0.05 (5%)");
            OriginalSettings.originalChance = listingStandard.Slider(OriginalSettings.originalChance, 0, 1);

            listingStandard.Label("Base Resurrection Time: " + OriginalSettings.baseResTime, -1, "How many ticks it takes to resurrect a fullblooded original (60,000 ticks is one day)");
            listingStandard.Label("Default: 70000");
           OriginalSettings.baseResTime = (int)listingStandard.Slider(OriginalSettings.baseResTime, 0, 150000);

            listingStandard.Label("Lowblood Resurrection Multiplier: " + OriginalSettings.lowbloodMult,-1,"Max multiplier for how long resurrection takes relative to fullblood resurrection. (Default 1.3 means it'll take 1.3x fullblood time max, factoring in hediff strength)");
            OriginalSettings.lowbloodMult = listingStandard.Slider(OriginalSettings.lowbloodMult, 1, 2);
            listingStandard.Label("Fullblood Resurrection Multiplier: " + OriginalSettings.fullbloodMult, -1, "Multiplier for how long resurrection takes relative to fullblood resurrection. (Default 0.82 means it'll take 0.82x fullblood res time minimum, factoring in hediff strength)");
            OriginalSettings.fullbloodMult = listingStandard.Slider(OriginalSettings.fullbloodMult, 0, 1);
            listingStandard.Label("Highblood Resurrection Multiplier: " + OriginalSettings.highbloodMult, -1, "Multiplier for how long resurrection takes relative to fullblood resurrection. (Default 0.6 means it'll take 0.6x fullblood res time minimum, factoring in hediff strength)");
            OriginalSettings.highbloodMult = listingStandard.Slider(OriginalSettings.highbloodMult, 0, 1);
            listingStandard.Label("Original Resurrection Multiplier: " + OriginalSettings.originalMult, -1, "Multiplier for how long resurrection takes relative to fullblood resurrection. (Default 0.4 means it'll take 0.4x fullblood res time minimum, factoring in hediff strength)");
            OriginalSettings.originalMult = listingStandard.Slider(OriginalSettings.originalMult, 0, 1);
            listingStandard.Label("Original Power Transfer Percent: " + OriginalSettings.originalTransferPercent,-1,"Percentage of an Original's power is absorbed by another Original when they die.");
            listingStandard.Label("Default: 0.2 (20%)");
            OriginalSettings.originalTransferPercent = listingStandard.Slider(OriginalSettings.originalTransferPercent, 0, 1);
            listingStandard.GapLine(6);
            listingStandard.Label("Staking Settings");
            listingStandard.Label("Mortal Stake Days: " + OriginalSettings.mortalStakeMult,-1,"Days an Original will be downed by the staking mechanic, Default: 4");
            listingStandard.Slider(OriginalSettings.mortalStakeMult, 0, 5);
            listingStandard.Label("Lowblood Stake Days: " + OriginalSettings.lowStakeMult, -1, "Days an Original will be downed by the staking mechanic, Default: 2.5");
            listingStandard.Slider(OriginalSettings.lowStakeMult, 0, 5);
            listingStandard.Label("Fullblood Stake Days: " + OriginalSettings.fullStakeMult, -1, "Days an Original will be downed by the staking mechanic, Default: 2");
            listingStandard.Slider(OriginalSettings.fullStakeMult, 0, 5);
            listingStandard.Label("Highblood Stake Days: " + OriginalSettings.highStakeMult, -1, "Days an Original will be downed by the staking mechanic, Default: 1");
            listingStandard.Slider(OriginalSettings.highStakeMult, 0, 5);
            listingStandard.Label("Original Stake Days: " + OriginalSettings.originalStakeMult, -1, "Days an Original will be downed by the staking mechanic, Default: 0.4");
            listingStandard.Slider(OriginalSettings.originalStakeMult, 0, 5);
            scrollRect = new Rect(0f, 0f, inRect.width, listingStandard.CurHeight+100);
            
            Widgets.EndScrollView();
            listingStandard.End();

            base.DoSettingsWindowContents(inRect);

        }

        public override string SettingsCategory()
        {
            return "Originals";
        }

        public void BeginScrollView(Listing_Standard listing, Rect rect, ref Vector2 scrollPosition, ref Rect viewRect)
        {
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect, true);
            rect.height = 100000f;
            rect.width -= 20f;
            listing.Begin(rect.AtZero());
        }

        public void EndScrollView(Listing_Standard listing, ref Rect viewRect)
        {
           // viewRect = new Rect(0f, 0f, viewRect.width, viewRect.curY);
            Widgets.EndScrollView();
            listing.End();
        }
    }
}
