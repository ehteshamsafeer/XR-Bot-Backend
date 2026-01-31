using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UnityXRRealtimeVoice : MonoBehaviour
{
    ClientWebSocket ws;
    AudioSource audioSource;

    AudioClip micClip;
    int lastSamplePos = 0;

    const int SAMPLE_RATE = 24000;
    const float SEND_INTERVAL = 0.5f; // seconds

    bool allowMicStreaming = true;
    bool isAttached = false;
    async void Start()
    {
        // Audio output (AI voice)
        audioSource = GetComponent<AudioSource>();

        // WebSocket
        ws = new ClientWebSocket();
        Uri uri = new Uri("wss://xr-bot-backend-production.up.railway.app/ws"); //Railway deploy
        //Uri uri = new Uri("ws://127.0.0.1:8000/ws"); // localhost

        Debug.Log("Connecting to backend...");
        await ws.ConnectAsync(uri, CancellationToken.None);
        Debug.Log("Connected to backend");

        // Start mic AFTER connection
        micClip = Microphone.Start(null, true, 1, SAMPLE_RATE);
        lastSamplePos = 0;

        InvokeRepeating(nameof(SendMicAudio), SEND_INTERVAL, SEND_INTERVAL);
        ReceiveLoop();
    }

    // ===================== SEND MIC AUDIO =====================

    async void SendMicAudio()
    {
        if (allowMicStreaming == false) return;
        
        if (ws.State != WebSocketState.Open || micClip == null)
            return;

        int pos = Microphone.GetPosition(null);
        if (pos < lastSamplePos)
            lastSamplePos = 0;

        int samplesAvailable = pos - lastSamplePos;
        if (samplesAvailable <= 0)
            return;

        float[] samples = new float[samplesAvailable];
        micClip.GetData(samples, lastSamplePos);
        lastSamplePos = pos;

        byte[] pcm = FloatToPCM16(samples);
        string base64 = Convert.ToBase64String(pcm);

        string json = JsonUtility.ToJson(new AudioInput
        {
            type = "audio_input",
            data = base64
        });

        await ws.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }

    byte[] FloatToPCM16(float[] samples)
    {
        byte[] pcm = new byte[samples.Length * 2];

        for (int i = 0; i < samples.Length; i++)
        {
            short s = (short)Mathf.Clamp(samples[i] * 32767f, -32768, 32767);
            BitConverter.GetBytes(s).CopyTo(pcm, i * 2);
        }
        return pcm;
    }
    // ===================== RECEIVE AI AUDIO =====================

    async void ReceiveLoop()
    {
        byte[] buffer = new byte[8192];
        StringBuilder messageBuilder = new StringBuilder();

        while (ws.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;

            do
            {
                result = await ws.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None
                );

                if (result.MessageType == WebSocketMessageType.Close)
                    return;

                messageBuilder.Append(
                    Encoding.UTF8.GetString(buffer, 0, result.Count)
                );

            } while (!result.EndOfMessage);

            string fullMessage = messageBuilder.ToString();
            messageBuilder.Clear();

            HandleMessage(fullMessage);
        }
    }


    void HandleMessage(string json)
    {
        AudioMessage msg = JsonUtility.FromJson<AudioMessage>(json);

        if (msg.type == "audio_output")
        {
            PlayPCM16(msg.data);
        }
        if (msg.type == "ai_done")
        {
            Invoke("ResumeMic", 4f);
        }
    }

    void PlayPCM16(string base64)
    {
        if (string.IsNullOrEmpty(base64))
        {
            Debug.LogWarning("Empty audio payload, skipping");
            return;
        }

        byte[] pcm = Convert.FromBase64String(base64);

        if (pcm.Length < 4) // less than 1 sample
        {
            Debug.LogWarning("PCM too small, skipping");
            return;
        }

        int sampleCount = pcm.Length / 2;

        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            short s = BitConverter.ToInt16(pcm, i * 2);
            samples[i] = Mathf.Clamp(s / 32768f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create(
            "AI_Response",
            sampleCount,
            1,
            SAMPLE_RATE, // 24000
            false
        );

        clip.SetData(samples, 0);

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();

        Debug.Log($"Playing AI audio | Samples: {sampleCount}");
    }


    // ===================== DATA STRUCTS =====================

    [Serializable]
    class AudioInput
    {
        public string type;
        public string data;
    }

    [Serializable]
    class AudioMessage
    {
        public string type;
        public string data;
    }

    async void OnApplicationQuit()
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "quit", CancellationToken.None);
        }
    }
    public async void NotifyStepComplete(GameObject obj)
    {
        string message;
        if (ws == null || ws.State != WebSocketState.Open)
            return;
        Debug.Log("Mic Streaming Paused!");
        allowMicStreaming = false;
        if (isAttached)
        {
            message = $"{obj.name} is attached.";
        }else
        {
            message = $"{obj.name} is deattached.";
        }
        
        Debug.Log(message);
        string json = JsonUtility.ToJson(new StepEvent
        {
            type = "task_completed",
            message = message
        });
        
        await ws.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }

    [Serializable]
    private class StepEvent
    {
        public string type;
        public string message;
    }
    void ResumeMic()
    {
        allowMicStreaming = true;
        Debug.Log("Mic Streaming Resumes!");
    }
    public void SetIsAttaced(bool value)
    {
        if (value) isAttached = true;
        else isAttached = false;
    }

}
