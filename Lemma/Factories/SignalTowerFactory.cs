﻿using System; using ComponentBind;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lemma.Components;

namespace Lemma.Factories
{
	public class SignalTowerFactory : Factory<Main>
	{
		private Random random = new Random();

		public SignalTowerFactory()
		{
			this.Color = new Vector3(0.4f, 0.4f, 0.4f);
		}

		public override Entity Create(Main main)
		{
			return new Entity(main, "SignalTower");
		}

		public override void Bind(Entity entity, Main main, bool creating = false)
		{
			PlayerTrigger trigger = entity.GetOrCreate<PlayerTrigger>("PlayerTrigger");
			this.SetMain(entity, main);
			Transform transform = entity.GetOrCreate<Transform>("Transform");

			if (entity.GetOrMakeProperty<bool>("Attach", true))
				MapAttachable.MakeAttachable(entity, main);
			Property<float> attachOffset = entity.GetOrMakeProperty<float>("AttachmentOffset", true);
			
			trigger.Editable = true;
			trigger.Add(new Binding<Vector3>(trigger.Position, () => Vector3.Transform(new Vector3(0.0f, 0.0f, attachOffset), transform.Matrix), attachOffset, transform.Matrix));

			PointLight light = entity.GetOrCreate<PointLight>();
			light.Add(new Binding<Vector3>(light.Position, trigger.Position));
			light.Color.Value = new Vector3(1.0f, 0.5f, 1.7f);

			Property<float> lightBaseRadius = new Property<float> { Value = 10.0f };

			Updater updater = new Updater
			{
				delegate(float dt)
				{
					light.Attenuation.Value = lightBaseRadius.Value * (1.0f + (((float)this.random.NextDouble() - 0.5f) * 0.1f));
				}
			};
			updater.EnabledInEditMode = true;
			entity.Add(updater);

			SignalTower tower = entity.GetOrCreate<SignalTower>("SignalTower");
			tower.Add(new CommandBinding<Entity>(trigger.PlayerEntered, tower.PlayerEnteredRange));
			tower.Add(new CommandBinding<Entity>(trigger.PlayerExited, tower.PlayerExitedRange));
			tower.Add(new Binding<Entity.Handle>(tower.Player, trigger.Player));

			if (!main.EditorEnabled)
				AkSoundEngine.PostEvent("Signal_tower_loop", entity);
			
			SoundKiller.Add(entity, "Stop_signal_tower_loop");

			AkGameObjectTracker.Attach(entity, trigger.Position);

			ParticleEmitter distortionEmitter = entity.GetOrCreate<ParticleEmitter>("DistortionEmitter");
			distortionEmitter.Serialize = false;
			distortionEmitter.Add(new Binding<Vector3>(distortionEmitter.Position, trigger.Position));
			distortionEmitter.ParticleType.Value = "Distortion";
			distortionEmitter.ParticlesPerSecond.Value = 4;
			distortionEmitter.Jitter.Value = new Vector3(0.5f);

			ParticleEmitter purpleEmitter = entity.GetOrCreate<ParticleEmitter>("PurpleEmitter");
			purpleEmitter.Serialize = false;
			purpleEmitter.Add(new Binding<Vector3>(purpleEmitter.Position, trigger.Position));
			purpleEmitter.ParticleType.Value = "Purple";
			purpleEmitter.ParticlesPerSecond.Value = 30;
			purpleEmitter.Jitter.Value = new Vector3(0.5f);

			Animation enterAnimation = null;
			trigger.Add(new CommandBinding<Entity>(trigger.PlayerEntered, delegate(Entity p)
			{
				if (enterAnimation == null || !enterAnimation.Active)
				{
					AkSoundEngine.PostEvent("Signal_tower_activate", entity);
					enterAnimation = new Animation
					(
						new Animation.FloatMoveTo(lightBaseRadius, 20.0f, 0.25f),
						new Animation.FloatMoveTo(lightBaseRadius, 10.0f, 0.5f)
					);
					entity.Add(enterAnimation);
				}
			}));
		}

		public override void AttachEditorComponents(Entity entity, Main main)
		{
			base.AttachEditorComponents(entity, main);

			MapAttachable.AttachEditorComponents(entity, main, entity.Get<Model>().Color);
			PlayerTrigger.AttachEditorComponents(entity, main, entity.Get<Model>().Color);
		}
	}
}
