using UnityEngine;

namespace General_Scripts.AI
{
    public interface IUnit
    {
        void SetTarget(Transform transform);
        bool DoFollowPathStep();
    }
}