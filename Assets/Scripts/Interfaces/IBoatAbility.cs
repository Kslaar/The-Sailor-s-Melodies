using UnityEngine;

public interface IBoatAbility
{
    bool CanActivate();
    void Activate();
    void Tick(); // Manche Fähigkeiten dauern an
    void Deactivate();
}
