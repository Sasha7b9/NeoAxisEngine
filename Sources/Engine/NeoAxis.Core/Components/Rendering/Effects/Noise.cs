﻿// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Drawing.Design;
using System.ComponentModel;
using System.Reflection;

namespace NeoAxis
{
	/// <summary>
	/// Rendering effect to show random noise on the screen.
	/// </summary>
	[DefaultOrderOfEffect( 10 )]
	public class Component_RenderingEffect_Noise : Component_RenderingEffect_Simple
	{
		const string shaderDefault = @"Base\Shaders\Effects\Noise_fs.sc";
		const string noiseTextureDefault = @"Base\Images\Noise.png";

		Random random = new Random();

		public Component_RenderingEffect_Noise()
		{
			Shader = shaderDefault;
		}

		/// <summary>
		/// The range of multiply blend mode. 
		/// </summary>
		[Serialize]
		[DefaultValue( "0.8 1.2" )]
		//!!!!редактор
		[Range( 0, 2 )]
		public Reference<Range> Multiply
		{
			get { if( _multiply.BeginGet() ) Multiply = _multiply.Get( this ); return _multiply.value; }
			set { if( _multiply.BeginSet( ref value ) ) { try { MultiplyChanged?.Invoke( this ); } finally { _multiply.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="Multiply"/> property value changes.</summary>
		public event Action<Component_RenderingEffect_Noise> MultiplyChanged;
		ReferenceField<Range> _multiply = new Range( 0.8, 1.2 );

		/// <summary>
		/// The range of addition blend mode. 
		/// </summary>
		[Serialize]
		[DefaultValue( "0 0" )]
		[Range( 0, 1 )]
		public Reference<Range> Add
		{
			get { if( _add.BeginGet() ) Add = _add.Get( this ); return _add.value; }
			set { if( _add.BeginSet( ref value ) ) { try { AddChanged?.Invoke( this ); } finally { _add.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="Add"/> property value changes.</summary>
		public event Action<Component_RenderingEffect_Noise> AddChanged;
		ReferenceField<Range> _add = Range.Zero;


		//!!!!
		//double Seed;
		//double SeedRandomUpdateTime;

		/// <summary>
		/// If active, random noise will be generated.
		/// </summary>
		[Serialize]
		[DefaultValue( false )]
		public Reference<bool> SeedRandom
		{
			get { if( _seedRandom.BeginGet() ) SeedRandom = _seedRandom.Get( this ); return _seedRandom.value; }
			set { if( _seedRandom.BeginSet( ref value ) ) { try { SeedRandomChanged?.Invoke( this ); } finally { _seedRandom.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="SeedRandom"/> property value changes.</summary>
		public event Action<Component_RenderingEffect_Noise> SeedRandomChanged;
		ReferenceField<bool> _seedRandom;


		protected override void OnSetShaderParameters( ViewportRenderingContext context, Component_RenderingPipeline.IFrameData frameData, Component_Image actualTexture, CanvasRenderer.ShaderItem shader )
		{
			base.OnSetShaderParameters( context, frameData, actualTexture, shader );

			var noiseTexture = ResourceManager.LoadResource<Component_Image>( noiseTextureDefault );
			if( noiseTexture == null )
				return;

			GpuTexture gpuNoiseTexture = noiseTexture.Result;// ResourceUtility.GetTextureCompiledData( noiseTexture );

			shader.Parameters.Set( new ViewportRenderingContext.BindTextureData( 1/*"noiseTexture"*/,
				noiseTexture, TextureAddressingMode.Wrap, FilterOption.Point, FilterOption.Point, FilterOption.Point ) );
			//shader.Parameters.Set( "1"/*"noiseTexture"*/, new GpuMaterialPass.TextureParameterValue( noiseTexture,
			//	TextureAddressingMode.Wrap, FilterOption.Point, FilterOption.Point, FilterOption.Point ) );

			{
				var size = actualTexture.Result.ResultSize;
				shader.Parameters.Set( "viewportSize", new Vector4( size.X, size.Y, 1.0 / (double)size.X, 1.0 / (double)size.Y ).ToVector4F() );
			}

			{
				var size = gpuNoiseTexture.ResultSize;
				shader.Parameters.Set( "noiseTextureSize", new Vector4( size.X, size.Y, 1.0 / (double)size.X, 1.0 / (double)size.Y ).ToVector4F() );
			}

			bool r = SeedRandom;

			Vector4F seeds = Vector4F.Zero;
			if( r )
				seeds = new Vector4F( random.NextFloat(), random.NextFloat(), random.NextFloat(), random.NextFloat() );
			shader.Parameters.Set( "seeds", seeds );
		}

		public override bool LimitedDevicesSupport
		{
			get { return true; }
		}

	}
}