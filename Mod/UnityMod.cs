using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vasi;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace UnityMod
{
    internal class UnityMod : Mod
    {
        internal static UnityMod Instance { get; private set; }

        public Dictionary<string, AssetBundle> Bundles = new();
        public Dictionary<string, GameObject> GameObjects { get; private set; }

        private Dictionary<string, (string, string)> _preloads = new()
        {
            // ["Wingmould"] = ("White_Palace_18", "White Palace Fly"),
        };

        private Material _blurMat;

        public UnityMod() : base("UnityMod") { }

        public override List<(string, string)> GetPreloadNames()
        {
            return _preloads.Values.ToList();
        }

        public override string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Instance = this;

            _blurMat = Resources.FindObjectsOfTypeAll<Material>().First(mat => mat.shader.name.Contains("UI/Blur/UIBlur"));

            foreach (var (name, (scene, path)) in _preloads)
            {
                GameObjects[name] = preloadedObjects[scene][path];
            }

            LoadAssets();

            On.BlurPlane.Awake += OnBlurPlaneAwake;
            On.PlayMakerFSM.Awake += OnPFSMAwake;
            On.TransitionPoint.Start += OnGateStart;
            USceneManager.activeSceneChanged += OnSceneChange;
        }

        private void OnBlurPlaneAwake(On.BlurPlane.orig_Awake orig, BlurPlane self)
        {
            orig(self);

            if (self.OriginalMaterial.shader.name == "UI/Default")
            {
                self.SetPlaneMaterial(_blurMat);
            }
        }

        private void OnPFSMAwake(On.PlayMakerFSM.orig_Awake orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.FsmName == "Door Control" && self.GetAction<ShowPromptMarker>("In Range").prefab.Value == null)
            {
                self.GetAction<ShowPromptMarker>("In Range").prefab = Resources.FindObjectsOfTypeAll<PromptMarker>().First(marker => marker.name == "Arrow Prompt New").gameObject;
                self.Fsm.GetFsmGameObject("Main Camera").Value = GameCameras.instance.tk2dCam.gameObject;
            }
        }

        private void OnGateStart(On.TransitionPoint.orig_Start orig, TransitionPoint self)
        {
            orig(self);

            if (USceneManager.GetActiveScene().name == "GG_Workshop" && self.targetScene == "GG_Atrium")
            {
                self.SetTargetScene("CustomScene");
                self.entryPoint = "Door_Custom";
            }
        }

        private void OnSceneChange(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name == "CustomScene")
            {
                
            }

        }

        private void LoadAssets()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                if (!resourceName.Contains("customscene")) continue;

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) continue;

                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);

                    stream.Dispose();

                    var bundle = AssetBundle.LoadFromMemory(buffer);
                    UObject.DontDestroyOnLoad(bundle);            

                    Bundles.Add(bundle.name, bundle);
                }
            }
        }
    }
}