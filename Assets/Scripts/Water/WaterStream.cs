using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterStream : MonoBehaviour
{
    public enum BoostMode
    {
        None,
        AnyBoost,
        WindBoost
    }

    [Header("Direction")]
    [SerializeField] private bool useForward = true;
    [SerializeField] private Vector3 direction = Vector3.forward;

    [Header("Block")]
    [SerializeField] private float needSpeed = 3f;     // benötigte Geschwindigkeit gegen die Strömung
    [SerializeField] private float blockMax = 0f;      // max. erlaubte Geschwindigkeit gegen die Strömung, wenn blockiert

    [Header("Resistance")]
    [SerializeField] private float resistMin = 4f;     
    [SerializeField] private float resistMax = 18f;    
    [SerializeField] private float resistPower = 1.6f; // wie schnell resist von min->max steigt

    [SerializeField] private float flowAssist = 0f;    

    [Header("Boost")]
    [SerializeField] private BoostMode boost = BoostMode.WindBoost;

    [Header("Filter")]
    [SerializeField] private LayerMask layers = 0;

    private struct Refs
    {
        public Rigidbody rb;
        public BoatControl boat;
        public WindGustAbility wind;
    }

    private readonly Dictionary<Rigidbody, Refs> inside = new();

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!LayerOk(other.gameObject.layer)) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;
        if (inside.ContainsKey(rb)) return;

        inside.Add(rb, new Refs
        {
            rb = rb,
            boat = rb.GetComponentInParent<BoatControl>(),
            wind = rb.GetComponentInParent<WindGustAbility>()
        });
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;
        inside.Remove(rb);
    }

    private void FixedUpdate()
    {
        if (inside.Count == 0) return;

        var gsm = GameStateManager.Instance;
        if (gsm != null && (gsm.State == GameState.Docked || gsm.State == GameState.Dialogue || gsm.State == GameState.Paused))
            return;

        Vector3 flowDir = GetDir();       // Strömung fließt so
        Vector3 upDir = -flowDir;         // gegen die Strömung

        foreach (var entry in inside)
        {
            Refs refs = entry.Value;
            if (refs.rb == null) continue;

            bool boostOk = BoostOk(refs.boat, refs.wind);

            Vector3 velFlat = refs.rb.linearVelocity;
            velFlat.y = 0f;

            float speedUp = Vector3.Dot(velFlat, upDir); // >0:  bewegt sich gegen die Strömung

            bool triesUp = speedUp > 0.01f;

            if (triesUp)
            {
                float ratio = 1f;
                if (needSpeed > 0.0001f) ratio = Mathf.Clamp01(speedUp / needSpeed);

                float ramp = Mathf.Pow(ratio, resistPower);
                float resist = Mathf.Lerp(resistMin, resistMax, ramp);

                bool fastEnough = speedUp >= needSpeed;
                bool canPass = boostOk && fastEnough;

                if (!canPass)
                {
                    CapUp(refs.rb, upDir, blockMax);
                    refs.rb.AddForce(flowDir * resist, ForceMode.Acceleration);
                    continue;
                }
            }

            if (flowAssist > 0f)
                refs.rb.AddForce(flowDir * flowAssist, ForceMode.Acceleration);
        }
    }

    private void CapUp(Rigidbody rb, Vector3 upDir, float maxUp)
    {
        Vector3 vel = rb.linearVelocity;
        float speedUp = Vector3.Dot(vel, upDir);

        if (speedUp <= maxUp) return;

        float remove = speedUp - maxUp;
        vel -= upDir * remove;

        rb.linearVelocity = vel;
    }

    private bool BoostOk(BoatControl boat, WindGustAbility wind)
    {
        if (boost == BoostMode.None) return true;
        if (boost == BoostMode.AnyBoost) return boat != null && boat.boostActive;
        if (boost == BoostMode.WindBoost) return wind != null && wind.IsBoosting;
        return true;
    }

    private Vector3 GetDir()
    {
        Vector3 d = useForward ? transform.forward : direction;
        d.y = 0f;
        if (d.sqrMagnitude < 0.0001f) d = Vector3.forward;
        return d.normalized;
    }

    private bool LayerOk(int layer)
    {
        if (layers.value == 0) return true;
        return (layers.value & (1 << layer)) != 0;
    }
}