using UnityEngine;

namespace Assets.Scripts.Cursor
{
    public sealed class RuntimeCursorController : MonoBehaviour
    {
        private const string CursorPrefabResourcePath = "Cursor/RuntimeCursorController";

        private static RuntimeCursorController instance;

        [SerializeField] private Texture2D cursorTexture;
        [SerializeField] private Vector2 hotspot = Vector2.zero;
        [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (instance != null)
            {
                return;
            }

            GameObject prefab = Resources.Load<GameObject>(CursorPrefabResourcePath);
            if (prefab == null)
            {
                return;
            }

            GameObject root = Instantiate(prefab);
            root.name = prefab.name;
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                ApplyCursor();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void ApplyCursor()
        {
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
        }
    }
}
