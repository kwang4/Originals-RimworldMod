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
            listingStandard.Label("Original Chance: " + OriginalSettings.originalChance);
            listingStandard.Label("Default: 0.05 (5%)");
            OriginalSettings.originalChance = listingStandard.Slider(OriginalSettings.originalChance, 0, 1);

            listingStandard.Label("Base Resurrection Time: " + OriginalSettings.baseResTime, -1, "How many ticks it takes to resurrect a fullblooded original (60,000 ticks is one day)");
            listingStandard.Label("Default: 70000");
           OriginalSettings.baseResTime = (int)listingStandard.Slider(OriginalSettings.baseResTime, 0, 150000);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);

        }

        public override string SettingsCategory()
        {
            return "Originals";
        }
    }
}
