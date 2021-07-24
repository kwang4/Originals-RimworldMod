using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Originals
{
    [StaticConstructorOnStartup]
    public class WeatherEvent_OriginalLightning:WeatherEvent_LightningFlash
    {
        public WeatherEvent_OriginalLightning(Map map, IntVec3 forcedLoc, int damage, bool sound = true) : base(map)
        {
            this.strikeLoc = forcedLoc;
            this.sound = sound;
            this.damage = damage;
        }

        public override void FireEvent()
        {
            base.FireEvent();
            if (!this.strikeLoc.IsValid)
            {
                this.strikeLoc = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable(this.map) && !this.map.roofGrid.Roofed(sq), this.map, 1000);
            }
            this.boltMesh = LightningBoltMeshPool.RandomBoltMesh;
            if (!this.strikeLoc.Fogged(this.map))
            {
                SoundDef explosionSound = null;
                if (!this.sound)
                    explosionSound = new SoundDef();
                GenExplosion.DoExplosion(this.strikeLoc, this.map, 1.9f, DamageDefOf.Smoke, null, this.damage, -1f, explosionSound, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false, null, null);
                Vector3 loc = this.strikeLoc.ToVector3Shifted();
                for (int i = 0; i < 4; i++)
                {
                    MoteMaker.ThrowSmoke(loc, this.map, 1.5f);
                    MoteMaker.ThrowMicroSparks(loc, this.map);
                    MoteMaker.ThrowLightningGlow(loc, this.map, 1.5f);
                }
            }
            if(this.sound)
            {
                SoundInfo info = SoundInfo.InMap(new TargetInfo(this.strikeLoc, this.map, false), MaintenanceType.None);
                SoundDefOf.Thunder_OnMap.PlayOneShot(info);
            }

        }

        public override void WeatherEventDraw()
        {
            Graphics.DrawMesh(this.boltMesh, this.strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity, FadedMaterialPool.FadedVersionOf(LightningMat, base.LightningBrightness), 0);
        }

        private IntVec3 strikeLoc = IntVec3.Invalid;

        private Mesh boltMesh;

        private int damage;

        private bool sound;

        private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt", -1);
    }
}
