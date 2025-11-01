using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using DevHoldableEngine;
using dingus.Behaviors;
using dingus.Networking;
using GorillaLocomotion.Swimming;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilla;

namespace dingus
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private static ConfigEntry<bool> enableSongByDefault;

        
        public static  AssetBundle bundle;
        public static  GameObject  dingusPrefab;
        private static GameObject  localDingus;
        private static Texture2D   dingusIcon;
        private        bool        dragging;
        private        Vector2     dragOffset;
        private        Rect        guiRect = new(10, 10, 240, 270);
        private        bool        muted;
        private        Vector3     ogDingusScale;

        private bool showGUI = true;

        private void Awake()
        {
            enableSongByDefault = Config.Bind(
                    "Dingus Settings",
                    "Enable Song by Default",
                    true,
                    "If false, the Dingus will start muted on launch."
            );
        }

        private void Start() => Events.GameInitialized += Init;

        internal void Update()
        {
            if (Keyboard.current.insertKey.wasPressedThisFrame)
                showGUI = !showGUI;
        }

        private void OnGUI()
        {
            if (!showGUI) return;

            GUI.backgroundColor = Color.blue;
            guiRect             = GUI.Window(41, guiRect, DrawWindow, "Dingus Controller");

            if (Event.current.type == EventType.MouseDown && guiRect.Contains(Event.current.mousePosition))
            {
                dragOffset = Event.current.mousePosition - new Vector2(guiRect.x, guiRect.y);
                dragging   = true;
            }

            if (!dragging || Event.current.type != EventType.MouseDrag)
                return;

            guiRect.position = Event.current.mousePosition - dragOffset;
            Event.current.Use();
        }

        private void Init(object sender, EventArgs e)
        {
            bundle     = LoadAssetBundle("dingus.Resources.dingus");
            dingusIcon = LoadTextureFromResource("dingus.Resources.dingusICON.png");

            dingusPrefab = bundle.LoadAsset<GameObject>("dingus");
            localDingus  = Instantiate(dingusPrefab);
            DontDestroyOnLoad(localDingus);

            localDingus.transform.position = new Vector3(-66.4f, 14.5f, -82.5f);
            ogDingusScale                  = localDingus.transform.localScale;

            DevHoldable holdable = localDingus.AddComponent<DevHoldable>();
            holdable.Rigidbody = localDingus.GetComponent<Rigidbody>();
            holdable.PickUp    = true;

            localDingus.AddComponent<DingusInjury>();
            localDingus.AddComponent<RigidbodyWaterInteraction>();
            localDingus.AddComponent<LocalDingus>();

            localDingus.layer = 8;

            gameObject.AddComponent<DingusManager>();

            if (!enableSongByDefault.Value)
            {
                foreach (AudioSource aSrc in localDingus.GetComponentsInChildren<AudioSource>())
                    aSrc.enabled = false;
                muted = true;
            }

            DingusNetworkManager.SetHasDingus();
        }


        private AssetBundle LoadAssetBundle(string path)
        {
            Stream      stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            AssetBundle bundle = AssetBundle.LoadFromStream(stream);
            stream.Close();

            return bundle;
        }

        private Texture2D LoadTextureFromResource(string resourcePath)
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);

            if (stream == null) return null;
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            Texture2D tex = new(2, 2);
            tex.LoadImage(buffer);

            return tex;
        }

        private void DrawWindow(int windowID)
        {
            if (dingusIcon != null)
                GUI.DrawTexture(new Rect(-15, 20, 280, 100), dingusIcon);

            GUI.backgroundColor = Color.green;
            if (GUI.Button(new Rect(10, 120, 220, 30), "Bring Dingus"))
                if (localDingus != null && GorillaTagger.Instance != null)
                    localDingus.transform.position = GorillaTagger.Instance.headCollider.transform.position +
                                                     new Vector3(0f, 0.3f, 0.5f);

            GUI.backgroundColor = Color.red;
            if (GUI.Button(new Rect(10, 160, 220, 30), "Reset Dingus"))
                if (localDingus != null && GorillaTagger.Instance != null)
                {
                    localDingus.transform.position   = new Vector3(-66.4f, 14.5f, -82.5f);
                    localDingus.transform.localScale = ogDingusScale;
                }

            GUI.backgroundColor = Color.magenta;
            if (GUI.Button(new Rect(10, 200, 220, 30), "STFU Dingus"))
                foreach (AudioSource aSrc in localDingus.GetComponentsInChildren<AudioSource>())
                    if (!muted)
                    {
                        aSrc.enabled = false;
                        muted        = true;
                    }
                    else
                    {
                        aSrc.enabled = true;
                        muted        = false;
                    }

            GUI.backgroundColor = Color.blue;

            GUI.Label(new Rect(37.5f, 230, 220, 25), "Press [Insert] to Toggle GUI");

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}