using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Sandbox.ModAPI;
using Sandbox.Game;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;

using VRage;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.Components;
using VRage.Utils;

using SpaceEngineers.Game.ModAPI;
using ObjectBuilders.SafeZone;
using VRageMath;


namespace CERBERUS_Parallax
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MyUpdate : MySessionComponentBase
    {
        private static Vector3D GPS = new Vector3D(-233615.5, 136384.5, -113615.5);
        private static double radius = 25000;
        private static float damage = 0.70f;
        private static List<IMyEntity> Entities = new List<IMyEntity>();
        private static short tiks = 0;

        public MyLanguagesEnum? Language { get; private set; }

        public override void LoadData()
        {
            LoadLocalization();

            MyAPIGateway.Gui.GuiControlRemoved += OnGuiControlRemoved;
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Gui.GuiControlRemoved -= OnGuiControlRemoved;
        }

        private void LoadLocalization()
        {
            var path = Path.Combine(ModContext.ModPathData, "Localization");
            var supportedLanguages = new HashSet<MyLanguagesEnum>();
            MyTexts.LoadSupportedLanguages(path, supportedLanguages);

            var currentLanguage = supportedLanguages.Contains(MyAPIGateway.Session.Config.Language) ? MyAPIGateway.Session.Config.Language : MyLanguagesEnum.English;
            if (Language != null && Language == currentLanguage)
            {
                return;
            }

            Language = currentLanguage;
            var languageDescription = MyTexts.Languages.Where(x => x.Key == currentLanguage).Select(x => x.Value).FirstOrDefault();
            if (languageDescription != null)
            {
                var cultureName = string.IsNullOrWhiteSpace(languageDescription.CultureName) ? null : languageDescription.CultureName;
                var subcultureName = string.IsNullOrWhiteSpace(languageDescription.SubcultureName) ? null : languageDescription.SubcultureName;
                MyLog.Default.WriteLine($"EXAMPLE => Path: {cultureName}");

                MyTexts.LoadTexts(path, cultureName, subcultureName);
            }
        }

        private void OnGuiControlRemoved(object obj)
        {
            if (obj.ToString().EndsWith("ScreenOptionsSpace"))
            {
                LoadLocalization();
            }
        }
    }
    
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_CubeGrid), false)]
    public class Drag : MyGameLogicComponent
    {
        private MyCubeGrid G;
        private double minV = 100, drag;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            if (Entity is MyCubeGrid)
            {
                G = (MyCubeGrid)Entity;
                if (G.IsStatic) { NeedsUpdate = MyEntityUpdateEnum.NONE; }
                else { NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME; }

                if (G.GridSizeEnum == MyCubeSize.Large) { drag = 0.7; }
                else { drag = 0.5; }

                G.OnStaticChanged += OnStaticChanged;
            }
            else { NeedsUpdate = MyEntityUpdateEnum.NONE; }
        }
        private void OnStaticChanged(MyCubeGrid grid, bool isStatic)
        {
            if (grid != G) { return; }

            if (isStatic) { NeedsUpdate = MyEntityUpdateEnum.NONE; }
            else { NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME; }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (!IsOk(G)) return;

            if (G.Physics.Speed >= minV)
            {
                G.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, Vector3D.Normalize(G.Physics.LinearVelocity) * (minV - G.Physics.Speed) * G.Mass * drag, G.Physics.CenterOfMassWorld, null);
            }
            else { NeedsUpdate = MyEntityUpdateEnum.EACH_10TH_FRAME; }
        }
        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            if (!IsOk(G)) return;

            if (G.Physics.Speed < 1) { NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME; }
            else if (G.Physics.Speed > minV) { NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME; }
        }
        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (!IsOk(G)) return;

            if (G.Physics.Speed > 1) { NeedsUpdate = MyEntityUpdateEnum.EACH_10TH_FRAME; }
        }

        private static bool IsOk(IMyCubeGrid G) { return !G.IsStatic && !G.MarkedForClose && G.Physics != null && G.Physics.Enabled && G.Physics.IsActive; }
    }
}