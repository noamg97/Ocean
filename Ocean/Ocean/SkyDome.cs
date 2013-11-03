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
    class SkyDome
    {
        private Model model;
        private Texture2D texture;

        public SkyDome(Model model, Texture2D texture)
        {
            this.model = model;
            this.texture = texture;
        }

        public void DrawDome(Vector3 position, Matrix view, Matrix projection)
        {
            Matrix[] modelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            Matrix world = Matrix.CreateScale(305) * Matrix.CreateTranslation(new Vector3(0, -60, 0));

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect cEffect in mesh.Effects)
                {
                    cEffect.TextureEnabled = true;
                    cEffect.Texture = texture;
                    cEffect.View = view;
                    cEffect.World = modelTransforms[mesh.ParentBone.Index] * world;
                    cEffect.Projection = projection;
                    cEffect.EmissiveColor = new Vector3(0.6f);
                }

                mesh.Draw();
            }
        }
    }
}