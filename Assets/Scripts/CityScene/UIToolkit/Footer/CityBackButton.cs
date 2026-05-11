using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.CityScene
{
	public class CityBackButton: CityFooterButton
	{

        protected override void Click()
        {
            SceneManager.LoadScene("MapScene");
		}
}
}