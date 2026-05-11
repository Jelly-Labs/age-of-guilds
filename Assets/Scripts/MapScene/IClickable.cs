using UnityEngine;

namespace Assets.Scripts.MapScene
{
    public interface IClickable
    {
        void OnClicked(RaycastHit hit, int mouseButton);
    }
}
