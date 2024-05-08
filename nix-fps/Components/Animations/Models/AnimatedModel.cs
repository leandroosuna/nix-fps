using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using nixfps.Components.Animations.DataTypes;
using nixfps.Components.Effects;


namespace nixfps.Components.Animations.Models;

/// <summary>
///     An enclosure for an XNA Model that we will use that includes support for Bones, animation, and some manipulations.
/// </summary>
public class AnimatedModel
{
    /// <summary>
    ///     The Model asset name.
    /// </summary>
    private readonly string _assetName;

    /// <summary>
    ///     The underlying Bones for the Model.
    /// </summary>
    private readonly List<Bone> _bones = new();

    /// <summary>
    ///     The actual underlying XNA Model.
    /// </summary>
    private Model _model;

    /// <summary>
    ///     Extra data associated with the XNA Model.
    /// </summary>
    private ModelExtra _modelExtra;

    /// <summary>
    ///     An associated animation clip Player.
    /// </summary>
    public AnimationPlayer _player;

    /// <summary>
    ///     The Model animation clips.
    /// </summary>
    public List<AnimationClip> Clips => _modelExtra.Clips;

    /// <summary>
    ///     Creates the Model from an XNA Model.
    /// </summary>
    /// <param name="assetName">The name of the asset for this Model.</param>
    public AnimatedModel(string assetName)
    {
        _assetName = assetName;
    }

    /// <summary>
    ///     Play an animation clip.
    /// </summary>
    /// <param name="clip">The clip to play.</param>
    /// <returns>The Player that will play this clip.</returns>
    public AnimationPlayer PlayClip(AnimationClip clip)
    {
        // Create a clip Player and assign it to this Model.
        _player = new AnimationPlayer(clip, this);
        return _player;
    }
    public void SetPlayer(AnimationPlayer ap)
    {
        _player = ap;
    }
    /// <summary>
    ///     Update animation for the Model.
    /// </summary>
    public float Update(GameTime gameTime)
    {
        _player.Update(gameTime);
        return _player._position;
    }

    /// <summary>
    ///     Draw the Model.
    /// </summary>
    /// <param name="world">A world matrix to place the Model.</param>
    /// <param name="view">The view matrix, normally from the camera.</param>
    /// <param name="projection">The projection matrix, normally from the application.</param>
    //public void Draw(Matrix world, Matrix view, Matrix projection)
    //{
    //    // Compute all of the bone absolute transforms.
    //    var boneTransforms = new Matrix[_bones.Count];

    //    for (var i = 0; i < _bones.Count; i++)
    //    {
    //        var bone = _bones[i];
    //        bone.ComputeAbsoluteTransform();

    //        boneTransforms[i] = bone.AbsoluteTransform;
    //    }

    //    // Determine the skin transforms from the skeleton.
    //    var skeleton = new Matrix[_modelExtra.Skeleton.Count];
    //    for (var s = 0; s < _modelExtra.Skeleton.Count; s++)
    //    {
    //        var bone = _bones[_modelExtra.Skeleton[s]];
    //        skeleton[s] = bone.SkinTransform * bone.AbsoluteTransform;
    //    }

    //    // Draw the Model.
    //    foreach (var modelMesh in _model.Meshes)
    //    {
    //        foreach (var effect in modelMesh.Effects)
    //        {
    //            var skinnedEffect = effect as SkinnedEffect;
    //            skinnedEffect.World = boneTransforms[modelMesh.ParentBone.Index] * world;
    //            skinnedEffect.View = view;
    //            skinnedEffect.Projection = projection;
    //            skinnedEffect.EnableDefaultLighting();
    //            skinnedEffect.PreferPerPixelLighting = true;
    //            skinnedEffect.SetBoneTransforms(skeleton);
    //        }

    //        modelMesh.Draw();
    //    }
    //}
    public void DrawCustom(NixFPS game, Matrix SRT )
    {
        var animManager = game.animationManager;
        var effect = animManager.effect;
        var camera = game.camera;
        // Compute all of the bone absolute transforms.
        var boneTransforms = new Matrix[_bones.Count];

        for (var i = 0; i < _bones.Count; i++)
        {
            var bone = _bones[i];
            bone.ComputeAbsoluteTransform();

            boneTransforms[i] = bone.AbsoluteTransform;
        }

        // Determine the skin transforms from the skeleton.
        var skeleton = new Matrix[_modelExtra.Skeleton.Count];
        for (var s = 0; s < _modelExtra.Skeleton.Count; s++)
        {
            var bone = _bones[_modelExtra.Skeleton[s]];
            skeleton[s] = bone.SkinTransform * bone.AbsoluteTransform;
        }
        //int c = 1;
        effect.SetCameraPosition(game.camera.position);
        effect.SetAmbientLight(game.lightsManager.ambientLight);
        effect.SetKA(0.3f);
        effect.SetKD(0.8f);
        effect.SetKS(0.8f);
        effect.SetShininess(30f);

        foreach (var mesh in _model.Meshes)
        {
            var worldBone = boneTransforms[mesh.ParentBone.Index] * SRT;

            effect.SetWorld(worldBone);
            effect.SetView(camera.view);
            effect.SetProjection(camera.projection);
            effect.SetBoneTransforms(skeleton);
            effect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(worldBone)));
            
            var part = mesh.MeshParts[0];

            effect.SetTexture(animManager.playerModelTex2);
            foreach (var pass in effect.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                game.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                game.GraphicsDevice.Indices = part.IndexBuffer;
                game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
            part = mesh.MeshParts[1];

            effect.SetTexture(animManager.playerModelTex);
            foreach (var pass in effect.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                game.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                game.GraphicsDevice.Indices = part.IndexBuffer;
                game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }

            //modelMesh.Draw();
        }
    }
    /// <summary>
    ///     Load the Model asset from content.
    /// </summary>
    public Model LoadContent(ContentManager content)
    {
        _model = content.Load<Model>(_assetName);
        
        _modelExtra = _model.Tag as ModelExtra;

        ObtainBones();

        return _model;
    }

    /// <summary>
    ///     Get the Bones from the Model and create a bone class object for each bone. We use our bone class to do the real
    ///     animated bone work.
    /// </summary>
    private void ObtainBones()
    {
        _bones.Clear();
        foreach (var bone in _model.Bones)
        {
            // Create the bone object and add to the hierarchy.
            var newBone = new Bone(bone.Name, bone.Transform, bone.Parent != null ? _bones[bone.Parent.Index] : null);

            // Add to the Bones for this Model.
            _bones.Add(newBone);
        }
    }

    /// <summary>
    ///     Find a bone in this Model by name.
    /// </summary>
    public Bone FindBone(string name)
    {
        foreach (var bone in _bones)
        {
            if (bone.Name == name)
            {
                return bone;
            }
        }

        return null;
    }

    
}