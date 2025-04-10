using System;
using BotSimZero.World.Terrain;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Engine.Events;
using Stride.Graphics;
using Stride.Graphics.GeometricPrimitives;
using Stride.Input;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;

namespace BotSimZero.World.UI
{
    public class CellHighlighter : SyncScript
    {
        // Parameters for the highlight effect
        public Vector3 Size { get; set; } = new Vector3(1, 1, 1);
        public Color GlowColor { get; set; } = Color.Yellow;
        public float GlowIntensity { get; set; } = 5.0f;
        public float FadeSpeed { get; set; } = 2.0f;

        private Entity highlightEntity;
        private Vector3 targetPosition;
        private bool isHighlightActive = false;
        EventReceiver<Vector3> highlightReceiver;

        // Create and initialize the highlight entity
        public override void Start()
        {
            highlightReceiver = new EventReceiver<Vector3>(GlobalEvents.HighlightCellEventKey);

            // Create the highlight entity
            highlightEntity = CreateHighlightCube();
            highlightEntity.Transform.Scale = Size;
            highlightEntity.Transform.Position = new Vector3(0, -10, 0); // Hide initially

            // Add the highlight to the scene
            Entity.Scene.Entities.Add(highlightEntity);
        }

        public override void Update()
        {
            if (isHighlightActive)
            {
                // Smoothly move to target position
                highlightEntity.Transform.Position = Vector3.Lerp(
                    highlightEntity.Transform.Position,
                    targetPosition,
                    (float)(FadeSpeed * Game.UpdateTime.Elapsed.TotalSeconds));
            } else
            {
                TryHighlightCell();
            }
        }

        // Call this method when a cell is clicked
        public void HighlightCell(Vector3 position)
        {
            targetPosition = position;
            isHighlightActive = true;
            highlightEntity.Transform.Position = position;
        }

        public void TryHighlightCell()
        {
            if (highlightReceiver.TryReceive(out Vector3 data)) {
                HighlightCell(data);
            }
        }

        // Call this to hide the highlight
        public void HideHighlight()
        {
            isHighlightActive = false;
            highlightEntity.Transform.Position = new Vector3(0, -10, 0);
        }

        private Entity CreateHighlightCube()
        {
            var entity = new Entity("CellHighlight");

            // Create the model component for the cube
            var modelComponent = new ModelComponent
            {
                Model = ModelFactory.CreateCube(GraphicsDevice)
            };
            entity.Add(modelComponent);

            // Create the volumetric gradient material
            var material = CreateVolumetricGradientMaterial();
            modelComponent.Materials.Add(new (0, material));

            return entity;
        }

        private Material CreateVolumetricGradientMaterial()
        {
            var material = Material.New(GraphicsDevice, new MaterialDescriptor()
            {
                Attributes = new MaterialAttributes
                {
                    DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                    Emissive = new MaterialEmissiveMapFeature()
                    {
                        Intensity = new ComputeFloat(GlowIntensity),
                        EmissiveMap = CreateVolumetricGradientMap()
                    },
                    Transparency = new MaterialTransparencyBlendFeature
                    {
                        Alpha = new ComputeFloat(0.8f),
                    }
                    
                }
            });

            //// Set up the blend state
            //material.BlendState = BlendStates.AlphaBlend;

            // Make sure the material casts no shadows
            //material.Attributes.ShadowMapCaster = false;

            return material;
        }

        private IComputeColor CreateVolumetricGradientMap()
        {
            var gradientCompute = new ComputeTextureColor
            {
                Texture = CreateGradientTexture(),
                // Use world position for the texture coordinates in Stride
                //Coordinates = new ComputeWorldPosition(),
                // Scale the coordinates to match our cube size
                Scale = new Vector2(1, 1),
                Offset = new Vector2(0, 0)
            };

            // We can use ComputeColor directly since we're using a pre-multiplied texture
            // This avoids needing to multiply colors in the shader
            return gradientCompute;

            // If we needed to combine with a color, we could use:
            // return new ComputeColor(GlowColor); 
        }

        private Texture CreateGradientTexture()
        {
            // Create a 1D gradient texture - more opaque at the bottom, fading to transparent at the top
            const int textureSize = 256;
            var textureData = new Color[textureSize];

            for (int i = 0; i < textureSize; i++)
            {
                // Create gradient from bottom (1) to top (0)
                float normalizedHeight = 1.0f - (float)i / textureSize;
                // Add a curve to make the gradient more interesting
                float alpha = MathF.Pow(normalizedHeight, 2.0f);

                // Apply color with alpha - pre-multiply with glow color here
                textureData[i] = new Color(GlowColor.R, GlowColor.G, GlowColor.B, alpha);
            }

            // Create the texture
            var texture = Texture.New2D(GraphicsDevice, textureSize, 1,
                PixelFormat.R8G8B8A8_UNorm, textureData, TextureFlags.ShaderResource);

            // Fill the texture with our gradient data

            return texture;
        }
    }
}
