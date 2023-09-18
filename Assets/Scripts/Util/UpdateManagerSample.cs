using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using kyon;

namespace kyon
{
	public class UpdateManagerTemplete : MonoBehaviour
	{
        private void Start()
        {
            //UpdateManager‚ÌSubject‚ÅManagedUpdate‚ð“o˜^
            //gameObject‚ª”jŠü‚³‚ê‚½‚Æ‚«‚ÉŽ©“®‚ÅŽ~‚ß‚½‚¢ê‡‚ÍAddTo(gameObject) (using UniRx‚ª•K—v)
            UpdateManager.Subscribe(_ => ManagedUpdate()).AddTo(gameObject);
        }

        //Update‚ðŽg‚í‚¸Ž©•ª‚Å‚»‚ê‚Á‚Û‚­’è‹`‚·‚é
        private void ManagedUpdate()
        {

        }
    }
}