using UnityEngine;
using TMPro;

public class RobotController : MonoBehaviour
{
    UnityXRRealtimeVoice AIState;
    public AudioSource audioSource;
    public TMP_Text stateText;
    public Animator anim;
    void Start()
    {

    }

    void Update()
    {
        if (audioSource.isPlaying)
        {
            anim.SetBool("isTalking", true);
            stateText.text = "Speaking ...";
        }
        else
        {
            anim.SetBool("isTalking", false);
            if (Microphone.IsRecording(null))
            {
                stateText.text = "Listening ...";
            }
            else
            {
                stateText.text = "Processing ...";
            }
        }        
        
    }
}
