using Assets.Scripts.Data;
using Assets.Scripts.MapScene;
using UnityEngine;

public class Water : MonoBehaviour, IClickable
{
    public void OnClicked(RaycastHit hit, int mouseButton)
    {
        // Right mouse button (1) sets ship destination
        if (mouseButton == 1)
        {
            if (ShipManager.instance.SelectedShip.userOwned)
            {
                ShipManager.instance.SelectedShip?.UpdateDestination(hit.point);
            }
        }
        else if (mouseButton == 0)
        {
            ShipManager.instance.SelectedShip = null;
        }
    }
}
