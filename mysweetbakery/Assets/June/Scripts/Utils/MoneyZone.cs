using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyZone : MonoBehaviour
{

    public bool isPlayerDeteced;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Æ©Åä¸®¾ó
            if (TutorialManager.Instance.currentStep == TutorialStep.PosTable)
            {
                TutorialManager.Instance.currentStep = TutorialStep.ReceiveMoney;
            }

            if (GameManager.Instance.playerMoneyCount > 0)
            {
                SoundManager.instance.PlayAudio(audioSource.Cost_Money);

            }

            isPlayerDeteced = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerDeteced = false;
        }
    }
}
