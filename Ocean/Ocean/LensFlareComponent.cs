using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Ocean
{
    class LensFlareComponent
    {
        const float glowSize = 300;
        const float querySize = 10;

        Vector2 lightPosition;
        bool lightBehindCamera;


        Texture2D glowSprite;
        SpriteBatch spriteBatch;
        BasicEffect basicEffect;
        VertexPositionColor[] queryVertices;

        static readonly BlendState ColorWriteDisable = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None
        };

        OcclusionQuery occlusionQuery;
        bool occlusionQueryActive;
        float occlusionAlpha;

        #region MyRegion
        class Flare
        {
            public Flare(float position, float scale, Color color, string textureName)
            {
                Position = position;
                Scale = scale;
                Color = color;
                TextureName = textureName;
            }

            public float Position;
            public float Scale;
            public Color Color;
            public string TextureName;
            public Texture2D Texture;
        }

        Flare[] flares =
        {
            new Flare(-0.5f, 0.7f, new Color( 50,  25,  50), "flare1"),
            new Flare( 0.3f, 0.4f, new Color(100, 255, 200), "flare1"),
            new Flare( 1.2f, 1.0f, new Color(100,  50,  50), "flare1"),
            new Flare( 1.5f, 1.5f, new Color( 50, 100,  50), "flare1"),

            new Flare(-0.3f, 0.7f, new Color(200,  50,  50), "flare2"),
            new Flare( 0.6f, 0.9f, new Color( 50, 100,  50), "flare2"),
            new Flare( 0.7f, 0.4f, new Color( 50, 200, 200), "flare2"),

            new Flare(-0.7f, 0.7f, new Color( 50, 100,  25), "flare3"),
            new Flare( 0.0f, 0.6f, new Color( 25,  25,  25), "flare3"),
            new Flare( 2.0f, 1.4f, new Color( 25,  50, 100), "flare3"),
        };
        #endregion


        public LensFlareComponent(SpriteBatch sprite, ContentManager cont, GraphicsDevice device)
        {
            this.spriteBatch = sprite;
            glowSprite = cont.Load<Texture2D>("Textures/glow");

            foreach (Flare flare in flares)
            {
                flare.Texture = cont.Load<Texture2D>("Textures/" + flare.TextureName);
            }


            basicEffect = new BasicEffect(device);

            basicEffect.View = Matrix.Identity;
            basicEffect.VertexColorEnabled = true;

            queryVertices = new VertexPositionColor[4];

            queryVertices[0].Position = new Vector3(-querySize / 2, -querySize / 2, -1);
            queryVertices[1].Position = new Vector3(querySize / 2, -querySize / 2, -1);
            queryVertices[2].Position = new Vector3(-querySize / 2, querySize / 2, -1);
            queryVertices[3].Position = new Vector3(querySize / 2, querySize / 2, -1);

            occlusionQuery = new OcclusionQuery(device);
        }

        public void Draw(Matrix view, Matrix projection, Vector3 lightDirection, GraphicsDevice device, GameWindow window)
        {
            UpdateOcclusion(view, projection, lightDirection, device, window);

            DrawGlow();
            DrawFlares(device.Viewport);

            RestoreRenderStates(device);
        }

        private void UpdateOcclusion(Matrix View, Matrix Projection, Vector3 LightDirection, GraphicsDevice device, GameWindow window)
        {
            Matrix infiniteView = View;
            infiniteView.Translation = Vector3.Zero;
            Vector3 projectedPosition = device.Viewport.Project(LightDirection, Projection, infiniteView, Matrix.Identity);

            if ((projectedPosition.Z < 0) || (projectedPosition.Z > 1))
            {
                lightBehindCamera = true;
                return;
            }
            else lightBehindCamera = false;

            lightPosition = new Vector2(projectedPosition.X, projectedPosition.Y - 150);


            if (occlusionQueryActive)
            {
                if (!occlusionQuery.IsComplete)
                    return;

                const float queryArea = querySize * querySize;

                occlusionAlpha = Math.Min(occlusionQuery.PixelCount / queryArea, 1);
            }

            device.BlendState = ColorWriteDisable;
            device.DepthStencilState = DepthStencilState.DepthRead;

            basicEffect.World = Matrix.CreateTranslation(lightPosition.X, lightPosition.Y, 0);

            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, device.Viewport.Height, 0, 0, 1);

            basicEffect.CurrentTechnique.Passes[0].Apply();

            occlusionQuery.Begin();

            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, queryVertices, 0, 2);

            occlusionQuery.End();

            occlusionQueryActive = true;
        }

        private void DrawGlow()
        {
            if (lightBehindCamera) return;
            Color color = Color.White;// * occlusionAlpha;
            Vector2 origin = new Vector2(glowSprite.Width, glowSprite.Height) / 2;
            float scale = glowSize * 2 / glowSprite.Width;

            spriteBatch.Begin();

            spriteBatch.Draw(glowSprite, lightPosition, null, color, 0, origin, scale, SpriteEffects.None, 0);

            spriteBatch.End();
        }

        private void DrawFlares(Viewport viewport)
        {
            if (lightBehindCamera) return;
            Vector2 screenCenter = new Vector2(viewport.Width, viewport.Height) / 2;

            Vector2 flareVector = screenCenter - lightPosition;

            spriteBatch.Begin(0, BlendState.Additive);

            foreach (Flare flare in flares)
            {
                Vector2 flarePosition = lightPosition + flareVector * flare.Position;
                Vector4 flareColor = flare.Color.ToVector4();
                flareColor.W *= occlusionAlpha;

                Vector2 flareOrigin = new Vector2(flare.Texture.Width, flare.Texture.Height) / 2;

                spriteBatch.Draw(flare.Texture, flarePosition, null, new Color(flareColor), 1, flareOrigin, flare.Scale, SpriteEffects.None, 0);
            }
            spriteBatch.End();
        }

        private void RestoreRenderStates(GraphicsDevice device)
        {
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            device.SamplerStates[0] = SamplerState.LinearWrap;
        }
    }
}
