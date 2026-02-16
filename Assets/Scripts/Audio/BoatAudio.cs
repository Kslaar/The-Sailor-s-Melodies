using UnityEngine;

public class BoatAudio : MonoBehaviour
{
    [Header("Wwise States")]
    public AK.Wwise.State SailingState;
    public AK.Wwise.State MotorState;

    [Header("Wwise Events")]
    public AK.Wwise.Event PlaySailing;
    public AK.Wwise.Event StopSailing;
    public AK.Wwise.Event MotorStart;
    public AK.Wwise.Event MotorStop;

    [Header("RTPCs")]
    public AK.Wwise.RTPC SpeedRTPC;

    [Header("Rigidbody")]
    public Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Startet im Segelmodus
        SailingState.SetValue();
        PlaySailing.Post(gameObject);
    }

    void Update()
    {
        if (SpeedRTPC != null && rb != null)
        {
            SpeedRTPC.SetValue(gameObject, rb.linearVelocity.magnitude);
        }
    }

    public void ActivateMotor()
    {
        StopSailing.Post(gameObject);   // Sailing-Sounds sauber beenden
        MotorState.SetValue();          // StateGroup umschalten
        MotorStart.Post(gameObject);     // Motor Start Ger�usch
    }

    public void DeactivateMotor()
    {
        MotorStop.Post(gameObject);      // Motor Aus
        SailingState.SetValue();         // zur�ck zu Sailing
        PlaySailing.Post(gameObject);    // Sailing Loops starten
    }
}