using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Scripts.CityScene;
using UnityEditor.VersionControl;

public static class CityInteractionSetup
{
    const string ScenePath = "Assets/Scenes/CityScene.unity";
    const string DefinitionFolder = "Assets/ScriptableObjects/City/Buildings";
    const string MarketDataFolder = "Assets/ScriptableObjects/City/Market";
    const string ShipyardDataFolder = "Assets/ScriptableObjects/City/Shipyard";
    const string SystemsRootName = "_CitySystems";
    const string UiRootName = "_CityUI";
    const string BuildingsRootName = "_buildings";

    static readonly Dictionary<string, BuildingPreset> Presets = new()
    {
        {
            "Shipyard",
            new BuildingPreset(
                "shipyard",
                "Shipyard",
                CityBuildingKind.Shipyard,
                "SY",
                new Color(0.93f, 0.72f, 0.42f, 1f),
                "Shipyard",
                "Commission hulls, review dock capacity, and prepare maritime upgrades.",
                -18f,
                24f,
                1.45f,
                36f)
        },
        {
            "GuildHouse",
            new BuildingPreset(
                "guild_house",
                "Guild House",
                CityBuildingKind.GuildHouse,
                "GH",
                new Color(0.55f, 0.72f, 1f, 1f),
                "Guild House",
                "Coordinate guild contracts, civic favors, and reputation-driven opportunities.",
                6f,
                23f,
                1.25f,
                34f)
        },
        {
            "CountingHouse",
            new BuildingPreset(
                "counting_house",
                "Counting House",
                CityBuildingKind.CountingHouse,
                "CH",
                new Color(0.92f, 0.84f, 0.55f, 1f),
                "Counting House",
                "Track trade ledgers, stored goods, tariffs, and financial throughput.",
                -4f,
                22f,
                1.22f,
                33f)
        },
        {
            "Bank",
            new BuildingPreset(
                "bank",
                "Bank",
                CityBuildingKind.Bank,
                "BK",
                new Color(0.78f, 0.9f, 0.64f, 1f),
                "Bank",
                "Manage reserves, credit, loans, and long-term city investment.",
                16f,
                23f,
                1.18f,
                32f)
        },
        {
            "Market",
            new BuildingPreset(
                "market",
                "Market",
                CityBuildingKind.Market,
                "MK",
                new Color(0.94f, 0.64f, 0.46f, 1f),
                "Market",
                "Inspect local demand, commodity prices, and available merchant actions.",
                -10f,
                21f,
                1.2f,
                34f)
        },
        {
            "Tavern",
            new BuildingPreset(
                "tavern",
                "Tavern",
                CityBuildingKind.Tavern,
                "TV",
                new Color(0.93f, 0.56f, 0.82f, 1f),
                "Tavern",
                "Hear rumors, recruit specialists, and review social city events.",
                -24f,
                24f,
                1.2f,
                34f)
        },
        {
            "Church",
            new BuildingPreset(
                "church",
                "Church",
                CityBuildingKind.Church,
                "CH",
                new Color(0.86f, 0.88f, 1f, 1f),
                "Church",
                "Review faith, morale, ceremonies, and public-order modifiers.",
                12f,
                26f,
                1.35f,
                35f)
        }
    };

    static readonly FooterPreset[] FooterPresets =
    {
        new("market", "Market", "market", true, "MK"),
        new("shipyard", "Shipyard", "shipyard", true, "SY"),
        new("counting_house", "Counting\nhouse", "counting_house", true, "CH"),
        new("business_management", "Business\nmanagement", "counting_house", true, "BM"),
        new("guild_house", "Guild\nhouse", "guild_house", false, "GH"),
        new("bank", "Bank", "bank", false, "BK")
    };

    [MenuItem("Tools/City/Setup Interaction Foundation")]
    public static void SetupCitySceneInteractionFoundation()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Transform buildingsRoot = GameObject.Find(BuildingsRootName)?.transform;
        if (buildingsRoot == null)
        {
            Debug.LogError($"City setup failed: missing {BuildingsRootName} root in {ScenePath}.");
            return;
        }

        EnsureFolder(DefinitionFolder);
        EnsureFolder(MarketDataFolder);
        EnsureFolder(ShipyardDataFolder);
        MarketTradePlaceConfig marketConfig = EnsureMarketTradePlaceConfig();
        ShipyardConfig shipyardConfig = EnsureShipyardConfig();
        List<CityBuilding> buildings = WrapBuildings(buildingsRoot);
        Camera camera = EnsureCamera(scene, buildings);
        CameraPose initialCameraPose = new CameraPose(camera.transform.position, camera.transform.rotation, camera.fieldOfView);
        CityCameraController cameraController = EnsureCameraController(camera, buildings);
        CityUIView cityView = EnsureCityUi(scene, marketConfig, shipyardConfig);
        CityInteractionController interactionController = EnsureInteractionController(scene, camera, cameraController, cityView);
        DefaultCameraViewsFromMainCameraOnce(scene, initialCameraPose, buildings);

        cityView.Bind(interactionController);
        cityView.SetCityName("Rome");
        cityView.SetBuildings(buildings);
        interactionController.Configure(camera, cameraController, cityView);

