using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ChopperController : MonoBehaviour
{
    [Header("Motor Settings")]
    [SerializeField] XRSocketInteractor MotorSocket;
    [SerializeField] XRSocketInteractor BatterySocket;
    [SerializeField] Transform rotorTransform;
    [SerializeField] float acceleration = 200f;
    [SerializeField] float deceleration = 300f;
    [SerializeField] float currentSpeed = 0f;
    [SerializeField] float maxSpeed = 1500f;

    [Header("Audio Settings")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] float maxPitch = 1.3f;
    [SerializeField] float picthUpFactor = 0.05f;
    [SerializeField] float currentAudioPitch = 1f;
    [SerializeField] float maxVolume = 1.3f;
    [SerializeField] float volumeDownFactor = 0.23f;
    [SerializeField] float volumeUpFactor = 0.2f;
    [SerializeField] float currrentAudioVolume = 0f;


    void FixedUpdate()
    {
        CheckMotorAudio();
        CheckMotorBladesConnection();
    }

    void CheckMotorAudio()
    {

        float reduce = 0.23f;
        if (BatterySocket.hasSelection)
        {
            currentAudioPitch = Mathf.MoveTowards(currentAudioPitch, maxPitch, picthUpFactor * Time.deltaTime);
            currrentAudioVolume = Mathf.MoveTowards(currrentAudioVolume, maxVolume, volumeUpFactor * Time.deltaTime);
        }
        else
        {
            currentAudioPitch = Mathf.MoveTowards(currentAudioPitch, 1f, picthUpFactor * Time.deltaTime);
            currrentAudioVolume = Mathf.MoveTowards(currrentAudioVolume, 0, volumeDownFactor * Time.deltaTime);            
        }

        audioSource.pitch = currentAudioPitch;
        audioSource.volume = currrentAudioVolume;
        rotorTransform.Rotate(0f, 0f, currentSpeed * Time.deltaTime);
    }
    void CheckMotorBladesConnection()
    {
        if (MotorSocket.hasSelection && BatterySocket.hasSelection)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        } 
    }

}
