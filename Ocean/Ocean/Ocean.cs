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
    class Ocean
    {
        Model ocean;
        Texture2D diffuseOceanTexture;
        Texture2D normalOceanTexture;

        Effect oceanEffect;

        float oceanLengthScale = 1000;
        float oceanHeightScale = 1000;
        float rippleSpeed = 12;
        const float rippleHeight = 0.0625f;

        Vector3 lightDirection;

        #region effect parameters
        EffectParameter projectionOceanParameter;
        EffectParameter viewOceanParameter;
        EffectParameter worldOceanParameter;
        EffectParameter ambientIntensityOceanParameter;
        EffectParameter ambientColorOceanParameter;

        EffectParameter diffuseIntensityOceanParameter;
        EffectParameter diffuseColorOceanParameter;
        EffectParameter lightDirectionOceanParameter;

        EffectParameter eyePosOceanParameter;
        EffectParameter specularColorOceanParameter;

        EffectParameter colorMapTextureOceanParameter;
        EffectParameter normalMapTextureOceanParameter;
        EffectParameter totalTimeOceanParameter;
        EffectParameter rippleSpeedParameter;
        EffectParameter rippleHeightParameter;
        #endregion


        public Ocean(Model model, Effect oceanEffect, Texture2D diffuse, Texture2D normal, Vector3 lightDirection)
        {
            this.ocean = model;
            this.oceanEffect = oceanEffect;
            this.diffuseOceanTexture = diffuse;
            this.normalOceanTexture = normal;

            this.lightDirection = lightDirection;

            SetupOceanShaderParameters();
        }

        public void SetupOceanShaderParameters()
        {
            worldOceanParameter = oceanEffect.Parameters["World"];
            viewOceanParameter = oceanEffect.Parameters["View"];
            projectionOceanParameter = oceanEffect.Parameters["Projection"];

            ambientColorOceanParameter = oceanEffect.Parameters["AmbientColor"];
            ambientIntensityOceanParameter = oceanEffect.Parameters["AmbientIntensity"];

            diffuseColorOceanParameter = oceanEffect.Parameters["DiffuseColor"];
            diffuseIntensityOceanParameter = oceanEffect.Parameters["DiffuseIntensity"];
            lightDirectionOceanParameter = oceanEffect.Parameters["LightDirection"];

            eyePosOceanParameter = oceanEffect.Parameters["EyePosition"];
            specularColorOceanParameter = oceanEffect.Parameters["SpecularColor"];

            colorMapTextureOceanParameter = oceanEffect.Parameters["ColorMap"];
            normalMapTextureOceanParameter = oceanEffect.Parameters["NormalMap"];
            totalTimeOceanParameter = oceanEffect.Parameters["TotalTime"];

            rippleSpeedParameter = oceanEffect.Parameters["rippleSpeed"];
            rippleHeightParameter = oceanEffect.Parameters["rippleHeight"];
        }


        public void DrawOcean(GameTime gameTime, GraphicsDeviceManager graphics, Vector3 position, Camera camera)
        {
            ModelMesh mesh = ocean.Meshes[0];
            ModelMeshPart meshPart = mesh.MeshParts[0];

            projectionOceanParameter.SetValue(camera.projection);
            viewOceanParameter.SetValue(camera.view);
            worldOceanParameter.SetValue(Matrix.CreateRotationY((float)MathHelper.ToRadians((int)270)) * Matrix.CreateRotationZ((float)MathHelper.ToRadians((int)90)) * Matrix.CreateScale(oceanLengthScale, oceanHeightScale, oceanLengthScale) * Matrix.CreateTranslation(0, -60, 0));
            ambientIntensityOceanParameter.SetValue(0.4f);
            ambientColorOceanParameter.SetValue(Color.White.ToVector4());
            diffuseColorOceanParameter.SetValue(Color.White.ToVector4());
            diffuseIntensityOceanParameter.SetValue(0.2f);
            specularColorOceanParameter.SetValue(Color.White.ToVector4());
            eyePosOceanParameter.SetValue(position);
            colorMapTextureOceanParameter.SetValue(diffuseOceanTexture);
            normalMapTextureOceanParameter.SetValue(normalOceanTexture);
            totalTimeOceanParameter.SetValue((float)gameTime.TotalGameTime.TotalMilliseconds / 5000f);
            rippleSpeedParameter.SetValue(rippleSpeed);
            rippleHeightParameter.SetValue(rippleHeight);
            lightDirectionOceanParameter.SetValue(lightDirection);


            graphics.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);

            graphics.GraphicsDevice.Indices = meshPart.IndexBuffer;

            graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            oceanEffect.CurrentTechnique = oceanEffect.Techniques["Technique1"];

            for (int i = 0; i < oceanEffect.CurrentTechnique.Passes.Count; i++)
            {
                oceanEffect.CurrentTechnique.Passes[i].Apply();

                graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
            }
        }

    }
}
