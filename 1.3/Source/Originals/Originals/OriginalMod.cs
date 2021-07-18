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
        public OriginalMod(ModContentPack content):base(content)
        {
            this.settings = GetSettings<OriginalSettings>();

        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Resurrection sickness", ref OriginalSettings.resSickness, "Should a resurrected pawn have resurrection sickness after coming back");
            listingStandard.CheckboxLabeled("Heal Scars", ref OriginalSettings.healScars, "Should Originals heal scars (brain damage, gunshot scars, etc)");
            listingStandard.GapLine(12);
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
            listingStandard.Label("Highblood Resurrection Multiplier: " + OriginalSettings.highbloodMult, -1, "Multiplier for how long resurrection takes relative to fullblood resurrection. (Default 0.7 means it'll take 0.7x fullblood res time minimum, factoring in hediff strength)");
            OriginalSettings.highbloodMult = listingStandard.Slider(OriginalSettings.highbloodMult, 0, 1);
            listingStandard.Label("Original Resurrection Multiplier: " + OriginalSettings.originalMult, -1, "Multiplier for how long resurrection takes relative to fullblood resurrection. (Default 0.45 means it'll take 0.45x fullblood res time minimum, factoring in hediff strength)");
            OriginalSettings.originalMult = listingStandard.Slider(OriginalSettings.originalMult, 0, 1);
            listingStandard.Label("Original Power Transfer Percent: " + OriginalSettings.originalTransferPercent,-1,"Percentage of an Original's power is absorbed by another Original when they die.");
            listingStandard.Label("Default: 0.25 (25%)");
            OriginalSettings.originalTransferPercent = listingStandard.Slider(OriginalSettings.originalTransferPercent, 0, 1);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);

        }

        public override string SettingsCategory()
        {
            return "Originals";
        }
    }
}
