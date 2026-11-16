// Configurador del prototipo FantasyAR (Nivel 2).
// Importa modelos y animaciones de Mixamo como Humanoid y crea los Animator Controllers.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class ConfiguradorFantasyAR
{
    public const string Raiz = "Assets/FantasyAR";
    public static readonly string[] Personajes = { "Caballero", "Mago", "Orco", "Elfo" };

    // Personaje controlado por botones virtuales (sus animaciones quedan en bucle hasta pulsar otro botón).
    public const string PersonajeConBotones = "Mago";

    [MenuItem("FantasyAR/1. Configurar importación de modelos y animaciones")]
    public static string ConfigurarImportaciones()
    {
        var log = new System.Text.StringBuilder();
        foreach (var p in Personajes) log.Append(ConfigurarPersonaje(p));
        return log.ToString();
    }

    public static string ConfigurarPersonaje(string p)
    {
        var log = new System.Text.StringBuilder();
        {
            // Modelo
            var rutaModelo = $"{Raiz}/Modelos/{p}/{p}.fbx";
            var mi = (ModelImporter)AssetImporter.GetAtPath(rutaModelo);
            mi.animationType = ModelImporterAnimationType.Human;
            mi.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            mi.importAnimation = false;
            mi.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            mi.materialLocation = ModelImporterMaterialLocation.InPrefab;
            mi.SaveAndReimport();
            var carpetaTex = $"{Raiz}/Modelos/{p}/Textures";
            if (!AssetDatabase.IsValidFolder(carpetaTex)) AssetDatabase.CreateFolder($"{Raiz}/Modelos/{p}", "Textures");
            mi.ExtractTextures(carpetaTex);
            AssetDatabase.Refresh();
            mi.SaveAndReimport();
            log.AppendLine($"Modelo {p}: Humanoid, avatar válido = {AvatarValido(rutaModelo)}");

            // Animaciones
            foreach (var ruta in Directory.GetFiles($"{Raiz}/Animaciones/{p}", "*.fbx").Select(f => f.Replace('\\', '/')))
            {
                var ai = (ModelImporter)AssetImporter.GetAtPath(ruta);
                ai.animationType = ModelImporterAnimationType.Human;
                ai.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                ai.importAnimation = true;
                ai.materialImportMode = ModelImporterMaterialImportMode.None;
                var tipo = TipoDeAnimacion(ruta);
                bool bucle = tipo == "Idle" || p == PersonajeConBotones;
                var clips = ai.defaultClipAnimations;
                foreach (var c in clips)
                {
                    c.name = $"{p}_{tipo}";
                    c.loopTime = bucle;
                    c.loopPose = false;
                    c.lockRootRotation = true;      // Bake Into Pose (rotación)
                    c.keepOriginalOrientation = true;
                    c.lockRootHeightY = true;       // Bake Into Pose (Y): el salto se ve sin mover el objeto
                    c.keepOriginalPositionY = true;
                    c.lockRootPositionXZ = true;    // Bake Into Pose (XZ): el personaje no se sale del marcador
                    c.keepOriginalPositionXZ = true;
                }
                ai.clipAnimations = clips;
                ai.SaveAndReimport();
                log.AppendLine($"  Animación {Path.GetFileName(ruta)} -> {p}_{tipo} (bucle={bucle})");
            }
        }
        return log.ToString();
    }

    static string TipoDeAnimacion(string ruta)
    {
        var n = Path.GetFileNameWithoutExtension(ruta);
        if (n.Contains("_Idle_")) return "Idle";
        if (n.Contains("_Spell_")) return "Spell";
        if (n.Contains("_Jump_")) return "Jump";
        throw new System.Exception("No se reconoce el tipo de animación: " + ruta);
    }

    static bool AvatarValido(string ruta)
    {
        var av = AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Avatar>().FirstOrDefault();
        return av != null && av.isValid && av.isHuman;
    }

    public static AnimationClip Clip(string personaje, string tipo)
    {
        var ruta = Directory.GetFiles($"{Raiz}/Animaciones/{personaje}", $"{personaje}_{tipo}_*.fbx")[0].Replace('\\', '/');
        return AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview__"));
    }

    [MenuItem("FantasyAR/2. Crear Animator Controllers")]
    public static string CrearAnimators()
    {
        var log = new System.Text.StringBuilder();
        foreach (var p in Personajes)
        {
            var ruta = $"{Raiz}/Animators/{p}_Animator.controller";
            AssetDatabase.DeleteAsset(ruta);
            var ac = AnimatorController.CreateAnimatorControllerAtPath(ruta);
            var sm = ac.layers[0].stateMachine;
            var idle = Clip(p, "Idle"); var spell = Clip(p, "Spell"); var jump = Clip(p, "Jump");

            if (p == PersonajeConBotones)
            {
                // Controlado por los botones virtuales: cada trigger activa SOLO su animación.
                ac.AddParameter("Spell", AnimatorControllerParameterType.Trigger);
                ac.AddParameter("Idle", AnimatorControllerParameterType.Trigger);
                ac.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
                var sIdle = sm.AddState("Idle", new Vector3(300, 0));
                var sSpell = sm.AddState("Spell", new Vector3(550, -100));
                var sJump = sm.AddState("Jump", new Vector3(550, 100));
                sIdle.motion = idle; sSpell.motion = spell; sJump.motion = jump;
                sm.defaultState = sIdle;
                foreach (var (estado, trigger) in new[] { (sSpell, "Spell"), (sIdle, "Idle"), (sJump, "Jump") })
                {
                    var t = sm.AddAnyStateTransition(estado);
                    t.AddCondition(AnimatorConditionMode.If, 0, trigger);
                    t.hasExitTime = false;
                    t.duration = 0.25f;
                    t.canTransitionToSelf = false;
                }
                log.AppendLine($"{p}: Idle (inicio) <-> Spell / Jump por triggers de botones virtuales");
            }
            else
            {
                // Ciclo automático para mostrar las tres animaciones: Idle -> Spell -> Idle -> Jump -> Idle ...
                var sIdle = sm.AddState("Idle", new Vector3(300, 0));
                var sSpell = sm.AddState("Spell", new Vector3(550, -100));
                var sIdle2 = sm.AddState("Idle 2", new Vector3(800, 0));
                var sJump = sm.AddState("Jump", new Vector3(550, 100));
                sIdle.motion = idle; sIdle2.motion = idle; sSpell.motion = spell; sJump.motion = jump;
                sm.defaultState = sIdle;
                Encadenar(sIdle, sSpell, 1.0f);
                Encadenar(sSpell, sIdle2, 0.9f);
                Encadenar(sIdle2, sJump, 1.0f);
                Encadenar(sJump, sIdle, 0.9f);
                log.AppendLine($"{p}: ciclo Idle -> Spell -> Idle -> Jump -> Idle");
            }
            EditorUtility.SetDirty(ac);
        }
        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    // ------------------------------------------------------------------ ESCENA
    const string BaseDatos = "Vuforia/ActvidadVuforiaBD.xml";   // ruta relativa a StreamingAssets
    const string RutaEscena = Raiz + "/Scenes/FantasyAR_Nivel2.unity";
    const string RutaLicencia = "Recursos/00_Vuforia/Licencia_Vuforia.txt";

    [MenuItem("FantasyAR/3. Construir escena")]
    public static string ConstruirEscena()
    {
        var log = new System.Text.StringBuilder();
        var escena = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
            UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, UnityEditor.SceneManagement.NewSceneMode.Single);
        Object.DestroyImmediate(GameObject.Find("Main Camera"));

        // Cámara AR + licencia
        var arCam = CrearDesdeMenu("GameObject/Vuforia Engine/AR Camera");
        arCam.tag = "MainCamera";
        var camara = arCam.GetComponent<Camera>();
        arCam.AddComponent<ConfigurarCamaraAR>();
        // Render a resolución completa en todos los perfiles (el perfil Mobile venía al 80 % y se veía borroso)
        for (int i = 0; i < QualitySettings.names.Length; i++)
            if (QualitySettings.GetRenderPipelineAssetAt(i) is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urp)
            {
                urp.renderScale = 1f;
                // Los efectos de Cartoon FX usan "partículas suaves": sin textura de profundidad son invisibles en Android
                urp.supportsCameraDepthTexture = true;
                EditorUtility.SetDirty(urp);
            }
        log.AppendLine(ConfigurarLicencia());

        // 1) MultiTarget Cuboid (Merge Cube 6 cm) -> Caballero
        var mt = CrearDesdeMenu("GameObject/Vuforia Engine/Multi Target");
        mt.name = "MultiTarget_Cuboid_Caballero";
        var smt = new SerializedObject(mt.GetComponent<Vuforia.MultiTargetBehaviour>());
        smt.FindProperty("mDataSetPath").stringValue = BaseDatos;
        smt.FindProperty("mTrackableName").stringValue = "cubeMerge";
        smt.FindProperty("mSize").vector3Value = new Vector3(6f, 6f, 6f);
        smt.FindProperty("mInitializedInEditor").boolValue = true;
        smt.ApplyModifiedPropertiesWithoutUndo();
        mt.transform.position = new Vector3(0f, 0f, 0f);
        // El caballero envuelve el cubo: 20 unidades de alto (cubo = 6) con el torso centrado en el cubo
        var caballero = ColocarPersonaje("Caballero", mt.transform, 20f, Vector3.zero, 0f, 0.6f);
        var anCab = caballero.GetComponent<Animator>();
        AgregarVFXEnHueso("CFXR Electrified 3", anCab.GetBoneTransform(HumanBodyBones.RightHand), 2.2f, "Electricidad_ManoDer");
        AgregarVFXEnHueso("CFXR Electrified 3", anCab.GetBoneTransform(HumanBodyBones.LeftHand), 2.2f, "Electricidad_ManoIzq");
        log.AppendLine("Cuboid listo: " + caballero.name);

        // 2) Image Targets
        var t1 = CrearImageTarget("dibujo1_mago_vuforia", "ImageTarget_Dibujo1_Mago", new Vector3(0.4f, 0f, 0f));
        var t2 = CrearImageTarget("dibujo2_orco_vuforia", "ImageTarget_Dibujo2_Orco", new Vector3(0.7f, 0f, 0f));
        var t3 = CrearImageTarget("dibujo3_elfo_vuforia", "ImageTarget_Dibujo3_Elfo", new Vector3(1.0f, 0f, 0f));

        // Cada personaje va dentro de un "pivote" en sus pies que se inclina para quedar de frente a la cámara.
        // Mago: pies justo encima de los 3 sellos para no taparlos (botones virtuales).
        var pMago = CrearPivote(t1.transform, "Mago", new Vector3(0f, 0f, -0.04f), camara);
        var mago = ColocarPersonaje("Mago", pMago, 0.14f, Vector3.zero, 180f, 0f);
        AgregarVFX("CFXR3 Magic Aura A (Runic)", pMago, new Vector3(0f, 0.002f, 0f), 0.06f, "AuraMagica");
        var poof = AgregarVFX("CFXR Magic Poof", pMago, new Vector3(0f, 0.08f, 0f), 0.03f, "Hechizo", loop: false);
        // Rayo que sale de la mano del Mago cada vez que termina el hechizo
        var anMago = mago.GetComponent<Animator>();
        var rayo = AgregarVFXEnHueso("CFXR3 Hit Electric C (Air)", anMago.GetBoneTransform(HumanBodyBones.RightHand), 0.07f, "Rayo", loop: false);
        ConfigurarBotones(t1, anMago, poof.GetComponentInChildren<ParticleSystem>(), camara);
        t1.GetComponent<ControlBotonesMago>().efectoRayo = rayo.GetComponentInChildren<ParticleSystem>();

        var pOrco = CrearPivote(t2.transform, "Orco", new Vector3(0f, 0f, -0.08f), camara);
        var orco = ColocarPersonaje("Orco", pOrco, 0.16f, Vector3.zero, 180f, 0f);
        var anOrco = orco.GetComponent<Animator>();
        AgregarVFXEnHueso("CFXR Fire", anOrco.GetBoneTransform(HumanBodyBones.RightHand), 0.03f, "Fuego_ManoDer", espacioMundo: true);
        AgregarVFXEnHueso("CFXR Fire", anOrco.GetBoneTransform(HumanBodyBones.LeftHand), 0.03f, "Fuego_ManoIzq", espacioMundo: true);

        var pElfo = CrearPivote(t3.transform, "Elfo", new Vector3(0f, 0f, -0.1f), camara);
        var elfo = ColocarPersonaje("Elfo", pElfo, 0.18f, new Vector3(0f, -0.015f, 0f), 180f, 0f);   // un poco más abajo
        AgregarVFX("CFXR4 Falling Stars", pElfo, new Vector3(0f, 0.2f, 0f), 0.1f, "EstrellasMagicas");
        AgregarVFXEnHueso("CFXR2 Shiny Item (Loop)", elfo.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand), 0.04f, "Brillo_Espada");

        ConfigurarLuces(arCam);
        log.AppendLine("Image Targets listos: Mago (botones), Orco, Elfo");

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(escena, RutaEscena);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(RutaEscena, true) };
        log.AppendLine("Escena guardada en " + RutaEscena);
        return log.ToString();
    }

    static Transform CrearPivote(Transform target, string nombre, Vector3 pies, Camera camara)
    {
        var p = new GameObject("Pivote_" + nombre).transform;
        p.SetParent(target, false);
        p.localPosition = pies;
        var o = p.gameObject.AddComponent<OrientarHaciaCamara>();
        o.imageTarget = target;
        o.camara = camara;
        return p;
    }

    // Luz que acompaña a la cámara (ilumina siempre la cara visible) + luz ambiental clara
    static void ConfigurarLuces(GameObject arCam)
    {
        var vieja = GameObject.Find("Directional Light");
        if (vieja) Object.DestroyImmediate(vieja);

        Light Crear(string n, Vector3 rot, float intensidad)
        {
            var go = new GameObject(n);
            go.transform.SetParent(arCam.transform, false);
            go.transform.localRotation = Quaternion.Euler(rot);
            var l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = intensidad;
            l.color = Color.white;
            l.shadows = LightShadows.None;
            return l;
        }
        // Desactivadas a pedido: los personajes se iluminan solo con la luz ambiental
        Crear("Luz_Principal_Camara", new Vector3(15f, -20f, 0f), 1.7f).enabled = false;
        Crear("Luz_Relleno", new Vector3(-30f, 160f, 0f), 0.8f).enabled = false;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.8f, 0.8f, 0.82f);
        RenderSettings.skybox = null;
    }

    // Saca los materiales del FBX para poder ajustarlos: sin aspecto metálico y con poco brillo especular
    [MenuItem("FantasyAR/0. Ajustar materiales")]
    public static string AjustarMateriales()
    {
        var log = new System.Text.StringBuilder();
        foreach (var p in Personajes)
        {
            var ruta = $"{Raiz}/Modelos/{p}/{p}.fbx";
            var carpeta = $"{Raiz}/Modelos/{p}/Materiales";
            if (!AssetDatabase.IsValidFolder(carpeta)) AssetDatabase.CreateFolder($"{Raiz}/Modelos/{p}", "Materiales");
            foreach (var m in AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Material>().ToArray())
            {
                var destino = $"{carpeta}/{m.name}.mat";
                if (!File.Exists(destino)) AssetDatabase.ExtractAsset(m, destino);
            }
            AssetDatabase.WriteImportSettingsIfDirty(ruta);
            AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);
            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { carpeta }))
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.15f);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
                if (mat.HasProperty("_EnvironmentReflections")) mat.SetFloat("_EnvironmentReflections", 0f);
                mat.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF"); mat.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
                EditorUtility.SetDirty(mat);
                log.AppendLine($"{p}: {mat.name} tex={(mat.GetTexture("_BaseMap") ? mat.GetTexture("_BaseMap").name : "NINGUNA")}");
            }
        }
        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    static GameObject CrearDesdeMenu(string menu)
    {
        Selection.activeObject = null;
        if (!EditorApplication.ExecuteMenuItem(menu)) throw new System.Exception("No se pudo ejecutar " + menu);
        return Selection.activeGameObject;
    }

    static string ConfigurarLicencia()
    {
        var clave = File.ReadAllText(RutaLicencia).Trim();
        var cfg = Vuforia.VuforiaConfiguration.Instance;
        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(cfg)))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CreateAsset(cfg, "Assets/Resources/VuforiaConfiguration.asset");
        }
        var so = new SerializedObject(cfg);
        so.FindProperty("vuforia.vuforiaLicenseKey").stringValue = clave;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(cfg);
        AssetDatabase.SaveAssets();
        return $"Licencia configurada ({clave.Length} caracteres) en {AssetDatabase.GetAssetPath(cfg)}";
    }

    static GameObject CrearImageTarget(string nombreTarget, string nombreObjeto, Vector3 posicion)
    {
        var go = CrearDesdeMenu("GameObject/Vuforia Engine/Image Target");
        go.name = nombreObjeto;
        go.transform.position = posicion;
        var it = go.GetComponent<Vuforia.ImageTargetBehaviour>();
        var so = new SerializedObject(it);
        so.FindProperty("mImageTargetType").enumValueIndex = 0; // PREDEFINED
        so.FindProperty("mDataSetPath").stringValue = BaseDatos;
        so.FindProperty("mTrackableName").stringValue = nombreTarget;
        so.ApplyModifiedPropertiesWithoutUndo();
        // Rutina del propio editor de Vuforia para leer ancho/alto desde la base de datos
        var edAsm = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Vuforia.Unity.Editor");
        var sit = edAsm.GetTypes().First(t => t.Name == "SerializedImageTarget");
        var obj = sit.GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                     .First().Invoke(new object[] { new SerializedObject(it) });
        edAsm.GetTypes().First(t => t.Name == "ImageTargetEditor")
             .GetMethod("UpdateImageTargetData", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
             .Invoke(null, new[] { obj });
        return go;
    }

    // anclaVertical: 0 = los pies quedan en posLocal; 0.58 = el torso queda en posLocal (para envolver el cubo)
    static GameObject ColocarPersonaje(string p, Transform target, float altura, Vector3 posLocal, float rotY, float anclaVertical)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Raiz}/Modelos/{p}/{p}.fbx");
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, target);
        go.name = p;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        var anim = go.GetComponent<Animator>();
        if (anim == null) anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{Raiz}/Animators/{p}_Animator.controller");
        anim.applyRootMotion = false;
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        var b = MedirLocal(go);
        float s = altura / b.size.y;
        var rot = Quaternion.Euler(0f, rotY, 0f);
        var centro = new Vector3(b.center.x, b.min.y + b.size.y * anclaVertical, b.center.z);
        go.transform.localScale = Vector3.one * s;
        go.transform.localRotation = rot;
        go.transform.localPosition = posLocal - rot * (centro * s);
        return go;
    }

    // Caja que envuelve la malla real (bind pose) en el espacio local del personaje
    static Bounds MedirLocal(GameObject go)
    {
        bool hay = false; var b = new Bounds();
        void Agregar(Vector3 mundo)
        {
            var l = go.transform.InverseTransformPoint(mundo);
            if (!hay) { b = new Bounds(l, Vector3.zero); hay = true; } else b.Encapsulate(l);
        }
        foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var m = new Mesh();
            smr.BakeMesh(m, true);
            var t = smr.transform;
            foreach (var v in m.vertices) Agregar(t.position + t.rotation * v);
            Object.DestroyImmediate(m);
        }
        foreach (var mf in go.GetComponentsInChildren<MeshFilter>())
            foreach (var v in mf.sharedMesh.vertices) Agregar(mf.transform.TransformPoint(v));
        return b;
    }

    // Efecto pegado a un hueso (mano, etc.): se mueve con la animación. escalaMundo = tamaño final en el mundo.
    static GameObject AgregarVFXEnHueso(string nombrePrefab, Transform hueso, float escalaMundo, string nombre, bool loop = true, bool espacioMundo = false)
    {
        var v = AgregarVFX(nombrePrefab, hueso, Vector3.zero, 1f, nombre, loop, espacioMundo);
        v.transform.localScale = Vector3.one * (escalaMundo / hueso.lossyScale.x);
        v.transform.rotation = hueso.root.rotation;   // orientado "hacia arriba" del personaje
        return v;
    }

    static GameObject AgregarVFX(string nombrePrefab, Transform padre, Vector3 posLocal, float escala, string nombre, bool loop = true, bool espacioMundo = false)
    {
        var guid = AssetDatabase.FindAssets(nombrePrefab + " t:Prefab", new[] { "Assets/JMO Assets" })
            .First(g => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(g)) == nombrePrefab);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
        var v = (GameObject)PrefabUtility.InstantiatePrefab(prefab, padre);
        PrefabUtility.UnpackPrefabInstance(v, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        v.name = "VFX_" + nombre;
        v.transform.localPosition = posLocal;
        v.transform.localRotation = Quaternion.identity;
        v.transform.localScale = Vector3.one * escala;
        // Quitar el componente de CFXR que destruye/desactiva el efecto al terminar
        foreach (var c in v.GetComponentsInChildren<MonoBehaviour>(true).Where(c => c != null && c.GetType().Name == "CFXR_Effect").ToArray())
            Object.DestroyImmediate(c);
        foreach (var ps in v.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.loop = loop;
            main.playOnAwake = loop;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.simulationSpace = espacioMundo ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
            main.stopAction = ParticleSystemStopAction.None;
        }
        return v;
    }

    // Posiciones de los 3 sellos del dibujo del Mago (1086 x 1448 px -> 0.19 x 0.2533 m)
    static readonly (string nombre, Vector2 pixel)[] Sellos =
    {
        ("Spell", new Vector2(190, 1152)),
        ("Idle",  new Vector2(545, 1155)),
        ("Jump",  new Vector2(900, 1155)),
    };

    static void ConfigurarBotones(GameObject target, Animator animator, ParticleSystem efecto, Camera camara)
    {
        const float anchoImg = 1086f, altoImg = 1448f, anchoM = 0.19f, altoM = 0.19f * altoImg / anchoImg;
        var matMarco = MaterialUnlit("Marco_Boton", new Color(1f, 0.82f, 0.25f));
        var fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var botones = new Dictionary<string, BotonVirtualAR>();
        foreach (var (nombre, px) in Sellos)
        {
            var b = new GameObject("BotonVirtual_" + nombre);
            b.transform.SetParent(target.transform, false);
            b.transform.localPosition = new Vector3((px.x / anchoImg - 0.5f) * anchoM, 0f, (0.5f - px.y / altoImg) * altoM);
            var bv = b.AddComponent<BotonVirtualAR>();
            bv.nombreBoton = nombre;
            bv.tamano = new Vector2(0.04f, 0.04f);
            botones[nombre] = bv;

            // Marco dorado alrededor de la zona sensible (fuera del área que se analiza)
            const float lado = 0.052f, grosor = 0.0018f;
            for (int i = 0; i < 4; i++)
            {
                var barra = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(barra.GetComponent<Collider>());
                barra.name = "Marco";
                barra.transform.SetParent(b.transform, false);
                bool horizontal = i < 2;
                float signo = i % 2 == 0 ? 1f : -1f;
                barra.transform.localPosition = horizontal ? new Vector3(0f, 0.0005f, signo * lado / 2f) : new Vector3(signo * lado / 2f, 0.0005f, 0f);
                barra.transform.localScale = horizontal ? new Vector3(lado + grosor, 0.0008f, grosor) : new Vector3(grosor, 0.0008f, lado + grosor);
                barra.GetComponent<MeshRenderer>().sharedMaterial = matMarco;
            }

            // Etiqueta debajo del sello
            var etiqueta = new GameObject("Etiqueta");
            etiqueta.transform.SetParent(b.transform, false);
            etiqueta.transform.localPosition = new Vector3(0f, 0.001f, -0.036f);
            etiqueta.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var tm = etiqueta.AddComponent<TextMesh>();
            tm.text = nombre.ToUpper();
            tm.font = fuente;
            tm.fontSize = 64;
            tm.characterSize = 0.0012f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(1f, 0.9f, 0.4f);
            etiqueta.GetComponent<MeshRenderer>().sharedMaterial = fuente.material;
        }

        var det = target.AddComponent<DetectorBotonesVirtuales>();
        det.imageTarget = target.GetComponent<Vuforia.ImageTargetBehaviour>();
        det.dibujoReferencia = TexturaReferencia();
        det.camaraAR = camara;

        var ctrl = target.AddComponent<ControlBotonesMago>();
        ctrl.animatorMago = animator;
        ctrl.botonSpell = botones["Spell"];
        ctrl.botonIdle = botones["Idle"];
        ctrl.botonJump = botones["Jump"];
        ctrl.efectoHechizo = efecto;
    }

    static Texture2D TexturaReferencia()
    {
        const string ruta = Raiz + "/Texturas/Dibujo1_Mago_Referencia.jpg";
        if (!File.Exists(ruta))
        {
            Directory.CreateDirectory(Raiz + "/Texturas");
            File.Copy("Recursos/01_Dibujos/ImageTargets/Dibujo1_Mago_Vuforia.jpg", ruta);
            AssetDatabase.ImportAsset(ruta);
        }
        var ti = (TextureImporter)AssetImporter.GetAtPath(ruta);
        ti.isReadable = true;
        ti.mipmapEnabled = false;
        ti.maxTextureSize = 256;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(ruta);
    }

    static Material MaterialUnlit(string nombre, Color color)
    {
        var ruta = $"{Raiz}/Materiales/{nombre}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(m, ruta);
        }
        m.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(m);
        return m;
    }

    // ------------------------------------------------------------------ ANDROID
    [MenuItem("FantasyAR/4. Configurar Android")]
    public static string ConfigurarAndroid()
    {
        PlayerSettings.companyName = "FantasyAR";
        PlayerSettings.productName = "FantasyAR";
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.fantasyar.nivel2");
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)29;   // Vuforia 11 exige Android 10
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(RutaEscena, true) };
        bool ok = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android
                  || EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        return $"Android configurado. Plataforma activa = {EditorUserBuildSettings.activeBuildTarget} (cambio ok={ok})";
    }

    public const string RutaApk = "Recursos/07_APK/FantasyAR_Nivel2.apk";
    const string RutaResultadoBuild = "Recursos/07_APK/resultado_build.txt";

    [MenuItem("FantasyAR/5. Generar APK")]
    public static string GenerarApk()
    {
        Directory.CreateDirectory("Recursos/07_APK");
        File.WriteAllText(RutaResultadoBuild, "EN PROCESO " + System.DateTime.Now);
        {
            var r = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { RutaEscena },
                locationPathName = RutaApk,
                target = BuildTarget.Android,
                options = BuildOptions.None
            });
            var s = r.summary;
            var errores = string.Join("\n", r.steps.SelectMany(st => st.messages)
                .Where(m => m.type == LogType.Error || m.type == LogType.Exception).Select(m => m.content).Take(20));
            File.WriteAllText(RutaResultadoBuild,
                $"{s.result} | {s.totalSize / (1024 * 1024)} MB | {s.totalTime} | errores={s.totalErrors}\n{errores}");
        }
        return File.ReadAllText(RutaResultadoBuild);
    }

    static void Encadenar(AnimatorState desde, AnimatorState hacia, float exitTime)
    {
        var t = desde.AddTransition(hacia);
        t.hasExitTime = true;
        t.exitTime = exitTime;
        t.duration = 0.25f;
    }
}
