﻿using System; using ComponentBind;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lemma.Components;

namespace Lemma.Factories
{
	public class PropFactory : Factory<Main>
	{
		public PropFactory()
		{
			this.Color = new Vector3(1.0f, 1.0f, 0.7f);
		}

		public override Entity Create(Main main)
		{
			return new Entity(main, "Prop");
		}

		public override void Bind(Entity entity, Main main, bool creating = false)
		{
			Model model = entity.GetOrCreate<Model>("Model");
			model.MapContent.Value = true;
			this.SetMain(entity, main);
			Transform transform = entity.GetOrCreate<Transform>("Transform");
			model.Add(new Binding<Matrix>(model.Transform, transform.Matrix));

			if (entity.GetOrMakeProperty<bool>("Attach", true))
				MapAttachable.MakeAttachable(entity, main);
		}

		public override void AttachEditorComponents(Entity entity, Main main)
		{
			base.AttachEditorComponents(entity, main);
			Model model = entity.Get<Model>("Model");
			Model editorModel = entity.Get<Model>("EditorModel");
			Property<bool> editorSelected = entity.GetOrMakeProperty<bool>("EditorSelected", false);
			editorSelected.Serialize = false;
			editorModel.Add(new Binding<bool>(editorModel.Enabled, () => !editorSelected || !model.IsValid, editorSelected, model.IsValid));

			MapAttachable.AttachEditorComponents(entity, main, editorModel.Color);
		}
	}

	public class PropAlphaFactory : PropFactory
	{
		public override Entity Create(Main main)
		{
			Entity entity = new Entity(main, "PropAlpha");
			ModelAlpha model = new ModelAlpha();
			model.DrawOrder.Value = 11;
			entity.Add("Model", model);

			return entity;
		}
	}
}
