using Domain;
using UnityEngine;

namespace Presentation
{
    public static class GridPositionExtensions
    {
        public static Vector2 ToVector2(this GridPosition gridPosition)
        {
            return new Vector2(gridPosition.X, gridPosition.Y);
        }

        public static Vector3 ToVector3(this GridPosition gridPosition)
        {
            return new Vector3(gridPosition.X, gridPosition.Y);
        }
    }
}