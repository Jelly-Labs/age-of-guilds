using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.Scripts.MapScene
{
    public class InputManager : MonoBehaviour
    {
        private static InputManager instance;

        public static InputManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Initialize();
                }
                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (instance == null)
            {
                GameObject obj = new("InputManager");
                instance = obj.AddComponent<InputManager>();
                DontDestroyOnLoad(obj);
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            // Check for left mouse button click
            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleClick(0);
            }

            // Check for right mouse button click
            if (Mouse.current != null && Mouse.current.rightButton.wasReleasedThisFrame)
            {
                HandleClick(1);
            }

            // Check for middle mouse button click
            if (Mouse.current != null && Mouse.current.middleButton.wasReleasedThisFrame)
            {
                HandleClick(2);
            }
        }

        private void HandleClick(int mouseButton)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if the hit object or any of its parents implements IClickable
                IClickable clickable = hit.collider.GetComponentInParent<IClickable>();

                if (clickable != null)
                {
                    clickable.OnClicked(hit, mouseButton);
                }
            }
        }
    }
}
