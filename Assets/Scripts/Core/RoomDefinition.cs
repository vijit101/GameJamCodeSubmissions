using UnityEngine;

namespace SpaceLoader.Core
{
    public sealed class RoomDefinition
    {
        public RoomDefinition(
            string title,
            string tutorial,
            Vector2 spawnPoint,
            Vector2 roomCenter,
            Vector2 roomSize,
            Vector2 objectivePoint,
            float orthographicSize,
            Color accentColor)
        {
            Title = title;
            Tutorial = tutorial;
            SpawnPoint = spawnPoint;
            RoomCenter = roomCenter;
            RoomSize = roomSize;
            ObjectivePoint = objectivePoint;
            OrthographicSize = orthographicSize;
            AccentColor = accentColor;
        }

        public string Title { get; }
        public string Tutorial { get; }
        public Vector2 SpawnPoint { get; }
        public Vector2 RoomCenter { get; }
        public Vector2 RoomSize { get; }
        public Vector2 ObjectivePoint { get; }
        public float OrthographicSize { get; }
        public Color AccentColor { get; }

        public Rect CameraBounds
        {
            get
            {
                Vector2 half = RoomSize * 0.5f;
                return new Rect(RoomCenter - half, RoomSize);
            }
        }
    }
}