        EnableCameraPostProcessing(camera);
        FixKnownTextureImports();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log($"City interaction foundation ready. Wrapped {buildings.Count} buildings and created camera/UI systems.");
    }

    [MenuItem("Tools/City/Setup UI Panels Only")]
    public static void SetupCityUiPanelsOnly()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        EnsureFolder(MarketDataFolder);
        EnsureFolder(ShipyardDataFolder);
        MarketTradePlaceConfig marketConfig = EnsureMarketTradePlaceConfig();
        ShipyardConfig shipyardConfig = EnsureShipyardConfig();
        EnsureCityUi(scene, marketConfig, shipyardConfig);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("City UI panels ready. Camera and building wrappers were not touched.");
    }

    [MenuItem("Tools/City/Reset Building Camera Views From Main Camera")]
    public static void ResetBuildingCameraViewsFromMainCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("City camera reset failed: no Main Camera found in the open scene.");
            return;
        }

        CityBuilding[] buildings = Object.FindObjectsByType<CityBuilding>(FindObjectsInactive.Exclude);
        CopyMainCameraToBuildingCameraViews(camera, buildings, true);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"Copied Main Camera pose/FOV to {buildings.Length} building camera views.");
    }

    static List<CityBuilding> WrapBuildings(Transform buildingsRoot)
    {
        List<CityBuilding> result = new();

        foreach (BuildingPreset preset in Presets.Values)
        {
            CityBuilding building = FindExistingBuilding(buildingsRoot, preset);
            if (building == null)
            {
                Transform art = FindDirectChild(buildingsRoot, preset.SceneObjectName);
                if (art == null)
                {
                    Debug.LogWarning($"City setup skipped missing building art: {preset.SceneObjectName}");
                    continue;
                }

                building = CreateBuildingWrapper(buildingsRoot, art, preset);
            }

            CityBuildingDefinition definition = EnsureDefinition(preset);
            building.BindDefinition(definition);
            RefreshBuildingReferences(building, preset);
            EditorUtility.SetDirty(building);
            result.Add(building);
        }

        return result;
    }

    static CityBuilding FindExistingBuilding(Transform buildingsRoot, BuildingPreset preset)
    {
        CityBuilding[] buildings = buildingsRoot.GetComponentsInChildren<CityBuilding>(true);
        foreach (CityBuilding building in buildings)
        {
            if (building.Definition != null && building.Definition.Id == preset.Id)
            {
                return building;
            }

            if (building.name == $"CityBuilding_{preset.SceneObjectName}")
            {
                return building;
            }
        }

        return null;
    }

    static Transform FindDirectChild(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name)
            {
                return child;
            }
        }

        return null;
    }

    static CityBuilding CreateBuildingWrapper(Transform buildingsRoot, Transform art, BuildingPreset preset)
    {
        Bounds bounds = CalculateBounds(art);
        GameObject wrapper = new GameObject($"CityBuilding_{preset.SceneObjectName}");
        wrapper.transform.SetParent(buildingsRoot, true);
        wrapper.transform.position = bounds.center;
        wrapper.transform.rotation = Quaternion.identity;
        wrapper.transform.localScale = Vector3.one;

        art.SetParent(wrapper.transform, true);
        CityBuilding building = wrapper.AddComponent<CityBuilding>();
        return building;
    }

    static void RefreshBuildingReferences(CityBuilding building, BuildingPreset preset)
    {
        Transform artRoot = null;
        for (int i = 0; i < building.transform.childCount; i++)
        {
            Transform child = building.transform.GetChild(i);
            if (child.name == preset.SceneObjectName)
            {
                artRoot = child;
                break;
            }
        }

        if (artRoot == null)
        {
            artRoot = building.transform;
        }

        Bounds bounds = CalculateBounds(artRoot);
        Transform focusTarget = GetOrCreateChild(building.transform, "FocusTarget");
        focusTarget.position = bounds.center + Vector3.up * Mathf.Max(1.5f, bounds.extents.y * 0.16f);

        Transform iconAnchor = GetOrCreateChild(building.transform, "IconAnchor");
        iconAnchor.position = bounds.center + Vector3.up * (bounds.extents.y + 6f);

        Transform cameraView = EnsureCameraView(building.transform, bounds, focusTarget, preset, out float cameraViewFov);

        BoxCollider collider = building.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = building.gameObject.AddComponent<BoxCollider>();
        }

        collider.center = building.transform.InverseTransformPoint(bounds.center);
        collider.size = new Vector3(
            Mathf.Max(bounds.size.x, 4f),
            Mathf.Max(bounds.size.y, 6f),
            Mathf.Max(bounds.size.z, 4f)) + new Vector3(3f, 4f, 3f);

        RemoveWorldIcon(iconAnchor);
        building.SetReferences(artRoot, focusTarget, iconAnchor, cameraView, cameraViewFov, collider);
    }

    static void RemoveWorldIcon(Transform anchor)
    {
        if (anchor == null)
        {
            return;
        }

        Transform existing = anchor.Find("WorldIcon");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }
    }

    static Transform EnsureCameraView(Transform buildingRoot, Bounds bounds, Transform focusTarget, BuildingPreset preset, out float cameraViewFov)
    {
        Transform cameraView = GetOrCreatePlainChild(buildingRoot, "CameraView", out bool created);
        Camera markerCamera = cameraView.GetComponent<Camera>();
        bool addedCamera = false;
        if (markerCamera == null)
        {
            markerCamera = cameraView.gameObject.AddComponent<Camera>();
            addedCamera = true;
        }

        markerCamera.enabled = false;
        markerCamera.nearClipPlane = 0.3f;
        markerCamera.farClipPlane = 500f;

        if (created || addedCamera)
        {
            markerCamera.fieldOfView = preset.CameraFov;
        }

        cameraViewFov = markerCamera.fieldOfView;

        if (!created)
        {
            return cameraView;
        }

        Vector3 focus = focusTarget.position + Vector3.up * 1.5f;
        float distance = Mathf.Clamp(bounds.extents.magnitude * preset.CameraDistanceMultiplier, 38f, 135f);
        Quaternion orbit = Quaternion.Euler(preset.CameraPitch, preset.CameraYaw, 0f);

        cameraView.position = focus + orbit * new Vector3(0f, 0f, -distance);
        cameraView.rotation = Quaternion.LookRotation(focus - cameraView.position, Vector3.up);
        return cameraView;
    }

    static Camera EnsureCamera(Scene scene, IReadOnlyList<CityBuilding> buildings)
    {
        Camera camera = Camera.main;
        if (camera != null)
        {
            return camera;
        }

        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        SceneManager.MoveGameObjectToScene(cameraObject, scene);
        cameraObject.tag = "MainCamera";
        return cameraObject.GetComponent<Camera>();
    }

    static CityCameraController EnsureCameraController(Camera camera, IReadOnlyList<CityBuilding> buildings)
    {
        CityCameraController controller = camera.GetComponent<CityCameraController>();
        if (controller == null)
        {
            controller = camera.gameObject.AddComponent<CityCameraController>();
        }

        Bounds cityBounds = CalculateCityBounds(buildings);
        Vector3 focus = cityBounds.center;
        focus.y = Mathf.Max(18f, cityBounds.center.y * 0.45f);
        float distance = Mathf.Clamp(Mathf.Max(cityBounds.size.x, cityBounds.size.z) * 0.62f, 145f, 320f);
        controller.ConfigureDefaultView(focus, 0f, 28f, distance, 42f);
        EditorUtility.SetDirty(controller);
        return controller;
    }

    static void DefaultCameraViewsFromMainCameraOnce(Scene scene, CameraPose cameraPose, IReadOnlyList<CityBuilding> buildings)
    {
        if (buildings == null || buildings.Count == 0)
        {
            return;
        }

        Transform systemsRoot = GetOrCreateRoot(scene, SystemsRootName);
        if (systemsRoot.Find("_CameraViewsDefaultedFromMainCamera") != null)
        {
            return;
        }

        CopyCameraPoseToBuildingCameraViews(cameraPose, buildings, false);

        GameObject marker = new GameObject("_CameraViewsDefaultedFromMainCamera");
        marker.transform.SetParent(systemsRoot, false);
        marker.hideFlags = HideFlags.HideInHierarchy;
        EditorUtility.SetDirty(systemsRoot);
    }

    static void CopyMainCameraToBuildingCameraViews(Camera camera, IReadOnlyList<CityBuilding> buildings, bool recordUndo)
    {
        if (camera == null)
        {
            return;
        }

        CopyCameraPoseToBuildingCameraViews(new CameraPose(camera.transform.position, camera.transform.rotation, camera.fieldOfView), buildings, recordUndo);
    }

    static void CopyCameraPoseToBuildingCameraViews(CameraPose cameraPose, IReadOnlyList<CityBuilding> buildings, bool recordUndo)
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            CityBuilding building = buildings[i];
            if (building == null || building.CameraView == null)
            {
                continue;
            }

            Transform cameraView = building.CameraView;
            if (recordUndo)
            {
                Undo.RecordObject(cameraView, "Reset City Building Camera View");
                Undo.RecordObject(building, "Reset City Building Camera View");
            }

            cameraView.SetPositionAndRotation(cameraPose.Position, cameraPose.Rotation);

            Camera markerCamera = cameraView.GetComponent<Camera>();
            if (markerCamera != null)
            {
                if (recordUndo)
                {
                    Undo.RecordObject(markerCamera, "Reset City Building Camera View");
                }

                markerCamera.fieldOfView = cameraPose.FieldOfView;
                EditorUtility.SetDirty(markerCamera);
            }

            EditorUtility.SetDirty(cameraView);
            EditorUtility.SetDirty(building);
        }
    }

    static CityInteractionController EnsureInteractionController(Scene scene, Camera camera, CityCameraController cameraController, CityUIView cityView)
    {
        Transform systemsRoot = GetOrCreateRoot(scene, SystemsRootName);
        Transform controllerTransform = GetOrCreateChild(systemsRoot, "CityInteractionController");
        CityInteractionController controller = controllerTransform.GetComponent<CityInteractionController>();
        if (controller == null)
        {
            controller = controllerTransform.gameObject.AddComponent<CityInteractionController>();
        }

        controller.Configure(camera, cameraController, cityView);
        EditorUtility.SetDirty(controller);
        return controller;
    }

    static MarketTradePlaceConfig EnsureMarketTradePlaceConfig()
    {
        MarketGoodsCatalog cityMarketGoods = EnsureMarketGoodsCatalog(
            "city_market_goods",
            "grain",
            "timber",
            "stone",
            "tools",
            "cloth",
            "wine",
            "spices",
            "iron");

        MarketGoodsCatalog shipGoods = EnsureMarketGoodsCatalog(
            "ship_name_goods",
            "grain",
            "cloth",
            "tools",
            "spices",
            "wine");

        MarketGoodsCatalog warehouseGoods = EnsureMarketGoodsCatalog(
            "counting_warehouse_goods",
            "grain",
            "timber",
            "stone",
            "iron",
            "cloth",
            "salt");

        string assetPath = $"{MarketDataFolder}/market_trade_places.asset";
        MarketTradePlaceConfig config = AssetDatabase.LoadAssetAtPath<MarketTradePlaceConfig>(assetPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<MarketTradePlaceConfig>();
            AssetDatabase.CreateAsset(config, assetPath);
        }

        config.AddDefaultPlaces(
            new MarketTradePlace("city_market", "CityMarket", null, cityMarketGoods),
            new MarketTradePlace("ship_name", "ShipName", null, shipGoods),
            new MarketTradePlace("counting_warehouse", "Counting/Warehouse", null, warehouseGoods));

        EditorUtility.SetDirty(config);
        return config;
    }

    static MarketGoodsCatalog EnsureMarketGoodsCatalog(string assetName, params string[] defaultGoodIds)
    {
        string assetPath = $"{MarketDataFolder}/{assetName}.asset";
        MarketGoodsCatalog catalog = AssetDatabase.LoadAssetAtPath<MarketGoodsCatalog>(assetPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<MarketGoodsCatalog>();
            AssetDatabase.CreateAsset(catalog, assetPath);
        }

        catalog.AddDefaultGoods(defaultGoodIds);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    static ShipyardConfig EnsureShipyardConfig()
    {
        string assetPath = $"{ShipyardDataFolder}/shipyard_config.asset";
        ShipyardConfig config = AssetDatabase.LoadAssetAtPath<ShipyardConfig>(assetPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<ShipyardConfig>();
            AssetDatabase.CreateAsset(config, assetPath);
        }

        ShipyardMaterialRequirement[] smallTradeMaterials =
        {
            new("timber", "Timber", null, "500"),
            new("cloth", "Cloth", null, "180"),
            new("tools", "Tools", null, "25"),
            new("iron", "Iron", null, "400"),
            new("tar", "Tar", null, "350"),
            new("provisions", "Provisions", null, "20")
        };

        ShipyardMaterialRequirement[] mediumTradeMaterials =
        {
            new("timber", "Timber", null, "650"),
            new("cloth", "Cloth", null, "220"),
            new("tools", "Tools", null, "40"),
            new("iron", "Iron", null, "520"),
            new("tar", "Tar", null, "420"),
            new("provisions", "Provisions", null, "30")
        };

        ShipyardMaterialRequirement[] heavyTradeMaterials =
        {
            new("timber", "Timber", null, "820"),
            new("cloth", "Cloth", null, "260"),
            new("tools", "Tools", null, "60"),
            new("iron", "Iron", null, "700"),
            new("tar", "Tar", null, "520"),
            new("provisions", "Provisions", null, "45")
        };

        ShipyardMaterialRequirement[] frigateMaterials =
        {
            new("timber", "Timber", null, "1000"),
            new("cloth", "Cloth", null, "340"),
            new("tools", "Tools", null, "90"),
            new("iron", "Iron", null, "900"),
            new("tar", "Tar", null, "700"),
            new("cannon", "Cannon", null, "24")
        };

        ShipyardShipDefinition[] defaultShips =
        {
            new("trade_cog", "Trade cog", "Trade", null, "3", "100", "5", "500", "20/day", "120,000", "August 11, 1228", smallTradeMaterials),
            new("merchant_fluyt", "Merchant Fluyt", "Trade", null, "2", "150", "12", "1,500", "110/day", "51,000", "August 29, 1228", mediumTradeMaterials),
            new("cargo_clipper", "Cargo Clipper", "Trade", null, "10", "120", "35", "3,500", "140/day", "87,000", "September 29, 1228", heavyTradeMaterials),
            new("frigate", "Frigate", "Battle", null, "7", "400", "55", "15", "200/day", "120,000", "October 29, 1228", frigateMaterials)
        };

        ShipyardConstructionEntry[] defaultQueue =
        {
            new(
                "current_trade_cog",
                "Trade cog",
                "You",
                "August 11, 1228",
                "Some materials are missing in the city. Construction will continue once materials are available.",
                0.64f,
                new[]
                {
                    new ShipyardMaterialRequirement("timber", "Timber", null, "320/500", true),
                    new ShipyardMaterialRequirement("cloth", "Cloth", null, "180/180"),
                    new ShipyardMaterialRequirement("tools", "Tools", null, "25/25")
                }),
            new("frigate_queue", "Frigate", "username1", "August 29, 1228", string.Empty, 0.18f, null),
            new("trade_cog_queue_1", "Trade cog", "username2", "September 29, 1228", string.Empty, 0.1f, null),
            new("trade_cog_queue_2", "Trade cog", "username1", "October 29, 1228", string.Empty, 0.06f, null),
            new("cargo_clipper_queue", "Cargo Clipper", "username3", "November 29, 1228", string.Empty, 0.03f, null),
            new("frigate_queue_2", "Frigate", "username4", "December 29, 1228", string.Empty, 0.01f, null)
        };

        ShipyardBuyOffer[] defaultBuyOffers =
        {
            new("trade_cog_offer", "Trade cog", "Trade", null, "3", "100", "5", "500", "20/day", "35,000", "1"),
            new("merchant_fluyt_offer", "Merchant Fluyt", "Trade", null, "2", "150", "12", "1,500", "110/day", "51,000", "0"),
            new("cargo_clipper_offer", "Cargo Clipper", "Trade", null, "10", "120", "35", "3,500", "140/day", "87,000", "0"),
            new("frigate_offer", "Frigate", "Battle", null, "7", "400", "55", "15", "200/day", "120,000", "0")
        };

        config.AddDefaultData(defaultShips, defaultQueue, defaultBuyOffers);
        EditorUtility.SetDirty(config);
        return config;
    }

    static CityUIView EnsureCityUi(Scene scene, MarketTradePlaceConfig marketConfig, ShipyardConfig shipyardConfig)
    {
        Transform uiRoot = GetOrCreateRoot(scene, UiRootName);
        Canvas canvas = uiRoot.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = uiRoot.gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = uiRoot.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = uiRoot.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1366f, 768f);
        scaler.matchWidthOrHeight = 0.5f;

        if (uiRoot.GetComponent<GraphicRaycaster>() == null)
        {
            uiRoot.gameObject.AddComponent<GraphicRaycaster>();
        }

        EnsureEventSystem(scene);

        CityUIView view = uiRoot.GetComponent<CityUIView>();
        if (view == null)
        {
            view = uiRoot.gameObject.AddComponent<CityUIView>();
        }

        RectTransform canvasRect = (RectTransform)uiRoot.transform;
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;

        DestroyChildIfExists(canvasRect, "TopBar");
        DestroyChildIfExists(canvasRect, "HeaderStack");
        DestroyChildIfExists(canvasRect, "BuildingPanel");

        RectTransform footer = EnsureFooterBar(canvasRect);
        PopupRefs popup = EnsurePopup(canvasRect, marketConfig, shipyardConfig);

        SerializedObject so = new SerializedObject(view);
        so.FindProperty("footerRoot").objectReferenceValue = footer;
        so.FindProperty("popupRoot").objectReferenceValue = popup.Root;
        so.FindProperty("placeholderRoot").objectReferenceValue = popup.PlaceholderRoot;
        so.FindProperty("marketPopupView").objectReferenceValue = popup.MarketView;
        so.FindProperty("shipyardPopupView").objectReferenceValue = popup.ShipyardView;
        so.FindProperty("popupGroup").objectReferenceValue = popup.Group;
        so.FindProperty("popupTitleText").objectReferenceValue = popup.Title;
        so.FindProperty("closeButton").objectReferenceValue = popup.CloseButton;
        so.FindProperty("defaultPopupSize").vector2Value = new Vector2(520f, 280f);
        so.FindProperty("marketPopupSize").vector2Value = new Vector2(1320f, 600f);
        so.FindProperty("shipyardPopupSize").vector2Value = new Vector2(1040f, 520f);
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(view);
        return view;
    }

    static RectTransform EnsureFooterBar(RectTransform canvasRect)
    {
        Transform existing = canvasRect.Find("FooterBar");
        RectTransform footer;
        if (existing == null)
        {
            GameObject footerObject = new GameObject("FooterBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            footer = footerObject.GetComponent<RectTransform>();
            footer.SetParent(canvasRect, false);
        }
        else
        {
            footer = (RectTransform)existing;
        }

        footer.anchorMin = new Vector2(0.5f, 0f);
        footer.anchorMax = new Vector2(0.5f, 0f);
        footer.pivot = new Vector2(0.5f, 0f);
        footer.anchoredPosition = new Vector2(0f, 14f);
        footer.sizeDelta = new Vector2(690f, 106f);

        Image background = footer.GetComponent<Image>();
        background.color = new Color(0.025f, 0.024f, 0.022f, 0.94f);

        HorizontalLayoutGroup layout = footer.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = footer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        layout.padding = new RectOffset(18, 18, 10, 10);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Dictionary<string, FooterButtonSnapshot> snapshots = CaptureFooterButtonSnapshots(footer);
        for (int i = footer.childCount - 1; i >= 0; i--)
        {
            Transform child = footer.GetChild(i);
            Object.DestroyImmediate(child.gameObject);
        }

        for (int i = 0; i < FooterPresets.Length; i++)
        {
            snapshots.TryGetValue(FooterPresets[i].EntryId, out FooterButtonSnapshot snapshot);
            EnsureFooterButton(footer, FooterPresets[i], i, snapshot);
        }

        return footer;
    }

    static Dictionary<string, FooterButtonSnapshot> CaptureFooterButtonSnapshots(RectTransform footer)
    {
        Dictionary<string, FooterButtonSnapshot> snapshots = new();
        if (footer == null)
        {
            return snapshots;
        }

        CityFooterButton[] buttons = footer.GetComponentsInChildren<CityFooterButton>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            CityFooterButton button = buttons[i];
            if (button == null || string.IsNullOrWhiteSpace(button.EntryId))
            {
                continue;
            }

            snapshots[button.EntryId] = FooterButtonSnapshot.Capture(button);
        }

        return snapshots;
    }

    static void EnsureFooterButton(RectTransform footer, FooterPreset preset, int siblingIndex, FooterButtonSnapshot snapshot)
    {
        Transform existing = footer.Find(preset.ObjectName);
        RectTransform rect;
        Image backplate;
        Button button;
        CityFooterButton footerButton;

        if (existing == null)
        {
            GameObject buttonObject = new GameObject(preset.ObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(CityFooterButton));
            rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(footer, false);
        }
        else
        {
            rect = (RectTransform)existing;
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(rect.gameObject);
        }

        rect.sizeDelta = new Vector2(96f, 86f);
        rect.SetSiblingIndex(siblingIndex);

        LayoutElement layoutElement = rect.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = rect.gameObject.AddComponent<LayoutElement>();
        }

        layoutElement.preferredWidth = 96f;
        layoutElement.preferredHeight = 86f;

        backplate = rect.GetComponent<Image>();
        backplate.color = preset.IsAvailable
            ? new Color(0.025f, 0.024f, 0.022f, 0.94f)
            : new Color(0.018f, 0.018f, 0.018f, 0.86f);

        button = rect.GetComponent<Button>();
        if (button == null)
        {
            button = rect.gameObject.AddComponent<Button>();
        }

        button.transition = Selectable.Transition.None;

        footerButton = rect.GetComponent<CityFooterButton>();
        if (footerButton == null)
        {
            footerButton = rect.gameObject.AddComponent<CityFooterButton>();
        }

        Image icon = EnsureChildImage(rect, "Icon", new Color(0.86f, 0.75f, 0.55f, 1f));
        RectTransform iconRect = (RectTransform)icon.transform;
        iconRect.anchorMin = new Vector2(0.5f, 1f);
        iconRect.anchorMax = new Vector2(0.5f, 1f);
        iconRect.pivot = new Vector2(0.5f, 1f);
        iconRect.anchoredPosition = new Vector2(0f, -7f);
        iconRect.sizeDelta = new Vector2(54f, 54f);
        icon.raycastTarget = false;
        icon.enabled = false;

        Text glyph = EnsureChildText(iconRect, "Glyph", preset.Glyph, 18, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.86f, 0.75f, 0.55f, 1f));
        Stretch((RectTransform)glyph.transform);

        Text label = EnsureChildText(rect, "Label", preset.Label, 12, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.86f, 0.75f, 0.55f, 1f));
        RectTransform labelRect = (RectTransform)label.transform;
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.anchoredPosition = new Vector2(0f, 5f);
        labelRect.sizeDelta = new Vector2(-8f, 30f);

        footerButton.Configure(preset.EntryId, preset.Label, preset.TargetBuildingId, preset.IsAvailable);
        footerButton.SetReferences(button, backplate, icon, glyph, label);
        snapshot.ApplyTo(footerButton);
        EditorUtility.SetDirty(footerButton);
    }

    static PopupRefs EnsurePopup(RectTransform canvasRect, MarketTradePlaceConfig marketConfig, ShipyardConfig shipyardConfig)
    {
        Transform existing = canvasRect.Find("BuildingPopup");
        RectTransform popup;
        if (existing == null)
        {
            GameObject popupObject = new GameObject("BuildingPopup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            popup = popupObject.GetComponent<RectTransform>();
            popup.SetParent(canvasRect, false);
        }
        else
        {
            popup = (RectTransform)existing;
        }

        popup.anchorMin = new Vector2(0.5f, 0.5f);
        popup.anchorMax = new Vector2(0.5f, 0.5f);
        popup.pivot = new Vector2(0.5f, 0.5f);
        popup.anchoredPosition = Vector2.zero;
        popup.sizeDelta = new Vector2(520f, 280f);

        Image background = popup.GetComponent<Image>();
        background.color = new Color(0.035f, 0.034f, 0.03f, 0.94f);

        CanvasGroup group = popup.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        Text title = EnsureChildText(popup, "Title", "Building", 26, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.96f, 0.82f, 0.58f, 1f));
        RectTransform titleRect = (RectTransform)title.transform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(24f, -20f);
        titleRect.sizeDelta = new Vector2(-84f, 40f);

        Button closeButton = EnsureCloseButton(popup);

        Transform placeholder = popup.Find("Placeholder");
        if (placeholder == null)
        {
            GameObject placeholderObject = new GameObject("Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            placeholder = placeholderObject.transform;
            placeholder.SetParent(popup, false);
        }

        RectTransform placeholderRect = (RectTransform)placeholder;
        placeholderRect.anchorMin = new Vector2(0f, 0f);
        placeholderRect.anchorMax = new Vector2(1f, 1f);
        placeholderRect.pivot = new Vector2(0.5f, 0.5f);
        placeholderRect.offsetMin = new Vector2(24f, 24f);
        placeholderRect.offsetMax = new Vector2(-24f, -78f);
        Image placeholderImage = placeholderRect.GetComponent<Image>();
        placeholderImage.color = new Color(0.08f, 0.075f, 0.066f, 0.25f);
        placeholder.gameObject.SetActive(true);

        MarketPopupView marketView = EnsureMarketPanel(popup, marketConfig);
        marketView.gameObject.SetActive(false);
        ShipyardPopupView shipyardView = EnsureShipyardPanel(popup, shipyardConfig);
        shipyardView.gameObject.SetActive(false);
        closeButton.transform.SetAsLastSibling();

        return new PopupRefs(popup, group, title, closeButton, placeholderRect, marketView, shipyardView);
    }

    static MarketPopupView EnsureMarketPanel(RectTransform popup, MarketTradePlaceConfig marketConfig)
    {
        RectTransform panel = EnsureChildRect(popup, "MarketPanel");
        Stretch(panel);

        Image background = panel.GetComponent<Image>();
        if (background == null)
        {
            background = panel.gameObject.AddComponent<Image>();
        }

        background.color = new Color(0.035f, 0.034f, 0.03f, 0.96f);
        background.raycastTarget = true;

        MarketPopupView view = panel.GetComponent<MarketPopupView>();
        if (view == null)
        {
            view = panel.gameObject.AddComponent<MarketPopupView>();
        }

        RectTransform leftPane = EnsureChildRect(panel, "MarketWorkspace");
        leftPane.anchorMin = new Vector2(0f, 0f);
        leftPane.anchorMax = new Vector2(1f, 1f);
        leftPane.pivot = new Vector2(0f, 0.5f);
        leftPane.offsetMin = new Vector2(0f, 0f);
        leftPane.offsetMax = new Vector2(-320f, 0f);

        RectTransform sidePanel = EnsureChildRect(panel, "TradePanel");
        sidePanel.anchorMin = new Vector2(1f, 0f);
        sidePanel.anchorMax = new Vector2(1f, 1f);
        sidePanel.pivot = new Vector2(1f, 0.5f);
        sidePanel.anchoredPosition = Vector2.zero;
        sidePanel.sizeDelta = new Vector2(320f, 0f);
        Image sideBackground = sidePanel.GetComponent<Image>();
        if (sideBackground == null)
        {
            sideBackground = sidePanel.gameObject.AddComponent<Image>();
        }

        sideBackground.color = new Color(0.025f, 0.024f, 0.022f, 0.98f);

        MarketTradeSelector leftSelector = EnsureMarketSelector(leftPane, "LeftSelector", new Vector2(24f, -22f), 450f, "CityMarket");
        MarketTradeSelector rightSelector = EnsureMarketSelector(leftPane, "RightSelector", new Vector2(526f, -22f), 450f, "ShipName");

        Text scaleText = EnsureChildText(leftPane, "ScaleIcon", "SC", 20, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.5f, 0.5f, 0.52f, 1f));
        RectTransform scaleRect = (RectTransform)scaleText.transform;
        scaleRect.anchorMin = new Vector2(0f, 1f);
        scaleRect.anchorMax = new Vector2(0f, 1f);
        scaleRect.pivot = new Vector2(0.5f, 1f);
        scaleRect.anchoredPosition = new Vector2(500f, -38f);
        scaleRect.sizeDelta = new Vector2(44f, 44f);

        EnsureMarketColumnHeaders(leftPane);
        MarketGoodsRow rowTemplate = EnsureMarketRows(leftPane, out RectTransform rowsRoot);
        EnsureTradePanel(sidePanel, out Text tradePlaceholder, out Text totalText, out Button confirmButton, out Button cancelButton);

        view.SetConfig(marketConfig);
        view.SetReferences(leftSelector, rightSelector, rowsRoot, rowTemplate, tradePlaceholder, totalText, confirmButton, cancelButton);
        EditorUtility.SetDirty(view);
        return view;
    }

    static MarketTradeSelector EnsureMarketSelector(RectTransform parent, string name, Vector2 anchoredPosition, float width, string label)
    {
        RectTransform selectorRect = EnsureChildRect(parent, name);
        selectorRect.anchorMin = new Vector2(0f, 1f);
        selectorRect.anchorMax = new Vector2(0f, 1f);
        selectorRect.pivot = new Vector2(0f, 1f);
        selectorRect.anchoredPosition = anchoredPosition;
        selectorRect.sizeDelta = new Vector2(width, 78f);

        Image selectorImage = selectorRect.GetComponent<Image>();
        if (selectorImage == null)
        {
            selectorImage = selectorRect.gameObject.AddComponent<Image>();
        }

        selectorImage.color = new Color(0.09f, 0.09f, 0.085f, 1f);

        Button selectorButton = selectorRect.GetComponent<Button>();
        if (selectorButton == null)
        {
            selectorButton = selectorRect.gameObject.AddComponent<Button>();
        }

        selectorButton.transition = Selectable.Transition.ColorTint;
        selectorButton.targetGraphic = selectorImage;

        Image icon = EnsureChildImage(selectorRect, "Icon", new Color(0.95f, 0.82f, 0.55f, 1f));
        RectTransform iconRect = (RectTransform)icon.transform;
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(26f, 0f);
        iconRect.sizeDelta = new Vector2(36f, 36f);
        icon.raycastTarget = false;
        icon.enabled = false;

        Text selectedLabel = EnsureChildText(selectorRect, "SelectedLabel", label, 24, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.98f, 0.82f, 0.55f, 1f));
        RectTransform selectedLabelRect = (RectTransform)selectedLabel.transform;
        selectedLabelRect.anchorMin = new Vector2(0f, 0f);
        selectedLabelRect.anchorMax = new Vector2(1f, 1f);
        selectedLabelRect.offsetMin = new Vector2(76f, 0f);
        selectedLabelRect.offsetMax = new Vector2(-56f, 0f);

        Text chevron = EnsureChildText(selectorRect, "Chevron", "v", 18, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.62f, 0.62f, 0.64f, 1f));
        RectTransform chevronRect = (RectTransform)chevron.transform;
        chevronRect.anchorMin = new Vector2(1f, 0.5f);
        chevronRect.anchorMax = new Vector2(1f, 0.5f);
        chevronRect.pivot = new Vector2(1f, 0.5f);
        chevronRect.anchoredPosition = new Vector2(-26f, 1f);
        chevronRect.sizeDelta = new Vector2(34f, 34f);

        RectTransform optionsRoot = EnsureChildRect(selectorRect, "Options");
        optionsRoot.anchorMin = new Vector2(0f, 0f);
        optionsRoot.anchorMax = new Vector2(1f, 0f);
        optionsRoot.pivot = new Vector2(0.5f, 1f);
        optionsRoot.anchoredPosition = new Vector2(0f, -6f);
        optionsRoot.sizeDelta = new Vector2(0f, 112f);
        Image optionsBackground = optionsRoot.GetComponent<Image>();
        if (optionsBackground == null)
        {
            optionsBackground = optionsRoot.gameObject.AddComponent<Image>();
        }

        optionsBackground.color = new Color(0.055f, 0.054f, 0.05f, 0.98f);

        VerticalLayoutGroup optionsLayout = optionsRoot.GetComponent<VerticalLayoutGroup>();
        if (optionsLayout == null)
        {
            optionsLayout = optionsRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        optionsLayout.padding = new RectOffset(6, 6, 6, 6);
        optionsLayout.spacing = 4f;
        optionsLayout.childControlWidth = true;
        optionsLayout.childControlHeight = false;
        optionsLayout.childForceExpandWidth = true;
        optionsLayout.childForceExpandHeight = false;

        Button optionTemplate = EnsureSelectorOptionTemplate(optionsRoot);
        MarketTradeSelector selector = selectorRect.GetComponent<MarketTradeSelector>();
        if (selector == null)
        {
            selector = selectorRect.gameObject.AddComponent<MarketTradeSelector>();
        }

        selector.SetReferences(selectorButton, icon, selectedLabel, optionsRoot, optionTemplate);
        optionsRoot.gameObject.SetActive(false);
        EditorUtility.SetDirty(selector);
        return selector;
    }

    static Button EnsureSelectorOptionTemplate(RectTransform optionsRoot)
    {
        RectTransform optionRect = EnsureChildRect(optionsRoot, "OptionTemplate");
        optionRect.sizeDelta = new Vector2(0f, 30f);

        LayoutElement layout = optionRect.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = optionRect.gameObject.AddComponent<LayoutElement>();
        }

        layout.preferredHeight = 30f;

        Image image = optionRect.GetComponent<Image>();
        if (image == null)
        {
            image = optionRect.gameObject.AddComponent<Image>();
        }

        image.color = new Color(0.09f, 0.09f, 0.085f, 1f);

        Button button = optionRect.GetComponent<Button>();
        if (button == null)
        {
            button = optionRect.gameObject.AddComponent<Button>();
        }

        button.targetGraphic = image;

        Text label = EnsureChildText(optionRect, "Label", "Option", 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.92f, 0.88f, 0.78f, 1f));
        RectTransform labelRect = (RectTransform)label.transform;
        Stretch(labelRect);
        labelRect.offsetMin = new Vector2(12f, 0f);
        labelRect.offsetMax = new Vector2(-12f, 0f);
        optionRect.gameObject.SetActive(false);
        return button;
    }

    static void EnsureMarketColumnHeaders(RectTransform parent)
    {
        RectTransform header = EnsureChildRect(parent, "ColumnHeaders");
        header.anchorMin = new Vector2(0f, 1f);
        header.anchorMax = new Vector2(1f, 1f);
        header.pivot = new Vector2(0f, 1f);
        header.anchoredPosition = new Vector2(24f, -118f);
        header.sizeDelta = new Vector2(-48f, 24f);

        CreateHeaderLabel(header, "GoodsHeader", "Goods", 0f, 70f);
        CreateHeaderLabel(header, "StockHeader", "Stock", 64f, 94f);
        CreateHeaderLabel(header, "PriceHeader", "Price", 170f, 94f);
        CreateHeaderLabel(header, "AmountHeader", "< Sell     Buy >", 426f, 170f);
        CreateHeaderLabel(header, "CargoHeader", "Cargo", 740f, 96f);
        CreateHeaderLabel(header, "BoughtHeader", "Bought for", 862f, 112f);
    }

    static void CreateHeaderLabel(RectTransform parent, string name, string value, float x, float width)
    {
        Text label = EnsureChildText(parent, name, value, 11, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.42f, 0.42f, 0.44f, 1f));
        RectTransform rect = (RectTransform)label.transform;
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(x, 0f);
        rect.sizeDelta = new Vector2(width, 22f);
    }

    static MarketGoodsRow EnsureMarketRows(RectTransform parent, out RectTransform rowsRoot)
    {
        RectTransform viewport = EnsureChildRect(parent, "RowsViewport");
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = new Vector2(24f, 46f);
        viewport.offsetMax = new Vector2(-24f, -150f);

        Image viewportImage = viewport.GetComponent<Image>();
        if (viewportImage == null)
        {
            viewportImage = viewport.gameObject.AddComponent<Image>();
        }

        viewportImage.color = new Color(0f, 0f, 0f, 0f);
        viewportImage.raycastTarget = false;

        Mask mask = viewport.GetComponent<Mask>();
        if (mask == null)
        {
            mask = viewport.gameObject.AddComponent<Mask>();
        }

        mask.showMaskGraphic = false;

        rowsRoot = EnsureChildRect(viewport, "RowsContent");
        rowsRoot.anchorMin = new Vector2(0f, 1f);
        rowsRoot.anchorMax = new Vector2(1f, 1f);
        rowsRoot.pivot = new Vector2(0.5f, 1f);
        rowsRoot.anchoredPosition = Vector2.zero;
        rowsRoot.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup layout = rowsRoot.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = rowsRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = rowsRoot.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = rowsRoot.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        RectTransform templateRect = EnsureChildRect(rowsRoot, "RowTemplate");
        templateRect.sizeDelta = new Vector2(0f, 44f);
        LayoutElement templateLayout = templateRect.GetComponent<LayoutElement>();
        if (templateLayout == null)
        {
            templateLayout = templateRect.gameObject.AddComponent<LayoutElement>();
        }

        templateLayout.preferredHeight = 44f;

        MarketGoodsRow row = EnsureMarketRowTemplate(templateRect);
        templateRect.gameObject.SetActive(false);
        return row;
    }

    static MarketGoodsRow EnsureMarketRowTemplate(RectTransform rowRect)
    {
        Image rowBackground = rowRect.GetComponent<Image>();
        if (rowBackground == null)
        {
            rowBackground = rowRect.gameObject.AddComponent<Image>();
        }

        rowBackground.color = new Color(0.09f, 0.09f, 0.085f, 1f);

        Image iconBack = EnsureChildImage(rowRect, "GoodIconBackplate", new Color(0.16f, 0.16f, 0.16f, 1f));
        RectTransform iconBackRect = (RectTransform)iconBack.transform;
        iconBackRect.anchorMin = new Vector2(0f, 0.5f);
        iconBackRect.anchorMax = new Vector2(0f, 0.5f);
        iconBackRect.pivot = new Vector2(0f, 0.5f);
        iconBackRect.anchoredPosition = new Vector2(0f, 0f);
        iconBackRect.sizeDelta = new Vector2(44f, 44f);
        iconBack.raycastTarget = false;

        Image icon = EnsureChildImage(iconBackRect, "Icon", Color.white);
        RectTransform iconRect = (RectTransform)icon.transform;
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(28f, 28f);
        icon.raycastTarget = false;

        Text glyph = EnsureChildText(iconBackRect, "Glyph", "G", 16, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        Stretch((RectTransform)glyph.transform);

        Text stock = CreateRowValue(rowRect, "StockText", "--", 62f, 94f, TextAnchor.MiddleLeft);
        Text price = CreateRowValue(rowRect, "PriceText", "--", 172f, 94f, TextAnchor.MiddleLeft);
        Text cargo = CreateRowValue(rowRect, "CargoText", "--", 742f, 96f, TextAnchor.MiddleCenter);
        Text boughtFor = CreateRowValue(rowRect, "BoughtForText", "--", 862f, 112f, TextAnchor.MiddleLeft);

        RectTransform amountRoot = EnsureChildRect(rowRect, "AmountControl");
        amountRoot.anchorMin = new Vector2(0f, 0.5f);
        amountRoot.anchorMax = new Vector2(0f, 0.5f);
        amountRoot.pivot = new Vector2(0.5f, 0.5f);
        amountRoot.anchoredPosition = new Vector2(502f, 0f);
        amountRoot.sizeDelta = new Vector2(140f, 34f);

        Slider slider = amountRoot.GetComponent<Slider>();
        if (slider == null)
        {
            slider = amountRoot.gameObject.AddComponent<Slider>();
        }

        Image amountBackground = amountRoot.GetComponent<Image>();
        if (amountBackground == null)
        {
            amountBackground = amountRoot.gameObject.AddComponent<Image>();
        }

        amountBackground.color = new Color(0.96f, 0.96f, 0.94f, 1f);
        slider.minValue = -100f;
        slider.maxValue = 100f;
        slider.wholeNumbers = true;

        Image handle = EnsureChildImage(amountRoot, "SliderHandle", new Color(0.78f, 0.78f, 0.76f, 1f));
        RectTransform handleRect = (RectTransform)handle.transform;
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.anchoredPosition = Vector2.zero;
        handleRect.sizeDelta = new Vector2(12f, 26f);
        slider.handleRect = handleRect;
        slider.targetGraphic = handle;

        Button decrease = EnsureMiniButton(amountRoot, "DecreaseButton", "<", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 0f));
        Button increase = EnsureMiniButton(amountRoot, "IncreaseButton", ">", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f));
        Text amount = EnsureChildText(amountRoot, "AmountText", "0", 16, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.48f, 0.48f, 0.48f, 1f));
        Stretch((RectTransform)amount.transform);

        MarketGoodsRow row = rowRect.GetComponent<MarketGoodsRow>();
        if (row == null)
        {
            row = rowRect.gameObject.AddComponent<MarketGoodsRow>();
        }

        row.SetReferences(icon, glyph, stock, price, amount, cargo, boughtFor, decrease, increase, slider);
        EditorUtility.SetDirty(row);
        return row;
    }

    static Text CreateRowValue(RectTransform parent, string name, string value, float x, float width, TextAnchor anchor)
    {
        Text text = EnsureChildText(parent, name, value, 18, FontStyle.Bold, anchor, new Color(0.92f, 0.92f, 0.9f, 1f));
        RectTransform rect = (RectTransform)text.transform;
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(x, 0f);
        rect.sizeDelta = new Vector2(width, 36f);
        return text;
    }

    static Button EnsureMiniButton(RectTransform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition)
    {
        RectTransform rect = EnsureChildRect(parent, name);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(34f, 30f);

        Image image = rect.GetComponent<Image>();
        if (image == null)
        {
            image = rect.gameObject.AddComponent<Image>();
        }

        image.color = new Color(1f, 1f, 1f, 0f);

        Button button = rect.GetComponent<Button>();
        if (button == null)
        {
            button = rect.gameObject.AddComponent<Button>();
        }

        button.targetGraphic = image;
        Text text = EnsureChildText(rect, "Label", label, 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.06f, 0.06f, 0.06f, 1f));
        Stretch((RectTransform)text.transform);
        return button;
    }

    static void EnsureTradePanel(RectTransform sidePanel, out Text tradePlaceholder, out Text totalText, out Button confirmButton, out Button cancelButton)
    {
        Text title = EnsureChildText(sidePanel, "Title", "Trade", 24, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.98f, 0.82f, 0.55f, 1f));
        RectTransform titleRect = (RectTransform)title.transform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(24f, -18f);
        titleRect.sizeDelta = new Vector2(-48f, 34f);

        RectTransform headers = EnsureChildRect(sidePanel, "TradeHeaders");
        headers.anchorMin = new Vector2(0f, 1f);
        headers.anchorMax = new Vector2(1f, 1f);
        headers.pivot = new Vector2(0f, 1f);
        headers.anchoredPosition = new Vector2(24f, -58f);
        headers.sizeDelta = new Vector2(-48f, 30f);

        CreateHeaderLabel(headers, "Goods", "Goods", 0f, 56f);
        CreateHeaderLabel(headers, "Amount", "Amount", 58f, 58f);
        CreateHeaderLabel(headers, "Avg", "Avg/unit", 116f, 76f);
        CreateHeaderLabel(headers, "Price", "Price", 218f, 64f);

        tradePlaceholder = EnsureChildText(sidePanel, "TradePlaceholder", "Manage trades on the left\nto see details.", 15, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.36f, 0.36f, 0.38f, 1f));
        RectTransform placeholderRect = (RectTransform)tradePlaceholder.transform;
        placeholderRect.anchorMin = new Vector2(0f, 0f);
        placeholderRect.anchorMax = new Vector2(1f, 1f);
        placeholderRect.offsetMin = new Vector2(24f, 140f);
        placeholderRect.offsetMax = new Vector2(-24f, -112f);

        RectTransform footer = EnsureChildRect(sidePanel, "TradeFooter");
        footer.anchorMin = new Vector2(0f, 0f);
        footer.anchorMax = new Vector2(1f, 0f);
        footer.pivot = new Vector2(0.5f, 0f);
        footer.anchoredPosition = Vector2.zero;
        footer.sizeDelta = new Vector2(0f, 116f);

        Text totalLabel = EnsureChildText(footer, "TotalLabel", "Total", 16, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.42f, 0.42f, 0.44f, 1f));
        RectTransform totalLabelRect = (RectTransform)totalLabel.transform;
        totalLabelRect.anchorMin = new Vector2(0f, 1f);
        totalLabelRect.anchorMax = new Vector2(0f, 1f);
        totalLabelRect.pivot = new Vector2(0f, 1f);
        totalLabelRect.anchoredPosition = new Vector2(24f, -18f);
        totalLabelRect.sizeDelta = new Vector2(90f, 30f);

        totalText = EnsureChildText(footer, "TotalText", "--", 18, FontStyle.Bold, TextAnchor.MiddleRight, Color.white);
        RectTransform totalRect = (RectTransform)totalText.transform;
        totalRect.anchorMin = new Vector2(1f, 1f);
        totalRect.anchorMax = new Vector2(1f, 1f);
        totalRect.pivot = new Vector2(1f, 1f);
        totalRect.anchoredPosition = new Vector2(-24f, -18f);
        totalRect.sizeDelta = new Vector2(150f, 30f);

        confirmButton = EnsureActionButton(footer, "ConfirmDealButton", "Confirm deal", new Vector2(24f, 16f), new Vector2(-82f, 16f), new Color(0.92f, 0.92f, 0.9f, 1f), new Color(0.05f, 0.05f, 0.05f, 1f));
        cancelButton = EnsureActionButton(footer, "CancelDealButton", "x", new Vector2(0f, 16f), new Vector2(-24f, 16f), new Color(0.18f, 0.18f, 0.17f, 1f), new Color(0.9f, 0.84f, 0.72f, 1f));
        RectTransform cancelRect = (RectTransform)cancelButton.transform;
        cancelRect.anchorMin = new Vector2(1f, 0f);
        cancelRect.anchorMax = new Vector2(1f, 0f);
        cancelRect.pivot = new Vector2(1f, 0f);
        cancelRect.sizeDelta = new Vector2(50f, 50f);
    }

    static Button EnsureActionButton(RectTransform parent, string name, string label, Vector2 offsetMin, Vector2 offsetMax, Color background, Color textColor)
    {
        RectTransform rect = EnsureChildRect(parent, name);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 50f);

        Image image = rect.GetComponent<Image>();
        if (image == null)
        {
            image = rect.gameObject.AddComponent<Image>();
        }

        image.color = background;

        Button button = rect.GetComponent<Button>();
        if (button == null)
        {
            button = rect.gameObject.AddComponent<Button>();
        }

        button.targetGraphic = image;
        Text text = EnsureChildText(rect, "Label", label, 14, FontStyle.Bold, TextAnchor.MiddleCenter, textColor);
        Stretch((RectTransform)text.transform);
        return button;
    }

    static ShipyardPopupView EnsureShipyardPanel(RectTransform popup, ShipyardConfig shipyardConfig)
    {
        RectTransform panel = EnsureChildRect(popup, "ShipyardPanel");
        Stretch(panel);

        Image background = panel.GetComponent<Image>();
        if (background == null)
        {
            background = panel.gameObject.AddComponent<Image>();
        }

        background.color = new Color(0.035f, 0.034f, 0.03f, 0.96f);
        background.raycastTarget = true;

        ShipyardPopupView view = panel.GetComponent<ShipyardPopupView>();
        if (view == null)
        {
            view = panel.gameObject.AddComponent<ShipyardPopupView>();
        }

        EnsureShipyardHeader(panel);
        RectTransform tabsRoot = EnsureChildRect(panel, "Tabs");
        tabsRoot.anchorMin = new Vector2(0f, 1f);
        tabsRoot.anchorMax = new Vector2(1f, 1f);
        tabsRoot.pivot = new Vector2(0f, 1f);
        tabsRoot.anchoredPosition = new Vector2(24f, -58f);
        tabsRoot.sizeDelta = new Vector2(-48f, 32f);

        Button orderTab = EnsureShipyardTab(tabsRoot, "OrderTab", "Order a ship", 0f, 124f, out Image orderTabBackplate);
        Button queueTab = EnsureShipyardTab(tabsRoot, "QueueTab", "Construction queue", 132f, 158f, out Image queueTabBackplate);
        Button buyTab = EnsureShipyardTab(tabsRoot, "BuyTab", "Buy a ship", 298f, 116f, out Image buyTabBackplate);

        RectTransform content = EnsureChildRect(panel, "Content");
        content.anchorMin = new Vector2(0f, 0f);
        content.anchorMax = new Vector2(1f, 1f);
        content.offsetMin = new Vector2(24f, 22f);
        content.offsetMax = new Vector2(-24f, -96f);

        RectTransform orderPanel = EnsureShipyardTabPanel(content, "OrderPanel");
        RectTransform queuePanel = EnsureShipyardTabPanel(content, "ConstructionQueuePanel");
        RectTransform buyPanel = EnsureShipyardTabPanel(content, "BuyShipPanel");

        ShipyardOrderRefs orderRefs = EnsureShipyardOrderPanel(orderPanel);
        ShipyardQueueRefs queueRefs = EnsureShipyardQueuePanel(queuePanel);
        ShipyardBuyRefs buyRefs = EnsureShipyardBuyPanel(buyPanel);

        view.SetConfig(shipyardConfig);
        view.SetReferences(
            orderTab,
            queueTab,
            buyTab,
            orderTabBackplate,
            queueTabBackplate,
            buyTabBackplate,
            orderPanel,
            queuePanel,
            buyPanel,
            orderRefs.PreviousButton,
            orderRefs.NextButton,
            orderRefs.ShipImage,
            orderRefs.ImageFallbackText,
            orderRefs.ShipNameText,
            orderRefs.ClassTagText,
            orderRefs.SpeedText,
            orderRefs.HealthText,
            orderRefs.CrewText,
            orderRefs.StorageText,
            orderRefs.OperationalCostText,
            orderRefs.PriceText,
            orderRefs.EstimatedBuildTimeText,
            orderRefs.MaterialsRoot,
            orderRefs.MaterialTemplate,
            orderRefs.OrderButton,
            queueRefs.UnderConstructionRow,
            queueRefs.QueueRoot,
            queueRefs.QueueTemplate,
            queueRefs.EmptyText,
            buyRefs.PreviousButton,
            buyRefs.NextButton,
            buyRefs.IndexText,
            buyRefs.CardsRoot,
            buyRefs.CardTemplate,
            buyRefs.EmptyText);

        SerializedObject viewObject = new SerializedObject(view);
        viewObject.FindProperty("maxVisibleBuyCards").intValue = 4;
        viewObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(view);
        return view;
    }

    static void EnsureShipyardHeader(RectTransform panel)
    {
        RectTransform header = EnsureChildRect(panel, "ShipyardHeader");
        header.anchorMin = new Vector2(0f, 1f);
        header.anchorMax = new Vector2(1f, 1f);
        header.pivot = new Vector2(0.5f, 1f);
        header.anchoredPosition = Vector2.zero;
        header.sizeDelta = new Vector2(0f, 46f);

        Image headerImage = header.GetComponent<Image>();
        if (headerImage == null)
        {
            headerImage = header.gameObject.AddComponent<Image>();
        }

        headerImage.color = new Color(0.024f, 0.023f, 0.021f, 1f);

        Text title = EnsureChildText(header, "Title", "Shipyard", 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.98f, 0.82f, 0.55f, 1f));
        Stretch((RectTransform)title.transform);
    }

    static Button EnsureShipyardTab(RectTransform parent, string name, string label, float x, float width, out Image backplate)
    {
        RectTransform rect = EnsureChildRect(parent, name);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, 0f);
        rect.sizeDelta = new Vector2(width, 30f);

        backplate = rect.GetComponent<Image>();
        if (backplate == null)
        {
            backplate = rect.gameObject.AddComponent<Image>();
        }

        backplate.color = new Color(0.045f, 0.043f, 0.04f, 1f);

        Button button = rect.GetComponent<Button>();
        if (button == null)
        {
            button = rect.gameObject.AddComponent<Button>();
        }

        button.targetGraphic = backplate;
        Text text = EnsureChildText(rect, "Label", label, 12, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.82f, 0.78f, 0.68f, 1f));
        Stretch((RectTransform)text.transform);
        return button;
    }

    static RectTransform EnsureShipyardTabPanel(RectTransform parent, string name)
    {
        RectTransform panel = EnsureChildRect(parent, name);
        Stretch(panel);
        return panel;
    }

    static ShipyardOrderRefs EnsureShipyardOrderPanel(RectTransform panel)
    {
        RectTransform preview = EnsureChildRect(panel, "ShipPreview");
        preview.anchorMin = new Vector2(0f, 0f);
        preview.anchorMax = new Vector2(1f, 1f);
        preview.offsetMin = new Vector2(0f, 0f);
        preview.offsetMax = new Vector2(-350f, 0f);

        Image previewImage = preview.GetComponent<Image>();
        if (previewImage == null)
        {
            previewImage = preview.gameObject.AddComponent<Image>();
        }

        previewImage.color = new Color(0.12f, 0.13f, 0.12f, 1f);

        Image shipImage = EnsureChildImage(preview, "ShipImage", new Color(0.72f, 0.68f, 0.58f, 1f));
        Stretch((RectTransform)shipImage.transform);
        shipImage.enabled = false;
        shipImage.preserveAspect = true;

        Text fallback = EnsureChildText(preview, "ImageFallback", "SHIP IMAGE", 34, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.38f, 0.36f, 0.32f, 1f));
        Stretch((RectTransform)fallback.transform);

        Text classTag = EnsureChildText(preview, "ClassTag", "Trade", 12, FontStyle.Bold, TextAnchor.UpperRight, new Color(0.9f, 0.86f, 0.38f, 1f));
        RectTransform classRect = (RectTransform)classTag.transform;
        classRect.anchorMin = new Vector2(1f, 1f);
        classRect.anchorMax = new Vector2(1f, 1f);
        classRect.pivot = new Vector2(1f, 1f);
        classRect.anchoredPosition = new Vector2(-24f, -18f);
        classRect.sizeDelta = new Vector2(120f, 24f);

        Button previous = EnsureShipyardIconButton(preview, "PreviousShipButton", "<", new Vector2(0f, 0.5f), new Vector2(28f, 0f));
        Button next = EnsureShipyardIconButton(preview, "NextShipButton", ">", new Vector2(1f, 0.5f), new Vector2(-28f, 0f));

        RectTransform statsRoot = EnsureChildRect(preview, "ShipStats");
        statsRoot.anchorMin = new Vector2(0f, 0f);
        statsRoot.anchorMax = new Vector2(1f, 0f);
        statsRoot.pivot = new Vector2(0.5f, 0f);
        statsRoot.anchoredPosition = Vector2.zero;
        statsRoot.sizeDelta = new Vector2(0f, 112f);

        Image statsImage = statsRoot.GetComponent<Image>();
        if (statsImage == null)
        {
            statsImage = statsRoot.gameObject.AddComponent<Image>();
        }

        statsImage.color = new Color(0.02f, 0.02f, 0.018f, 0.82f);

        Text shipName = EnsureChildText(statsRoot, "ShipName", "Trade cog", 23, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.98f, 0.82f, 0.55f, 1f));
        RectTransform nameRect = (RectTransform)shipName.transform;
        nameRect.anchorMin = new Vector2(0f, 1f);
        nameRect.anchorMax = new Vector2(0.5f, 1f);
        nameRect.pivot = new Vector2(0f, 1f);
        nameRect.anchoredPosition = new Vector2(28f, -14f);
        nameRect.sizeDelta = new Vector2(260f, 30f);

        Text speed = EnsureShipyardStat(statsRoot, "SpeedStat", "Speed", 48f, 46f);
        Text health = EnsureShipyardStat(statsRoot, "HealthStat", "Health", 48f, 64f);
        Text crew = EnsureShipyardStat(statsRoot, "CrewStat", "Crew", 48f, 82f);
        Text storage = EnsureShipyardStat(statsRoot, "StorageStat", "Storage", 48f, 100f);
        Text operational = EnsureShipyardFooterStat(statsRoot, "OperationalCost", "Operational cost", 28f, 10f);

        RectTransform side = EnsureChildRect(panel, "RequiredMaterials");
        side.anchorMin = new Vector2(1f, 0f);
        side.anchorMax = new Vector2(1f, 1f);
        side.pivot = new Vector2(1f, 0.5f);
        side.anchoredPosition = Vector2.zero;
        side.sizeDelta = new Vector2(326f, 0f);

        Text materialsTitle = EnsureChildText(side, "Title", "Required materials", 19, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        RectTransform titleRect = (RectTransform)materialsTitle.transform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(18f, -14f);
        titleRect.sizeDelta = new Vector2(-36f, 30f);

        RectTransform materialsRoot = EnsureChildRect(side, "MaterialsRoot");
        materialsRoot.anchorMin = new Vector2(0f, 1f);
        materialsRoot.anchorMax = new Vector2(1f, 1f);
        materialsRoot.pivot = new Vector2(0.5f, 1f);
        materialsRoot.anchoredPosition = new Vector2(0f, -58f);
        materialsRoot.sizeDelta = new Vector2(-36f, 210f);
        VerticalLayoutGroup materialsLayout = EnsureVerticalLayout(materialsRoot, 8f, TextAnchor.UpperCenter);
        materialsLayout.childForceExpandWidth = true;

        ShipyardMaterialRow materialTemplate = EnsureShipyardMaterialTemplate(materialsRoot, "MaterialTemplate", true);

        Text etaLabel = EnsureChildText(side, "BuildTimeLabel", "Estimated build time", 12, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.42f, 0.42f, 0.44f, 1f));
        RectTransform etaLabelRect = (RectTransform)etaLabel.transform;
        etaLabelRect.anchorMin = new Vector2(0f, 0f);
        etaLabelRect.anchorMax = new Vector2(0f, 0f);
        etaLabelRect.pivot = new Vector2(0f, 0f);
        etaLabelRect.anchoredPosition = new Vector2(18f, 82f);
        etaLabelRect.sizeDelta = new Vector2(130f, 24f);

        Text eta = EnsureChildText(side, "EstimatedBuildTimeText", "--", 12, FontStyle.Bold, TextAnchor.MiddleRight, new Color(0.68f, 0.66f, 0.62f, 1f));
        RectTransform etaRect = (RectTransform)eta.transform;
        etaRect.anchorMin = new Vector2(1f, 0f);
        etaRect.anchorMax = new Vector2(1f, 0f);
        etaRect.pivot = new Vector2(1f, 0f);
        etaRect.anchoredPosition = new Vector2(-18f, 82f);
        etaRect.sizeDelta = new Vector2(140f, 24f);

        Text price = EnsureChildText(side, "PriceText", "--", 15, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        RectTransform priceRect = (RectTransform)price.transform;
        priceRect.anchorMin = new Vector2(0f, 0f);
        priceRect.anchorMax = new Vector2(1f, 0f);
        priceRect.pivot = new Vector2(0.5f, 0f);
        priceRect.anchoredPosition = new Vector2(0f, 44f);
        priceRect.sizeDelta = new Vector2(-36f, 28f);

        Button orderButton = EnsureActionButton(side, "OrderButton", "Order", new Vector2(18f, 6f), new Vector2(-18f, 6f), new Color(0.96f, 0.96f, 0.94f, 1f), new Color(0.05f, 0.05f, 0.05f, 1f));
        return new ShipyardOrderRefs(previous, next, shipImage, fallback, shipName, classTag, speed, health, crew, storage, operational, price, eta, materialsRoot, materialTemplate, orderButton);
    }

    static Text EnsureShipyardStat(RectTransform parent, string name, string label, float x, float y)
    {
        Text text = EnsureChildText(parent, name, $"{label}      --", 11, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.76f, 0.74f, 0.68f, 1f));
        RectTransform rect = (RectTransform)text.transform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(220f, 18f);
        return text;
    }

    static Text EnsureShipyardFooterStat(RectTransform parent, string name, string label, float x, float y)
    {
        Text text = EnsureChildText(parent, name, $"{label}     --", 12, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.76f, 0.74f, 0.68f, 1f));
        RectTransform rect = (RectTransform)text.transform;
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(260f, 22f);
        return text;
    }

    static ShipyardQueueRefs EnsureShipyardQueuePanel(RectTransform panel)
    {
        Text currentLabel = EnsureChildText(panel, "UnderConstructionLabel", "Under construction", 14, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.82f, 0.8f, 0.74f, 1f));
        RectTransform currentLabelRect = (RectTransform)currentLabel.transform;
        currentLabelRect.anchorMin = new Vector2(0f, 1f);
        currentLabelRect.anchorMax = new Vector2(1f, 1f);
        currentLabelRect.pivot = new Vector2(0f, 1f);
        currentLabelRect.anchoredPosition = new Vector2(22f, -14f);
        currentLabelRect.sizeDelta = new Vector2(-44f, 24f);

        RectTransform currentRowRect = EnsureChildRect(panel, "UnderConstructionRow");
        currentRowRect.anchorMin = new Vector2(0f, 1f);
        currentRowRect.anchorMax = new Vector2(1f, 1f);
        currentRowRect.pivot = new Vector2(0.5f, 1f);
        currentRowRect.anchoredPosition = new Vector2(0f, -48f);
        currentRowRect.sizeDelta = new Vector2(-44f, 96f);
        ShipyardConstructionRow currentRow = EnsureShipyardConstructionRowTemplate(currentRowRect, true);

        Text queueLabel = EnsureChildText(panel, "QueueLabel", "Queue", 12, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.42f, 0.42f, 0.44f, 1f));
        RectTransform queueLabelRect = (RectTransform)queueLabel.transform;
        queueLabelRect.anchorMin = new Vector2(0f, 1f);
        queueLabelRect.anchorMax = new Vector2(1f, 1f);
        queueLabelRect.pivot = new Vector2(0f, 1f);
        queueLabelRect.anchoredPosition = new Vector2(22f, -160f);
        queueLabelRect.sizeDelta = new Vector2(-44f, 22f);

        DestroyChildIfExists(panel, "ConstructionRowsRoot");

        RectTransform scrollRoot = EnsureChildRect(panel, "ConstructionRowsScroll");
        scrollRoot.anchorMin = new Vector2(0f, 0f);
        scrollRoot.anchorMax = new Vector2(1f, 1f);
        scrollRoot.offsetMin = new Vector2(22f, 0f);
        scrollRoot.offsetMax = new Vector2(-22f, -188f);

        Image scrollBackground = scrollRoot.GetComponent<Image>();
        if (scrollBackground == null)
        {
            scrollBackground = scrollRoot.gameObject.AddComponent<Image>();
        }

        scrollBackground.color = new Color(0.045f, 0.045f, 0.04f, 0.45f);

        ScrollRect scroll = scrollRoot.GetComponent<ScrollRect>();
        if (scroll == null)
        {
            scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
        }

        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 24f;

        RectTransform viewport = EnsureChildRect(scrollRoot, "Viewport");
        Stretch(viewport);
        viewport.offsetMax = new Vector2(-12f, 0f);

        Image viewportImage = viewport.GetComponent<Image>();
        if (viewportImage == null)
        {
            viewportImage = viewport.gameObject.AddComponent<Image>();
        }

        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.GetComponent<Mask>();
        if (mask == null)
        {
            mask = viewport.gameObject.AddComponent<Mask>();
        }

        mask.showMaskGraphic = false;

        RectTransform queueRoot = EnsureChildRect(viewport, "ConstructionRowsRoot");
        queueRoot.anchorMin = new Vector2(0f, 1f);
        queueRoot.anchorMax = new Vector2(1f, 1f);
        queueRoot.pivot = new Vector2(0.5f, 1f);
        queueRoot.anchoredPosition = Vector2.zero;
        queueRoot.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = EnsureVerticalLayout(queueRoot, 8f, TextAnchor.UpperCenter);
        layout.childForceExpandWidth = true;

        ContentSizeFitter fitter = queueRoot.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = queueRoot.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport;
        scroll.content = queueRoot;

        RectTransform scrollbarRect = EnsureChildRect(scrollRoot, "Scrollbar");
        scrollbarRect.anchorMin = new Vector2(1f, 0f);
        scrollbarRect.anchorMax = new Vector2(1f, 1f);
        scrollbarRect.pivot = new Vector2(1f, 0.5f);
        scrollbarRect.anchoredPosition = Vector2.zero;
        scrollbarRect.sizeDelta = new Vector2(8f, 0f);

        Image scrollbarBack = scrollbarRect.GetComponent<Image>();
        if (scrollbarBack == null)
        {
            scrollbarBack = scrollbarRect.gameObject.AddComponent<Image>();
        }

        scrollbarBack.color = new Color(0.12f, 0.12f, 0.11f, 0.85f);

        Scrollbar scrollbar = scrollbarRect.GetComponent<Scrollbar>();
        if (scrollbar == null)
        {
            scrollbar = scrollbarRect.gameObject.AddComponent<Scrollbar>();
        }

        RectTransform slidingArea = EnsureChildRect(scrollbarRect, "SlidingArea");
        Stretch(slidingArea);
        slidingArea.offsetMin = new Vector2(0f, 4f);
        slidingArea.offsetMax = new Vector2(0f, -4f);

        RectTransform handle = EnsureChildRect(slidingArea, "Handle");
        Stretch(handle);
        Image handleImage = handle.GetComponent<Image>();
        if (handleImage == null)
        {
            handleImage = handle.gameObject.AddComponent<Image>();
        }

        handleImage.color = new Color(0.82f, 0.78f, 0.68f, 0.9f);
        scrollbar.targetGraphic = handleImage;
        scrollbar.handleRect = handle;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scroll.verticalScrollbar = scrollbar;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

        RectTransform templateRect = EnsureChildRect(queueRoot, "ConstructionRowTemplate");
        templateRect.sizeDelta = new Vector2(0f, 46f);
        LayoutElement layoutElement = templateRect.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = templateRect.gameObject.AddComponent<LayoutElement>();
        }

        layoutElement.preferredHeight = 46f;
        ShipyardConstructionRow template = EnsureShipyardConstructionRowTemplate(templateRect, false);
        template.gameObject.SetActive(false);

        Text empty = EnsureChildText(panel, "ConstructionEmptyText", "No ships are currently under construction.", 15, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.36f, 0.36f, 0.38f, 1f));
        Stretch((RectTransform)empty.transform);
        return new ShipyardQueueRefs(currentRow, queueRoot, template, empty);
    }

    static ShipyardConstructionRow EnsureShipyardConstructionRowTemplate(RectTransform rowRect, bool detailed)
    {
        Image background = rowRect.GetComponent<Image>();
        if (background == null)
        {
            background = rowRect.gameObject.AddComponent<Image>();
        }

        background.color = detailed ? new Color(0.09f, 0.09f, 0.085f, 1f) : new Color(0.105f, 0.105f, 0.095f, 1f);

        Text shipName = EnsureChildText(rowRect, "ShipName", "Trade cog", detailed ? 15 : 14, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.86f, 0.84f, 0.78f, 1f));
        RectTransform shipNameRect = (RectTransform)shipName.transform;
        shipNameRect.anchorMin = new Vector2(0f, 1f);
        shipNameRect.anchorMax = new Vector2(0f, 1f);
        shipNameRect.pivot = new Vector2(0f, 1f);
        shipNameRect.anchoredPosition = new Vector2(18f, detailed ? -18f : -10f);
        shipNameRect.sizeDelta = new Vector2(190f, 26f);

        Text owner = EnsureChildText(rowRect, "OwnerText", "Owner --", 10, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.62f, 0.6f, 0.56f, 1f));
        RectTransform ownerRect = (RectTransform)owner.transform;
        ownerRect.anchorMin = new Vector2(0f, 1f);
        ownerRect.anchorMax = new Vector2(0f, 1f);
        ownerRect.pivot = new Vector2(0f, 1f);
        ownerRect.anchoredPosition = new Vector2(180f, detailed ? -18f : -10f);
        ownerRect.sizeDelta = new Vector2(180f, 24f);

        Text eta = EnsureChildText(rowRect, "EstimatedBuildTimeText", "--", 11, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.78f, 0.76f, 0.7f, 1f));
        RectTransform etaRect = (RectTransform)eta.transform;
        etaRect.anchorMin = new Vector2(1f, 1f);
        etaRect.anchorMax = new Vector2(1f, 1f);
        etaRect.pivot = new Vector2(1f, 1f);
        etaRect.anchoredPosition = new Vector2(-184f, detailed ? -18f : -10f);
        etaRect.sizeDelta = new Vector2(220f, 24f);

        RectTransform progressBack = EnsureChildRect(rowRect, "ProgressBackplate");
        progressBack.anchorMin = new Vector2(1f, 1f);
        progressBack.anchorMax = new Vector2(1f, 1f);
        progressBack.pivot = new Vector2(1f, 1f);
        progressBack.anchoredPosition = new Vector2(-18f, detailed ? -20f : -14f);
        progressBack.sizeDelta = new Vector2(108f, 8f);
        Image progressBackImage = progressBack.GetComponent<Image>();
        if (progressBackImage == null)
        {
            progressBackImage = progressBack.gameObject.AddComponent<Image>();
        }

        progressBackImage.color = new Color(0.32f, 0.32f, 0.32f, 1f);

        Image progressFill = EnsureChildImage(progressBack, "ProgressFill", new Color(0.92f, 0.92f, 0.9f, 1f));
        RectTransform progressFillRect = (RectTransform)progressFill.transform;
        progressFillRect.anchorMin = Vector2.zero;
        progressFillRect.anchorMax = new Vector2(0.5f, 1f);
        progressFillRect.offsetMin = Vector2.zero;
        progressFillRect.offsetMax = Vector2.zero;

        RectTransform materialsRoot = EnsureChildRect(rowRect, "MaterialStatusRoot");
        materialsRoot.anchorMin = new Vector2(0f, 0f);
        materialsRoot.anchorMax = new Vector2(0f, 0f);
        materialsRoot.pivot = new Vector2(0f, 0f);
        materialsRoot.anchoredPosition = new Vector2(16f, 10f);
        materialsRoot.sizeDelta = new Vector2(260f, 34f);
        HorizontalLayoutGroup materialLayout = materialsRoot.GetComponent<HorizontalLayoutGroup>();
        if (materialLayout == null)
        {
            materialLayout = materialsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        materialLayout.spacing = 12f;
        materialLayout.childControlWidth = false;
        materialLayout.childControlHeight = false;
        materialLayout.childForceExpandWidth = false;
        materialLayout.childForceExpandHeight = false;
        materialsRoot.gameObject.SetActive(detailed);

        ShipyardMaterialRow materialTemplate = EnsureShipyardMaterialTemplate(materialsRoot, "MaterialStatusTemplate", false);
        materialTemplate.gameObject.SetActive(false);

        Text status = EnsureChildText(rowRect, "StatusText", string.Empty, 10, FontStyle.Bold, TextAnchor.MiddleRight, new Color(0.92f, 0.84f, 0.18f, 1f));
        RectTransform statusRect = (RectTransform)status.transform;
        statusRect.anchorMin = new Vector2(0.5f, 0f);
        statusRect.anchorMax = new Vector2(1f, 0f);
        statusRect.pivot = new Vector2(1f, 0f);
        statusRect.anchoredPosition = new Vector2(-166f, 10f);
        statusRect.sizeDelta = new Vector2(350f, 32f);

        Button cancel = EnsureShipyardSmallButton(rowRect, "CancelButton", detailed ? "Cancel order" : string.Empty, new Vector2(1f, 0f), new Vector2(-18f, 12f), new Vector2(126f, 28f));
        cancel.gameObject.SetActive(detailed);

        ShipyardConstructionRow row = rowRect.GetComponent<ShipyardConstructionRow>();
        if (row == null)
        {
            row = rowRect.gameObject.AddComponent<ShipyardConstructionRow>();
        }

        row.SetReferences(shipName, owner, eta, status, progressFill, materialsRoot, materialTemplate, cancel);
        EditorUtility.SetDirty(row);
        return row;
    }

    static ShipyardBuyRefs EnsureShipyardBuyPanel(RectTransform panel)
    {
        Button previous = EnsureShipyardIconButton(panel, "PreviousBuyButton", "<", new Vector2(0f, 0.5f), new Vector2(54f, 0f));
        Button next = EnsureShipyardIconButton(panel, "NextBuyButton", ">", new Vector2(1f, 0.5f), new Vector2(-54f, 0f));

        RectTransform cardsRoot = EnsureChildRect(panel, "BuyCardsRoot");
        cardsRoot.anchorMin = new Vector2(0f, 0f);
        cardsRoot.anchorMax = new Vector2(1f, 1f);
        cardsRoot.pivot = new Vector2(0.5f, 0.5f);
        cardsRoot.offsetMin = new Vector2(92f, 34f);
        cardsRoot.offsetMax = new Vector2(-92f, -8f);

        HorizontalLayoutGroup layout = cardsRoot.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = cardsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = 24f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.MiddleCenter;

        RectTransform templateRect = EnsureChildRect(cardsRoot, "BuyCardTemplate");
        templateRect.sizeDelta = new Vector2(164f, 346f);
        LayoutElement layoutElement = templateRect.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = templateRect.gameObject.AddComponent<LayoutElement>();
        }

        layoutElement.preferredWidth = 164f;
        layoutElement.preferredHeight = 346f;
        ShipyardBuyCard template = EnsureShipyardBuyCardTemplate(templateRect);
        template.gameObject.SetActive(false);

        Text index = EnsureChildText(panel, "BuyCarouselIndexText", "1 / 1", 12, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.66f, 0.64f, 0.6f, 1f));
        RectTransform indexRect = (RectTransform)index.transform;
        indexRect.anchorMin = new Vector2(0.5f, 0f);
        indexRect.anchorMax = new Vector2(0.5f, 0f);
        indexRect.pivot = new Vector2(0.5f, 0f);
        indexRect.anchoredPosition = new Vector2(0f, 8f);
        indexRect.sizeDelta = new Vector2(120f, 24f);

        Text empty = EnsureChildText(panel, "BuyEmptyText", "No ships are currently available for purchase.", 15, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.36f, 0.36f, 0.38f, 1f));
        Stretch((RectTransform)empty.transform);
        return new ShipyardBuyRefs(previous, next, index, cardsRoot, template, empty);
    }

    static ShipyardBuyCard EnsureShipyardBuyCardTemplate(RectTransform cardRect)
    {
        Image background = cardRect.GetComponent<Image>();
        if (background == null)
        {
            background = cardRect.gameObject.AddComponent<Image>();
        }

        background.color = new Color(0.105f, 0.105f, 0.095f, 1f);

        Image shipImage = EnsureChildImage(cardRect, "ShipImage", new Color(0.22f, 0.22f, 0.2f, 1f));
        RectTransform imageRect = (RectTransform)shipImage.transform;
        imageRect.anchorMin = new Vector2(0f, 1f);
        imageRect.anchorMax = new Vector2(1f, 1f);
        imageRect.pivot = new Vector2(0.5f, 1f);
        imageRect.anchoredPosition = Vector2.zero;
        imageRect.sizeDelta = new Vector2(0f, 132f);
        shipImage.enabled = false;
        shipImage.preserveAspect = true;

        Text fallback = EnsureChildText(cardRect, "ImageFallback", "SHIP", 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.38f, 0.36f, 0.32f, 1f));
        RectTransform fallbackRect = (RectTransform)fallback.transform;
        fallbackRect.anchorMin = new Vector2(0f, 1f);
        fallbackRect.anchorMax = new Vector2(1f, 1f);
        fallbackRect.pivot = new Vector2(0.5f, 1f);
        fallbackRect.anchoredPosition = Vector2.zero;
        fallbackRect.sizeDelta = new Vector2(0f, 132f);

        Text classTag = EnsureChildText(cardRect, "ClassTag", "Trade", 10, FontStyle.Bold, TextAnchor.MiddleRight, new Color(0.9f, 0.86f, 0.38f, 1f));
        RectTransform classRect = (RectTransform)classTag.transform;
        classRect.anchorMin = new Vector2(1f, 1f);
        classRect.anchorMax = new Vector2(1f, 1f);
        classRect.pivot = new Vector2(1f, 1f);
        classRect.anchoredPosition = new Vector2(-10f, -8f);
        classRect.sizeDelta = new Vector2(72f, 20f);

        Text name = EnsureChildText(cardRect, "ShipName", "Trade cog", 20, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.98f, 0.82f, 0.55f, 1f));
        RectTransform nameRect = (RectTransform)name.transform;
        nameRect.anchorMin = new Vector2(0f, 1f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.pivot = new Vector2(0f, 1f);
        nameRect.anchoredPosition = new Vector2(12f, -138f);
        nameRect.sizeDelta = new Vector2(-24f, 28f);

        Text speed = EnsureShipyardCardStat(cardRect, "SpeedText", "Speed", -172f);
        Text health = EnsureShipyardCardStat(cardRect, "HealthText", "Health", -190f);
        Text crew = EnsureShipyardCardStat(cardRect, "CrewText", "Crew", -208f);
        Text storage = EnsureShipyardCardStat(cardRect, "StorageText", "Storage", -226f);
        Text operational = EnsureShipyardCardStat(cardRect, "OperationalCostText", "Operational cost", -268f);

        Button buy = EnsureActionButton(cardRect, "BuyButton", "Buy", new Vector2(12f, 26f), new Vector2(-12f, 26f), new Color(0.96f, 0.96f, 0.94f, 1f), new Color(0.05f, 0.05f, 0.05f, 1f));
        Transform buyLabelTransform = buy.transform.Find("Label");
        if (buyLabelTransform != null)
        {
            buyLabelTransform.gameObject.SetActive(false);
        }

        Text price = EnsureChildText((RectTransform)buy.transform, "PriceText", "--", 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.05f, 0.05f, 0.05f, 1f));
        Stretch((RectTransform)price.transform);

        Text owned = EnsureChildText(cardRect, "OwnedCountText", "You have: --", 10, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.66f, 0.64f, 0.6f, 1f));
        RectTransform ownedRect = (RectTransform)owned.transform;
        ownedRect.anchorMin = new Vector2(0f, 0f);
        ownedRect.anchorMax = new Vector2(1f, 0f);
        ownedRect.pivot = new Vector2(0.5f, 0f);
        ownedRect.anchoredPosition = new Vector2(0f, 0f);
        ownedRect.sizeDelta = new Vector2(0f, 20f);

        ShipyardBuyCard card = cardRect.GetComponent<ShipyardBuyCard>();
        if (card == null)
        {
            card = cardRect.gameObject.AddComponent<ShipyardBuyCard>();
        }

        card.SetReferences(shipImage, fallback, name, classTag, speed, health, crew, storage, operational, price, owned, buy);
        EditorUtility.SetDirty(card);
        return card;
    }

    static Text EnsureShipyardCardStat(RectTransform parent, string name, string label, float y)
    {
        Text text = EnsureChildText(parent, name, $"{label}      --", 10, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.78f, 0.76f, 0.7f, 1f));
        RectTransform rect = (RectTransform)text.transform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(12f, y);
        rect.sizeDelta = new Vector2(-24f, 18f);
        return text;
    }

    static Button EnsureShipyardIconButton(RectTransform parent, string name, string label, Vector2 anchor, Vector2 anchoredPosition)
    {
        return EnsureShipyardSmallButton(parent, name, label, anchor, anchoredPosition, new Vector2(42f, 42f));
    }

    static Button EnsureShipyardSmallButton(RectTransform parent, string name, string label, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
    {
        RectTransform rect = EnsureChildRect(parent, name);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = rect.GetComponent<Image>();
        if (image == null)
        {
            image = rect.gameObject.AddComponent<Image>();
        }

        image.color = new Color(1f, 1f, 1f, string.IsNullOrEmpty(label) ? 0f : 0.04f);

        Button button = rect.GetComponent<Button>();
        if (button == null)
        {
            button = rect.gameObject.AddComponent<Button>();
        }

        button.targetGraphic = image;
        Text text = EnsureChildText(rect, "Label", label, 20, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        Stretch((RectTransform)text.transform);
        return button;
    }

    static ShipyardMaterialRow EnsureShipyardMaterialTemplate(RectTransform parent, string name, bool showLabel)
    {
        RectTransform rect = EnsureChildRect(parent, name);
        rect.sizeDelta = showLabel ? new Vector2(0f, 34f) : new Vector2(82f, 28f);

        LayoutElement layout = rect.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = rect.gameObject.AddComponent<LayoutElement>();
        }

        layout.preferredHeight = showLabel ? 34f : 28f;
        layout.preferredWidth = showLabel ? -1f : 82f;

        Image iconBack = EnsureChildImage(rect, "IconBackplate", new Color(0.18f, 0.18f, 0.17f, 1f));
        RectTransform iconBackRect = (RectTransform)iconBack.transform;
        iconBackRect.anchorMin = new Vector2(0f, 0.5f);
        iconBackRect.anchorMax = new Vector2(0f, 0.5f);
        iconBackRect.pivot = new Vector2(0f, 0.5f);
        iconBackRect.anchoredPosition = new Vector2(0f, 0f);
        iconBackRect.sizeDelta = new Vector2(showLabel ? 34f : 28f, showLabel ? 34f : 28f);
        iconBack.raycastTarget = false;

        Image icon = EnsureChildImage(iconBackRect, "Icon", Color.white);
        RectTransform iconRect = (RectTransform)icon.transform;
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(showLabel ? 22f : 18f, showLabel ? 22f : 18f);
        icon.raycastTarget = false;
        icon.enabled = false;

        Text glyph = EnsureChildText(iconBackRect, "Glyph", "MA", showLabel ? 11 : 9, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        Stretch((RectTransform)glyph.transform);

        Text label = EnsureChildText(rect, "Label", "Material name", showLabel ? 13 : 10, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.92f, 0.92f, 0.9f, 1f));
        RectTransform labelRect = (RectTransform)label.transform;
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(showLabel ? 44f : 32f, 0f);
        labelRect.offsetMax = new Vector2(showLabel ? -66f : -4f, 0f);

        Text amount = EnsureChildText(rect, "Amount", "--", showLabel ? 13 : 10, FontStyle.Bold, TextAnchor.MiddleRight, new Color(0.92f, 0.92f, 0.9f, 1f));
        RectTransform amountRect = (RectTransform)amount.transform;
        amountRect.anchorMin = new Vector2(1f, 0f);
        amountRect.anchorMax = new Vector2(1f, 1f);
        amountRect.pivot = new Vector2(1f, 0.5f);
        amountRect.anchoredPosition = Vector2.zero;
        amountRect.sizeDelta = new Vector2(showLabel ? 62f : 50f, 0f);

        ShipyardMaterialRow row = rect.GetComponent<ShipyardMaterialRow>();
        if (row == null)
        {
            row = rect.gameObject.AddComponent<ShipyardMaterialRow>();
        }

        row.SetReferences(icon, glyph, label, amount);
        rect.gameObject.SetActive(false);
        EditorUtility.SetDirty(row);
        return row;
    }

    static VerticalLayoutGroup EnsureVerticalLayout(RectTransform root, float spacing, TextAnchor alignment)
    {
        VerticalLayoutGroup layout = root.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        layout.spacing = spacing;
        layout.childAlignment = alignment;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return layout;
    }

    static void DestroyChildIfExists(RectTransform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }
    }

    static Button EnsureCloseButton(RectTransform panel)
    {
        Transform existing = panel.Find("CloseButton");
        RectTransform closeRect;
        Button button;

        if (existing == null)
        {
            GameObject closeObject = new GameObject("CloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            closeRect = closeObject.GetComponent<RectTransform>();
            closeRect.SetParent(panel, false);
            button = closeObject.GetComponent<Button>();
        }
        else
        {
            closeRect = (RectTransform)existing;
            button = closeRect.GetComponent<Button>();
            if (button == null)
            {
                button = closeRect.gameObject.AddComponent<Button>();
            }
        }

        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-16f, -16f);
        closeRect.sizeDelta = new Vector2(36f, 36f);

        Image image = closeRect.GetComponent<Image>();
        image.color = new Color(0.02f, 0.02f, 0.018f, 0.92f);

        Text label = EnsureChildText(closeRect, "Label", "X", 17, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.92f, 0.84f, 0.7f, 1f));
        Stretch((RectTransform)label.transform);

        return button;
    }

    static void EnsureEventSystem(Scene scene)
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        inputModule.AssignDefaultActions();
        SceneManager.MoveGameObjectToScene(eventSystem, scene);
    }

    static void EnableCameraPostProcessing(Camera camera)
    {
        UniversalAdditionalCameraData cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null)
        {
            return;
        }

        cameraData.renderPostProcessing = true;
        cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        cameraData.antialiasingQuality = AntialiasingQuality.High;
        EditorUtility.SetDirty(cameraData);
    }

    static void FixKnownTextureImports()
    {
        TextureImporter bankNormal = AssetImporter.GetAtPath("Assets/Materials/Textures/Bank_normal.JPEG") as TextureImporter;
        if (bankNormal == null)
        {
            return;
        }

        bool changed = false;
        if (bankNormal.textureType != TextureImporterType.NormalMap)
        {
            bankNormal.textureType = TextureImporterType.NormalMap;
            changed = true;
        }

        if (bankNormal.sRGBTexture)
        {
            bankNormal.sRGBTexture = false;
            changed = true;
        }

        if (changed)
        {
            bankNormal.SaveAndReimport();
        }
    }

    static CityBuildingDefinition EnsureDefinition(BuildingPreset preset)
    {
        string assetPath = $"{DefinitionFolder}/{preset.Id}.asset";
        CityBuildingDefinition definition = AssetDatabase.LoadAssetAtPath<CityBuildingDefinition>(assetPath);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<CityBuildingDefinition>();
            AssetDatabase.CreateAsset(definition, assetPath);
        }

        definition.Configure(
            preset.Id,
            preset.DisplayName,
            preset.Kind,
            preset.Glyph,
            preset.Accent,
            preset.PanelTitle,
            preset.PanelSummary);
        EditorUtility.SetDirty(definition);
        return definition;
    }

    static Bounds CalculateCityBounds(IReadOnlyList<CityBuilding> buildings)
    {
        if (buildings == null || buildings.Count == 0)
        {
            return new Bounds(new Vector3(-70f, 18f, 150f), new Vector3(220f, 90f, 260f));
        }

        Bounds bounds = buildings[0].GetWorldBounds();
        for (int i = 1; i < buildings.Count; i++)
        {
            bounds.Encapsulate(buildings[i].GetWorldBounds());
        }

        return bounds;
    }

    static Bounds CalculateBounds(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(root.position, Vector3.one * 8f);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    static Transform GetOrCreateRoot(Scene scene, string name)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null)
        {
            return existing.transform;
        }

        GameObject root = new GameObject(name, typeof(RectTransform));
        SceneManager.MoveGameObjectToScene(root, scene);
        return root.transform;
    }

    static Transform GetOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child;
        }

        GameObject childObject = new GameObject(name, typeof(RectTransform));
        child = childObject.transform;
        child.SetParent(parent, false);
        return child;
    }

    static Transform GetOrCreatePlainChild(Transform parent, string name, out bool created)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            created = false;
            return child;
        }

        GameObject childObject = new GameObject(name);
        child = childObject.transform;
        child.SetParent(parent, true);
        created = true;
        return child;
    }

    static RectTransform EnsureChildRect(RectTransform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return (RectTransform)existing;
        }

        GameObject childObject = new GameObject(name, typeof(RectTransform));
        RectTransform rect = childObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    static Image EnsureChildImage(RectTransform parent, string name, Color color)
    {
        Transform existing = parent.Find(name);
        if (existing != null && existing.TryGetComponent(out Image image))
        {
            image.color = color;
            return image;
        }

        return CreateUiImage(parent, name, color);
    }

    static Text EnsureChildText(RectTransform parent, string name, string value, int size, FontStyle style, TextAnchor alignment, Color color)
    {
        Transform existing = parent.Find(name);
        if (existing != null && existing.TryGetComponent(out Text text))
        {
            ConfigureText(text, value, size, style, alignment, color);
            return text;
        }

        return CreateUiText(parent, name, value, size, style, alignment, color);
    }

    static Image CreateUiImage(RectTransform parent, string name, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    static Text CreateUiText(RectTransform parent, string name, string value, int size, FontStyle style, TextAnchor alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        ConfigureText(text, value, size, style, alignment, color);
        return text;
    }

    static void ConfigureText(Text text, string value, int size, FontStyle style, TextAnchor alignment, Color color)
    {
        text.text = value;
        text.font = GetDefaultFont();
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    static Font GetDefaultFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }

    static void EnsureFolder(string folder)
    {
        string[] parts = folder.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    readonly struct BuildingPreset
    {
        public readonly string Id;
        public readonly string SceneObjectName;
        public readonly string DisplayName;
        public readonly CityBuildingKind Kind;
        public readonly string Glyph;
        public readonly Color Accent;
        public readonly string PanelTitle;
        public readonly string PanelSummary;
        public readonly float CameraYaw;
        public readonly float CameraPitch;
        public readonly float CameraDistanceMultiplier;
        public readonly float CameraFov;

        public BuildingPreset(
            string id,
            string displayName,
            CityBuildingKind kind,
            string glyph,
            Color accent,
            string panelTitle,
            string panelSummary,
            float cameraYaw = 0f,
            float cameraPitch = 24f,
            float cameraDistanceMultiplier = 1.25f,
            float cameraFov = 34f)
        {
            Id = id;
            SceneObjectName = displayName.Replace(" ", string.Empty);
            DisplayName = displayName;
            Kind = kind;
            Glyph = glyph;
            Accent = accent;
            PanelTitle = panelTitle;
            PanelSummary = panelSummary;
            CameraYaw = cameraYaw;
            CameraPitch = cameraPitch;
            CameraDistanceMultiplier = cameraDistanceMultiplier;
            CameraFov = cameraFov;
        }
    }

    readonly struct FooterPreset
    {
        public readonly string EntryId;
        public readonly string Label;
        public readonly string TargetBuildingId;
        public readonly bool IsAvailable;
        public readonly string Glyph;

        public string ObjectName => $"FooterButton_{EntryId}";

        public FooterPreset(string entryId, string label, string targetBuildingId, bool isAvailable, string glyph)
        {
            EntryId = entryId;
            Label = label;
            TargetBuildingId = targetBuildingId;
            IsAvailable = isAvailable;
            Glyph = glyph;
        }
    }

    readonly struct ShipyardOrderRefs
    {
        public readonly Button PreviousButton;
        public readonly Button NextButton;
        public readonly Image ShipImage;
        public readonly Text ImageFallbackText;
        public readonly Text ShipNameText;
        public readonly Text ClassTagText;
        public readonly Text SpeedText;
        public readonly Text HealthText;
        public readonly Text CrewText;
        public readonly Text StorageText;
        public readonly Text OperationalCostText;
        public readonly Text PriceText;
        public readonly Text EstimatedBuildTimeText;
        public readonly RectTransform MaterialsRoot;
        public readonly ShipyardMaterialRow MaterialTemplate;
        public readonly Button OrderButton;

        public ShipyardOrderRefs(
            Button previousButton,
            Button nextButton,
            Image shipImage,
            Text imageFallbackText,
            Text shipNameText,
            Text classTagText,
            Text speedText,
            Text healthText,
            Text crewText,
            Text storageText,
            Text operationalCostText,
            Text priceText,
            Text estimatedBuildTimeText,
            RectTransform materialsRoot,
            ShipyardMaterialRow materialTemplate,
            Button orderButton)
        {
            PreviousButton = previousButton;
            NextButton = nextButton;
            ShipImage = shipImage;
            ImageFallbackText = imageFallbackText;
            ShipNameText = shipNameText;
            ClassTagText = classTagText;
            SpeedText = speedText;
            HealthText = healthText;
            CrewText = crewText;
            StorageText = storageText;
            OperationalCostText = operationalCostText;
            PriceText = priceText;
            EstimatedBuildTimeText = estimatedBuildTimeText;
            MaterialsRoot = materialsRoot;
            MaterialTemplate = materialTemplate;
            OrderButton = orderButton;
        }
    }

    readonly struct ShipyardQueueRefs
    {
        public readonly ShipyardConstructionRow UnderConstructionRow;
        public readonly RectTransform QueueRoot;
        public readonly ShipyardConstructionRow QueueTemplate;
        public readonly Text EmptyText;

        public ShipyardQueueRefs(ShipyardConstructionRow underConstructionRow, RectTransform queueRoot, ShipyardConstructionRow queueTemplate, Text emptyText)
        {
            UnderConstructionRow = underConstructionRow;
            QueueRoot = queueRoot;
            QueueTemplate = queueTemplate;
            EmptyText = emptyText;
        }
    }

    readonly struct ShipyardBuyRefs
    {
        public readonly Button PreviousButton;
        public readonly Button NextButton;
        public readonly Text IndexText;
        public readonly RectTransform CardsRoot;
        public readonly ShipyardBuyCard CardTemplate;
        public readonly Text EmptyText;

        public ShipyardBuyRefs(Button previousButton, Button nextButton, Text indexText, RectTransform cardsRoot, ShipyardBuyCard cardTemplate, Text emptyText)
        {
            PreviousButton = previousButton;
            NextButton = nextButton;
            IndexText = indexText;
            CardsRoot = cardsRoot;
            CardTemplate = cardTemplate;
            EmptyText = emptyText;
        }
    }

    readonly struct PopupRefs
    {
        public readonly RectTransform Root;
        public readonly CanvasGroup Group;
        public readonly Text Title;
        public readonly Button CloseButton;
        public readonly RectTransform PlaceholderRoot;
        public readonly MarketPopupView MarketView;
        public readonly ShipyardPopupView ShipyardView;

        public PopupRefs(RectTransform root, CanvasGroup group, Text title, Button closeButton, RectTransform placeholderRoot, MarketPopupView marketView, ShipyardPopupView shipyardView)
        {
            Root = root;
            Group = group;
            Title = title;
            CloseButton = closeButton;
            PlaceholderRoot = placeholderRoot;
            MarketView = marketView;
            ShipyardView = shipyardView;
        }
    }

    readonly struct CameraPose
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly float FieldOfView;

        public CameraPose(Vector3 position, Quaternion rotation, float fieldOfView)
        {
            Position = position;
            Rotation = rotation;
            FieldOfView = fieldOfView;
        }
    }

    readonly struct FooterButtonSnapshot
    {
        readonly bool hasValue;
        readonly Sprite defaultSprite;
        readonly Sprite highlightedSprite;
        readonly Sprite selectedSprite;
        readonly Sprite disabledSprite;
        readonly Color defaultTint;
        readonly Color highlightedTint;
        readonly Color selectedTint;
        readonly Color disabledTint;
        readonly Color defaultBackplate;
        readonly Color highlightedBackplate;
        readonly Color selectedBackplate;
        readonly Color disabledBackplate;

        FooterButtonSnapshot(
            Sprite defaultSprite,
            Sprite highlightedSprite,
            Sprite selectedSprite,
            Sprite disabledSprite,
            Color defaultTint,
            Color highlightedTint,
            Color selectedTint,
            Color disabledTint,
            Color defaultBackplate,
            Color highlightedBackplate,
            Color selectedBackplate,
            Color disabledBackplate)
        {
            hasValue = true;
            this.defaultSprite = defaultSprite;
            this.highlightedSprite = highlightedSprite;
            this.selectedSprite = selectedSprite;
            this.disabledSprite = disabledSprite;
            this.defaultTint = defaultTint;
            this.highlightedTint = highlightedTint;
            this.selectedTint = selectedTint;
            this.disabledTint = disabledTint;
            this.defaultBackplate = defaultBackplate;
            this.highlightedBackplate = highlightedBackplate;
            this.selectedBackplate = selectedBackplate;
            this.disabledBackplate = disabledBackplate;
        }

        public static FooterButtonSnapshot Capture(CityFooterButton button)
        {
            SerializedObject so = new SerializedObject(button);
            return new FooterButtonSnapshot(
                so.FindProperty("defaultSprite").objectReferenceValue as Sprite,
                so.FindProperty("highlightedSprite").objectReferenceValue as Sprite,
                so.FindProperty("selectedSprite").objectReferenceValue as Sprite,
                so.FindProperty("disabledSprite").objectReferenceValue as Sprite,
                so.FindProperty("defaultTint").colorValue,
                so.FindProperty("highlightedTint").colorValue,
                so.FindProperty("selectedTint").colorValue,
                so.FindProperty("disabledTint").colorValue,
                so.FindProperty("defaultBackplate").colorValue,
                so.FindProperty("highlightedBackplate").colorValue,
                so.FindProperty("selectedBackplate").colorValue,
                so.FindProperty("disabledBackplate").colorValue);
        }

        public void ApplyTo(CityFooterButton button)
        {
            if (!hasValue || button == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(button);
            so.FindProperty("defaultSprite").objectReferenceValue = defaultSprite;
            so.FindProperty("highlightedSprite").objectReferenceValue = highlightedSprite;
            so.FindProperty("selectedSprite").objectReferenceValue = selectedSprite;
            so.FindProperty("disabledSprite").objectReferenceValue = disabledSprite;
            so.FindProperty("defaultTint").colorValue = defaultTint;
            so.FindProperty("highlightedTint").colorValue = highlightedTint;
            so.FindProperty("selectedTint").colorValue = selectedTint;
            so.FindProperty("disabledTint").colorValue = disabledTint;
            so.FindProperty("defaultBackplate").colorValue = defaultBackplate;
            so.FindProperty("highlightedBackplate").colorValue = highlightedBackplate;
            so.FindProperty("selectedBackplate").colorValue = selectedBackplate;
            so.FindProperty("disabledBackplate").colorValue = disabledBackplate;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

}
