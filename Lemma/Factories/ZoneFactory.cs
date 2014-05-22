﻿using System; using ComponentBind;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lemma.Components;

namespace Lemma.Factories
{
	public class ZoneFactory : Factory<Main>
	{
		public ZoneFactory()
		{
			this.Color = new Vector3(1.0f, 0.7f, 0.4f);
		}

		public override Entity Create(Main main)
		{
			return new Entity(main, "Zone");
		}

		public override void Bind(Entity entity, Main main, bool creating = false)
		{
			entity.CannotSuspend = true;
			entity.CannotSuspendByDistance = true;
			Zone zone = entity.GetOrCreate<Zone>("Zone");
			Transform transform = entity.GetOrCreate<Transform>("Transform");
			zone.Add(new Binding<Matrix>(zone.Transform, transform.Matrix));
			this.SetMain(entity, main);
		}

		private Transform addCornerModel(Entity entity, Property<bool> selected)
		{
			Transform transform = new Transform { Serialize = false, Editable = false };
			entity.AddWithoutOverwriting(transform);

			Model cornerModel1 = new Model();
			cornerModel1.Filename.Value = "Models\\sphere";
			cornerModel1.Color.Value = this.Color;
			cornerModel1.Scale.Value = new Vector3(0.5f);
			cornerModel1.Editable = false;
			cornerModel1.Serialize = false;
			entity.Add(cornerModel1);

			cornerModel1.Add(new Binding<Matrix, Vector3>(cornerModel1.Transform, x => Matrix.CreateTranslation(x), transform.Position));
			cornerModel1.Add(new Binding<bool>(cornerModel1.Enabled, selected));

			return transform;
		}

		public override void AttachEditorComponents(Entity entity, Main main)
		{
			Transform transform = entity.Get<Transform>();

			Property<bool> selected = entity.GetOrMakeProperty<bool>("EditorSelected");
			selected.Serialize = false;

			Zone zone = entity.Get<Zone>();

			EntityConnectable.AttachEditorComponents(entity, zone.ConnectedEntities);

			zone.Add(new CommandBinding<Entity>(entity.GetCommand<Entity>("ToggleEntityConnected"), delegate(Entity other)
			{
				if (zone.ConnectedEntities.Contains(other))
				{
					Zone z = other.Get<Zone>();
					if (z != null)
						z.Parent.Value = null;
				}
				else
				{
					Zone z = other.Get<Zone>();
					if (z != null)
						z.Parent.Value = entity;
				}
			}));

			Model model = new Model();
			model.Filename.Value = "Models\\sphere";
			model.Color.Value = this.Color;
			model.Scale.Value = new Vector3(0.5f);
			model.Editable = false;
			model.Serialize = false;
			entity.Add("EditorModel", model);
			model.Add(new Binding<Matrix, Vector3>(model.Transform, x => Matrix.CreateTranslation(x), transform.Position));

			Property<Vector3> corner1 = new Property<Vector3> { Editable = false, Serialize = false, Value = zone.BoundingBox.Value.Min };
			Property<Vector3> corner2 = new Property<Vector3> { Editable = false, Serialize = false, Value = zone.BoundingBox.Value.Max };

			entity.Add(new Binding<BoundingBox>(zone.BoundingBox, delegate()
			{
				Vector3 a = corner1, b = corner2;
				return new BoundingBox(new Vector3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z)), new Vector3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z)));
			}, corner1, corner2));

			Transform cornerTransform1 = this.addCornerModel(entity, selected);
			cornerTransform1.Add(new TwoWayBinding<Vector3, Vector3>
			(
				corner1,
				x => Vector3.Transform(x, Matrix.Invert(transform.Matrix)),
				new[] { transform.Matrix },
				cornerTransform1.Position,
				x => Vector3.Transform(x, transform.Matrix),
				new[] { transform.Matrix }
			));

			Transform cornerTransform2 = this.addCornerModel(entity, selected);
			cornerTransform2.Add(new TwoWayBinding<Vector3, Vector3>
			(
				corner2,
				x => Vector3.Transform(x, Matrix.Invert(transform.Matrix)),
				new[] { transform.Matrix },
				cornerTransform2.Position,
				x => Vector3.Transform(x, transform.Matrix),
				new[] { transform.Matrix }
			));

			ModelAlpha box = new ModelAlpha();
			box.Filename.Value = "Models\\alpha-box";
			box.Color.Value = new Vector3(this.Color.X, this.Color.Y, this.Color.Z);
			box.Alpha.Value = 0.125f;
			box.Editable = false;
			box.Serialize = false;
			box.DrawOrder.Value = 11; // In front of water
			box.DisableCulling.Value = true;
			entity.Add(box);
			box.Add(new Binding<Matrix>(box.Transform, delegate()
			{
				BoundingBox b = zone.BoundingBox;
				return Matrix.CreateScale(b.Max - b.Min) * Matrix.CreateTranslation((b.Min + b.Max) * 0.5f) * transform.Matrix;
			}, zone.BoundingBox, transform.Matrix));
			box.Add(new Binding<bool>(box.Enabled, selected));
			box.Add(new Binding<BoundingBox>(box.BoundingBox, zone.BoundingBox));
			box.CullBoundingBox.Value = false;
		}
	}
}
