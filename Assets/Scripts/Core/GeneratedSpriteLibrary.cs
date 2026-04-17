using UnityEngine;

namespace SpaceLoader.Core
{
    public static class GeneratedSpriteLibrary
    {
        private static Sprite squareSprite;
        private static Sprite circleSprite;

        public static Sprite SquareSprite => squareSprite != null ? squareSprite : squareSprite = CreateSquareSprite();
        public static Sprite CircleSprite => circleSprite != null ? circleSprite : circleSprite = CreateCircleSprite();

        private static Sprite CreateSquareSprite()
        {
            return CreateSprite(64, (x, y, size) => Color.white);
        }

        private static Sprite CreateCircleSprite()
        {
            return CreateSprite(128, (x, y, size) =>
            {
                float half = (size - 1) * 0.5f;
                Vector2 point = new Vector2(x - half, y - half);
                float radius = half * 0.9f;

                return point.sqrMagnitude <= radius * radius ? Color.white : Color.clear;
            });
        }

        private static Sprite CreateSprite(int size, System.Func<int, int, int, Color> getColor)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = getColor(x, y, size);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
    }
}
